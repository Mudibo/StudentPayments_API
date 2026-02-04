using System;
using System.Threading.Tasks;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.DTOs.Responses;

public class BankClientService : IBankClientService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<BankClientService> _logger;
    public BankClientService(StudentPaymentsDbContext context, ILogger<BankClientService> logger)
    {
        _context = context;
        _logger = logger;
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
    }
