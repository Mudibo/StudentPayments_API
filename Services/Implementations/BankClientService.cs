
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Security.Interfaces;
using StudentPayments_API.Models.Enums;
using Npgsql;
using StudentPayments_API.Security.OAuthScopes;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

public class BankClientService : IBankClientService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<BankClientService> _logger;
    private readonly ITokenService _tokenService;
    private readonly IDistributedCache _cache;
    public BankClientService(StudentPaymentsDbContext context, ILogger<BankClientService> logger, ITokenService tokenService, IDistributedCache cache)
    {
        _context = context;
        _logger = logger;
        _tokenService = tokenService;
        _cache = cache;
    }
    public async Task<AddBankClientResponseDto> CreateBankClientAsync(CreateBankClientDto dto)
    {
        try
        {
            var ifExists = await _context.BankClients.AnyAsync(bc => bc.ClientId == dto.ClientId.Trim());
            if (ifExists)
            {
                return new AddBankClientResponseDto
                {
                    Success = false,
                    Error = OAuthErrorEnum.Conflict.ToOAuthErrorString(),
                    Message = "A bank client with the same Client ID already exists."
                };
            } 
            var secretHash = BCrypt.Net.BCrypt.HashPassword(dto.ClientSecret.Trim());
            var bankClient = new BankClient
            {
                ClientId = dto.ClientId.Trim(),
                ClientSecretHash = secretHash,
                BankName = dto.BankName.Trim(),
                AllowedScopes = dto.AllowedScopes != null ? string.Join(' ', dto.AllowedScopes) : "",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.BankClients.Add(bankClient);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Bank client created successfully with BankName: {BankName}", dto.BankName.Trim());
            return new AddBankClientResponseDto
            {
                Success = true,
                Message = "Bank client created successfully.",
                BankName = bankClient.BankName,
                IsActive = bankClient.IsActive
            };

        }
        catch(DbUpdateException dbEx)
        {
            _logger.LogError("Database error while creating bank client with BankName: {BankName}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.BankName.Trim(), dbEx.GetType().FullName, dbEx.StackTrace);
            return new AddBankClientResponseDto
            {
                Success = false,
                Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                Message = "A database error occurred while creating the bank client.",
                BankName = null,
                IsActive = false
            };
        } 
        catch(Exception ex)
        {
            _logger.LogError("Unexpected error while creating bank client with BankName: {BankName}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.BankName.Trim(), ex.GetType().FullName, ex.StackTrace);
            return new AddBankClientResponseDto
            {
                Success = false,
                Error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                Message = "An unexpected error occurred while creating the bank client.",
                BankName = null,
                IsActive = false
            };
        }     
        }
        
        public async Task<OAuthTokenResponseDto> AuthenticateOAuthClientAsync(OAuthClientAuthRequestDto dto){
        var cacheKey = $"oauth:{dto.ClientId.Trim()}:{dto.Scope?.Trim()}";
        var clientIdToBankClientIdKey = $"oauth:clientid-to-bankclientid:{dto.ClientId.Trim()}";
        try
        {
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                var cachedToken = JsonConvert.DeserializeObject<CachedOAuthToken>(cached);
                _logger.LogInformation("OAuth token cache hit for ClientId: {ClientId} with Scope: {Scope}", dto.ClientId.Trim(), dto.Scope);
                return new OAuthTokenResponseDto
                {
                    access_token = cachedToken.AccessToken,
                    token_type = cachedToken.TokenType,
                    expires_in = cachedToken.ExpiresIn,
                    scope = cachedToken.Scope
                };
            }else
            {
                _logger.LogInformation("OAuth token cache miss for ClientId: {ClientId} with Scope: {Scope}", dto.ClientId.Trim(), dto.Scope);
            }
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Cache read failed for OAuth token with ClientId: {ClientId} and Scope: {Scope}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.ClientId.Trim(), dto.Scope, ex.GetType().FullName, ex.StackTrace);
        }
        try
        {
            var client = await _context.BankClients.FirstOrDefaultAsync(bc => bc.ClientId == dto.ClientId.Trim() && bc.IsActive);
            if (client == null)
            {
                _logger.LogWarning("OAuth authentication failed for ClientId: {ClientId} - client not found or inactive", dto.ClientId.Trim());
                throw new OAuthException(OAuthErrorEnum.InvalidClient, "Invalid Client Credentials.");
            }
            bool isSecretValid = BCrypt.Net.BCrypt.Verify(dto.ClientSecret.Trim(), client.ClientSecretHash);
            if (!isSecretValid){
                _logger.LogWarning("OAuth authentication failed for ClientId: {ClientId} - invalid secret", dto.ClientId.Trim());
                throw new OAuthException(OAuthErrorEnum.InvalidClient, "Invalid Client Credentials.");
            }
            // Validate requested scopes against allowed scopes using OAuthScopeEnum
            var requestedScopes = dto.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var allowedScopes = client.AllowedScopeList;

            foreach (var reqScope in requestedScopes)
            {
                if (!allowedScopes.Contains(reqScope))
                {
                    _logger.LogWarning("OAuth authentication failed for ClientId: {ClientId} - invalid scope requested: {Scope}", dto.ClientId.Trim(), reqScope);
                    throw new OAuthException(OAuthErrorEnum.InvalidScope, $"Invalid scope: {reqScope}");
                }
            }
            var token = _tokenService.GenerateOAuthToken(
                client.ClientId,
                requestedScopes
            );
            var tokenResponse = new OAuthTokenResponseDto
            {
                access_token = token.Token,
                token_type = "Bearer",
                expires_in = (int)(token.Expiration - DateTime.UtcNow).TotalSeconds,
                scope = string.Join(" ", requestedScopes)
            };
            var cacheObject = new CachedOAuthToken
            {
                AccessToken = tokenResponse.access_token,
                TokenType = tokenResponse.token_type,
                ExpiresIn = tokenResponse.expires_in,
                Scope = tokenResponse.scope,
                BankClientId = client.BankClientId
            };
            try
            {
                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(cacheObject), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.expires_in - 30) // Cache slightly less than token lifetime
                });
                // Also cache clientId-to-bankClientId mapping for PaymentNotificationService
                await _cache.SetStringAsync(
                    clientIdToBankClientIdKey,
                    client.BankClientId.ToString(),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    }
                );
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Cache write failed for OAuth token with ClientId: {ClientId} and Scope: {Scope}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.ClientId.Trim(), dto.Scope, ex.GetType().FullName, ex.StackTrace);
            }
        return tokenResponse;
        }catch(NpgsqlException npgEx)when(npgEx.IsTransient)
        {
            _logger.LogError(npgEx, "Database error while authenticating OAuth client with ClientId: {ClientId}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.ClientId.Trim(), npgEx.GetType().FullName, npgEx.StackTrace);
            throw new OAuthException(OAuthErrorEnum.TemporarilyUnavailable, "Database error occurred. Please try again.");
        }catch(InvalidOperationException invOpEx) when (invOpEx.InnerException is NpgsqlException npgEx && npgEx.IsTransient)
        {
            _logger.LogError(invOpEx, "Database error while authenticating OAuth client with ClientId: {ClientId}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.ClientId.Trim(), invOpEx.GetType().FullName, invOpEx.StackTrace);
            throw new OAuthException(OAuthErrorEnum.TemporarilyUnavailable, "Database error occurred. Please try again.");
        }
        catch (OAuthException)
        {
            throw;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while authenticating OAuth client with ClientId: {ClientId}", dto.ClientId.Trim());
            throw new OAuthException(OAuthErrorEnum.ServerError, "Server error");
        }
    }}
 