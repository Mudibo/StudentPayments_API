
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

public class BankClientService : IBankClientService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<BankClientService> _logger;
    private readonly ITokenService _tokenService;
    public BankClientService(StudentPaymentsDbContext context, ILogger<BankClientService> logger, ITokenService tokenService)
    {
        _context = context;
        _logger = logger;
        _tokenService = tokenService;
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
                    Message = "A bank client with the same Client ID already exists."
                };
            } 
            var secretHash = BCrypt.Net.BCrypt.HashPassword(dto.ClientSecret.Trim());
            var bankClient = new BankClient
            {
                ClientId = dto.ClientId.Trim(),
                ClientSecretHash = secretHash,
                BankName = dto.BankName.Trim(),
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
            _logger.LogError(dbEx, "Database error while creating bank client with BankName: {BankName}", dto.BankName.Trim());
            return new AddBankClientResponseDto
            {
                Success = false,
                Message = "A database error occurred while creating the bank client.",
                BankName = null,
                IsActive = false
            };
        } 
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating bank client with BankName: {BankName}", dto.BankName.Trim());
            return new AddBankClientResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred while creating the bank client.",
                BankName = null,
                IsActive = false
            };
        }     
        }
        
        public async Task<OAuthTokenResponseDto> AuthenticateOAuthClientAsync(OAuthClientAuthRequestDto dto){
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
            //Validate requested scopes against allowed scopes
            var allowedScopes = OAuthScopes.All;
            var requestedScopes = dto.Scope?.Split(' ') ?? Array.Empty<string>();
            if(requestedScopes.Any(s => !allowedScopes.Contains(s)))
            {
                _logger.LogWarning("OAuth authentication failed for ClientId: {ClientId} - invalid scope requested: {Scope}", dto.ClientId.Trim(), dto.Scope);
                throw new OAuthException(OAuthErrorEnum.InvalidScope, "Invalid scope");
            }
            var token = _tokenService.GenerateOAuthToken(
                client.ClientId,
                requestedScopes
            );
            return new OAuthTokenResponseDto
            {
                access_token = token.Token,
                token_type = "Bearer",
                expires_in = (int)(token.Expiration - DateTime.UtcNow).TotalSeconds,
                scope = string.Join(" ", requestedScopes)
            };
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
 