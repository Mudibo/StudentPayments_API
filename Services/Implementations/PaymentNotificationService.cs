using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StudentPayments_API.Data;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Utils;
using System;
using System.Threading.Tasks;

namespace StudentPayments_API.Services.Implementations;

public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<PaymentNotificationService> _logger;
    private readonly IDistributedCache _cache;

    public PaymentNotificationService(StudentPaymentsDbContext context, ILogger<PaymentNotificationService> logger, IDistributedCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }
    public async Task<PaymentNotificationResponseDto> ProcessNotificationAsync(PaymentNotificationRequestDto dto, string idempotencyKey, string clientId)
    {
        //Retrieve the bank_client_id from cache
        var cacheKey = $"oauth:clientid-to-bankclientid:{clientId}";
        int? bankClientId = null;
        try
        {
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached) && int.TryParse(cached, out var cachedId))
            {
                bankClientId = cachedId;
                _logger.LogInformation("Cache hit for clientId: {ClientId}, bankClientId: {BankClientId}", clientId, bankClientId);
            }
            else
            {
                _logger.LogInformation("Cache miss for clientId: {ClientId}", clientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing cache for clientId: {ClientId} Exception: {ExceptionType}, StackTrace: {StackTrace}", clientId, ex.GetType().Name, ex.StackTrace);
        }

        //Fallback to DB if not found in the cache
        if (bankClientId == null)
        {
            var bankClient = await _context.BankClients.FirstOrDefaultAsync(b => b.ClientId == clientId);
            if (bankClient == null)
            {
                _logger.LogWarning("Bank Client not found for client_id; {ClientId}", clientId);
                return new PaymentNotificationResponseDto
                {
                    Success = false,
                    Error = OAuthErrorEnum.InvalidClient.ToOAuthErrorString(),
                    Message = "Invalid Bank Client",
                    TransactionUuid = null,
                    Status = null
                };
            }
            bankClientId = bankClient.BankClientId;

            //Cache the bank_client_id for future requests
            try
            {
                await _cache.SetStringAsync(cacheKey, bankClientId.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
                _logger.LogInformation("Caching bankClientId for clientId: {ClientId}", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching bankClientId for clientId: {ClientId} Exception: {ExceptionType}, StackTrace: {StackTrace}", clientId, ex.GetType().Name, ex.StackTrace);
            }
        }

        //Retrieve the studentId from the validation cache
        var studentCacheKey = $"student:validation:{dto.AdmissionNumber.Trim()}";
        int? studentId = null;
        try
        {
            var cachedStudent = await _cache.GetStringAsync(studentCacheKey);
            if (!string.IsNullOrEmpty(cachedStudent))
            {
                var cachedValidation = System.Text.Json.JsonSerializer.Deserialize<StudentValidationResponseDto>(cachedStudent);
                if (cachedValidation != null && cachedValidation.Status == StudentValidationStatus.Valid)
                {
                    studentId = cachedValidation.StudentId;
                    _logger.LogInformation("Cache hit for student Id, AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for Admission Number: {AdmissionNumber} Exception: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, ex.GetType().Name, ex.StackTrace);
        }
        if (studentId == null)
        {
            var studentData = await _context.Students
                .Where(k => k.AdmissionNumber == dto.AdmissionNumber.Trim())
                .Select(s => new
                {
                    s.StudentId
                }).FirstOrDefaultAsync();
            if (studentData == null)
            {
                _logger.LogWarning("Student not found for AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
                return new PaymentNotificationResponseDto
                {
                    Success = false,
                    Error = OAuthErrorEnum.Unauthorized.ToOAuthErrorString(),
                    Message = "Unauthorized: Student not found and thus payment cannot be associated with any student record",
                    TransactionUuid = null,
                    Status = null
                };
            }
            studentId = studentData.StudentId;
        }
        _logger.LogInformation("Hashing the payload for idempotency check for clientId: {ClientId}, studentId: {StudentId}, idempotencyKey: {IdempotencyKey}", clientId, studentId, idempotencyKey);
        //Hash the payload
        var payloadToHash = JsonConvert.SerializeObject(dto);
        var requestHash = HashHelper.ComputeSha256Hash(payloadToHash);

        _logger.LogInformation("Preparing to process payment notification for clientId: {ClientId}, studentId: {StudentId}, idempotencyKey: {IdempotencyKey}", clientId, studentId, idempotencyKey);
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Checking idempotency key for clientId: {ClientId}, idempotencyKey: {IdempotencyKey}", clientId, idempotencyKey);
                //Lookup idempotency key by bank_client_id idempotency_key string
                var existingKey = await _context.IdempotencyKeys.FirstOrDefaultAsync(
                    k => k.BankClientId == bankClientId && k.Key == idempotencyKey
                );
                if (existingKey != null)
                {
                    if (existingKey.RequestHash == requestHash)
                    {
                        var existingTx = await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.IdempotencyKeyId == existingKey.Id);
                        if (existingTx != null)
                        {
                            _logger.LogInformation("Idempotent request matched for clientId: {ClientId}, idempotencyKey: {IdempotencyKey}", clientId, idempotencyKey);
                            return new PaymentNotificationResponseDto
                            {
                                Success = true,
                                Error = OAuthErrorEnum.None.ToOAuthErrorString(),
                                Message = "Payment notification already processed",
                                TransactionUuid = existingTx.InternalReference,
                                Status = existingTx.Status.ToString()
                            };
                        }
                        else
                        {
                            _logger.LogWarning("Idempotency key found but no associated transaction for clientId: {ClientId}, idempotencyKey: {IdempotencyKey}", clientId, idempotencyKey);
                            return new PaymentNotificationResponseDto
                            {
                                Success = false,
                                Error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                                Message = "Inconsistent state: Idempotency key exists without transaction",
                                TransactionUuid = null,
                                Status = null
                            };
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Idempotency key conflict for clientId: {ClientId}, idempotencyKey: {IdempotencyKey}", clientId, idempotencyKey);
                        return new PaymentNotificationResponseDto
                        {
                            Success = false,
                            Error = OAuthErrorEnum.Conflict.ToOAuthErrorString(),
                            Message = "Idempotency key conflict: same key used with different payload",
                            TransactionUuid = null,
                            Status = null
                        };
                    }
                }
                var newKey = new IdempotencyKey
                {
                    BankClientId = bankClientId.Value,
                    Key = idempotencyKey,
                    RequestHash = requestHash,
                    ResourceType = IdempotencyResourceTypeEnum.PaymentTransaction,
                };
                _logger.LogInformation("ResourceType value being saved: {ResourceType}", newKey.ResourceType);
                _context.IdempotencyKeys.Add(newKey);
                await _context.SaveChangesAsync();

                var existingBankRef = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(t => t.BankReference == dto.BankReference && t.BankClientId == bankClientId.Value);
                if (existingBankRef != null)
                {
                    var sanitizedBankReference = dto.BankReference?.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    _logger.LogWarning("Duplicate bank reference detected for clientId: {ClientId}, bankReference: {BankReference}", clientId, sanitizedBankReference);
                    return new PaymentNotificationResponseDto
                    {
                        Success = false,
                        Error = OAuthErrorEnum.Conflict.ToOAuthErrorString(),
                        Message = "Duplicate bank reference: a transaction with the same bank reference already exists",
                        TransactionUuid = existingBankRef.InternalReference,
                        Status = existingBankRef.Status.ToString()
                    };
                }
                //Insert payment transaction
                var paymentTx = new PaymentTransaction
                {
                    BankClientId = bankClientId.Value,
                    BankReference = dto.BankReference,
                    Amount = dto.Amount,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    PaymentType = dto.PaymentType,
                    PaymentChannel = dto.PaymentChannel,
                    StudentId = studentId.Value,
                    InternalReference = Guid.NewGuid(),
                    CurrencyType = dto.Currency,
                    IdempotencyKeyId = newKey.Id
                };
                _context.PaymentTransactions.Add(paymentTx);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Committing transaction for clientId: {ClientId}, idempotencyKey: {IdempotencyKey}", clientId, idempotencyKey);
                await transaction.CommitAsync();

                return new PaymentNotificationResponseDto
                {
                    Success = true,
                    Message = "Payment notification processed successfully.",
                    TransactionUuid = paymentTx.InternalReference,
                    Status = paymentTx.Status.ToString()
                };

            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while processing idempotency key for clientId: {ClientId}, idempotencyKey: {IdempotencyKey} Exception: {ExceptionType}, StackTrace: {StackTrace}", clientId, idempotencyKey, dbEx.GetType().Name, dbEx.StackTrace);
                return new PaymentNotificationResponseDto
                {
                    Success = false,
                    Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                    Message = "Database error while processing request",
                    TransactionUuid = null,
                    Status = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing payment notification for clientId: {ClientId}, idempotencyKey: {IdempotencyKey} Exception: {ExceptionType}, StackTrace: {StackTrace}", clientId, idempotencyKey, ex.GetType().Name, ex.StackTrace);
                return new PaymentNotificationResponseDto
                {
                    Success = false,
                    Message = "Unexpected error while processing request",
                    TransactionUuid = null,
                    Status = OAuthErrorEnum.ServerError.ToOAuthErrorString()
                };
            }
        });
    }

    public async Task<PaginatedResultDto<GetStudentPaymentNotificationResponseDto>> GetStudentPaymentNotificationsAsync(GetStudentPaymentsRequestDto dto)
    {
        var studentCacheKey = $"student:validation:{dto.AdmissionNumber.Trim()}";
        int? studentId = null;
        try
        {
            var cachedStudent = await _cache.GetStringAsync(studentCacheKey);
            if (!string.IsNullOrEmpty(cachedStudent))
            {
                var cachedValidation = System.Text.Json.JsonSerializer.Deserialize<StudentValidationResponseDto>(cachedStudent);
                if (cachedValidation != null && cachedValidation.Status == StudentValidationStatus.Valid)
                {
                    studentId = cachedValidation.StudentId;
                    _logger.LogInformation("Cache hit for student Id, AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
                }
            }
            else
            {
                _logger.LogInformation("Cache miss for student Id, AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Cache read failed for Admission Number: {AdmissionNumber} Exception: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, ex.GetType().Name, ex.StackTrace);
        }

        //Fallback to database
        if (studentId == null)
        {
            try
            {
                var studentData = await _context.Students
                    .Where(s => s.AdmissionNumber == dto.AdmissionNumber.Trim())
                    .Select(s => new { s.StudentId })
                    .FirstOrDefaultAsync();
                if (studentData == null)
                {
                    return new PaginatedResultDto<GetStudentPaymentNotificationResponseDto>
                    {
                        TotalCount = 0,
                        Page = dto.Page,
                        PageSize = dto.PageSize,
                        Items = new List<GetStudentPaymentNotificationResponseDto>()
                    };
                }
                studentId = studentData.StudentId;
            }
            catch (TimeoutException tex)
            {
                _logger.LogError(tex, "Database timeout while retrieving student for Admission Number: {AdmissionNumber} Exception: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, tex.GetType().Name, tex.StackTrace);
                return new PaginatedResultDto<GetStudentPaymentNotificationResponseDto>
                {
                    TotalCount = 0,
                    Page = dto.Page,
                    PageSize = dto.PageSize,
                    Error = OAuthErrorEnum.TemporarilyUnavailable,
                    Message = "Database timeout while processing request",
                    Items = new List<GetStudentPaymentNotificationResponseDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving student for Admission Number: {AdmissionNumber} Exception: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, ex.GetType().Name, ex.StackTrace);
                return new PaginatedResultDto<GetStudentPaymentNotificationResponseDto>
                {
                    TotalCount = 0,
                    Page = dto.Page,
                    PageSize = dto.PageSize,
                    Error = OAuthErrorEnum.ServerError,
                    Message = "Unexpected error while processing request",
                    Items = new List<GetStudentPaymentNotificationResponseDto>()
                };
            }
        }

        //Query Payments for a student
        try
        {
            var query = _context.PaymentTransactions
                .Where(pt => pt.StudentId == studentId.Value)
                .OrderByDescending(pt => pt.CreatedAt);

            var totalCount = await query.CountAsync();

            var payments = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(pt => new GetStudentPaymentNotificationResponseDto
                {
                    Amount = pt.Amount,
                    BankReference = pt.BankReference,
                    CreatedAt = pt.CreatedAt,
                    CurrencyType = pt.CurrencyType,
                    PaymentChannel = pt.PaymentChannel,
                    PaymentType = pt.PaymentType,
                    Status = pt.Status,
                    InternalReference = pt.InternalReference
                }).ToListAsync();
            return new PaginatedResultDto<GetStudentPaymentNotificationResponseDto>
            {
                TotalCount = totalCount,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Items = payments
            };
        }
        catch (TimeoutException tex)
        {
            _logger.LogError(tex, "Database timeout while retrieving payments for studentId: {StudentId} Exception: {ExceptionType}, StackTrace: {StackTrace}", studentId, tex.GetType().Name, tex.StackTrace);
            return new PaginatedResultDto<GetStudentPaymentNotificationResponseDto>
            {
                TotalCount = 0,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Error = OAuthErrorEnum.TemporarilyUnavailable,
                Message = "Database timeout while processing request",
                Items = new List<GetStudentPaymentNotificationResponseDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving payments for studentId: {StudentId} Exception: {ExceptionType}, StackTrace: {StackTrace}", studentId, ex.GetType().Name, ex.StackTrace);
            return new PaginatedResultDto<GetStudentPaymentNotificationResponseDto>
            {
                TotalCount = 0,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Error = OAuthErrorEnum.ServerError,
                Message = "Unexpected error while processing request",
                Items = new List<GetStudentPaymentNotificationResponseDto>()
            };
        }
    }
}