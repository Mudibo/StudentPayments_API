
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Security.Interfaces;

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
        public async Task<BankClientAuthResponseDto> AuthenticateBankClientAsync(BankClientAuthRequestDto dto)
        {
        try
        {
            var bankClient = await _context.BankClients.FirstOrDefaultAsync(bc => bc.ClientId == dto.ClientId.Trim() && bc.IsActive);
            if (bankClient == null)
            {
                return new BankClientAuthResponseDto
                {
                    Success = false,
                    Message = "Invalid Client ID or inactive Bank Client.",
                    AccessToken = null, 
                    BankName = null
                };
            }
            bool isSecretValid = BCrypt.Net.BCrypt.Verify(dto.ClientSecret.Trim(), bankClient.ClientSecretHash);
            if (!isSecretValid)
            {
                return new BankClientAuthResponseDto
                {
                    Success = false,
                    Message = "Invalid Client Secret",
                    AccessToken = null,
                    BankName = null
                };
            }
            var tokenResponse = _tokenService.GenerateBankClientToken(bankClient);
            return new BankClientAuthResponseDto
            {
                Success = true,
                Message = "Authentication Successful",
                AccessToken = tokenResponse.Token,
                ExpiresAt = tokenResponse.Expiration,
                BankName = bankClient.BankName,  
            };
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while authenticating bank client with ClientId: {ClientId}", dto.ClientId.Trim());
            return new BankClientAuthResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred while authenticating the bank client",
                AccessToken = null,
                BankName = null
            };
        }
        }
    }
 