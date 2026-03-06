
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Caching.Distributed;

namespace StudentPayments_API.Services.Implementations;
public class StudentValidationService : IStudentValidationService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<StudentValidationService> _logger;
    private readonly IDistributedCache _cache;

    //Inject the DbContext for database access
    //Constructor receives the DbContext and stores it
    public StudentValidationService(StudentPaymentsDbContext context, ILogger<StudentValidationService> logger, IDistributedCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<StudentValidationResponseDto> ValidateStudentAsync(StudentValidationRequestDto dto)
    {
        try {
            if (string.IsNullOrWhiteSpace(dto.AdmissionNumber))
            {
                _logger.LogWarning("Admission number is required but was not provided.");
                return new StudentValidationResponseDto {
                    Status = StudentValidationStatus.Error,
                    Message = "Admission number is required",
                    StudentName = null,
                    Program = null,
                    StudentId = null
                };
            }
            string normalizedAdmissionNumber = dto.AdmissionNumber.Trim();
            string cacheKey = $"student:validation:{normalizedAdmissionNumber}";
            StudentValidationResponseDto cachedResponse = null;

            //Try to get from cache
            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if(cached != null)
                {
                    _logger.LogInformation("Cache hit for student validation with AdmissionNumber: {AdmissionNumber}", normalizedAdmissionNumber);
                    cachedResponse = System.Text.Json.JsonSerializer.Deserialize<StudentValidationResponseDto>(cached);
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed for AdmissionNumber: {AdmissionNumber}", normalizedAdmissionNumber, ex.GetType().FullName, ex.StackTrace);
            }
            if(cachedResponse != null)
            {
                return cachedResponse;
            }
            var student = await _context.Students
                .Where(s => s.AdmissionNumber == dto.AdmissionNumber.Trim())
                .Select(s => new
                {
                    s.FirstName,
                    s.AdmissionNumber,
                    s.StudentId,
                    s.LastName,
                    s.Program,
                    s.EnrollmentStatus
                }).FirstOrDefaultAsync();
            StudentValidationResponseDto response;
            if(student == null)
            {
                _logger.LogWarning("Student not found with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
                var notFoundResponse = new StudentValidationResponseDto {
                    Status = StudentValidationStatus.NotFound,
                    Message = "Student not found",
                    StudentName = null,
                    Program = null,
                    StudentId = null
                };
                try
                {
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                    };
                    await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(notFoundResponse), cacheOptions);
                    _logger.LogInformation("Caching not found result for AdmissionNumber: {AdmissionNumber}", normalizedAdmissionNumber);
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Cache write failed for AdmissionNumber: {AdmissionNumber}", normalizedAdmissionNumber, ex.GetType().FullName, ex.StackTrace);
                }
                return notFoundResponse;
            }
            else if (student.EnrollmentStatus != Models.Enums.EnrollmentStatusEnum.Active)
            {
                _logger.LogWarning("Student with AdmissionNumber: {AdmissionNumber} has inactive enrollment status.", dto.AdmissionNumber);
                return new StudentValidationResponseDto {
                    Status = StudentValidationStatus.Inactive,
                    Message = "Student enrollment status is not active",
                    StudentName = null,
                    Program = null,
                    StudentId = null
                };
            }else{
                _logger.LogInformation("Student with AdmissionNumber: {AdmissionNumber} is valid.", dto.AdmissionNumber);
                response = new StudentValidationResponseDto
                {
                    Status = StudentValidationStatus.Valid,
                    AdmissionNumber = student.AdmissionNumber,
                    Message = "Student validated successfully",
                    StudentName = $"{student.FirstName} {student.LastName}",
                    Program = student.Program.ToString(),
                    StudentId = student.StudentId
                };
            }
            //Cache the result
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = response.Status == StudentValidationStatus.Valid 
                        ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(2)
                };
                await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(response), cacheOptions);
                _logger.LogInformation("Caching validation result for AdmissionNumber: {AdmissionNumber} with status: {Status}", normalizedAdmissionNumber, response.Status);
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for AdmissionNumber: {AdmissionNumber}", normalizedAdmissionNumber, ex.GetType().FullName, ex.StackTrace);
            }
            return response;
        }catch(TimeoutException tex)
        {
            _logger.LogError(tex, "A timeout occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber, tex.GetType().FullName, tex.StackTrace);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch(NpgsqlException npex)
        {
            _logger.LogError(npex, "A database error occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber, npex.GetType().FullName, npex.StackTrace);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch (DbUpdateException dbEx){
            _logger.LogError(dbEx, "A database update error occurred while validating student with AdmissionNumber: {AdmissionNumber}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, dbEx.GetType().FullName, dbEx.StackTrace);
            return new StudentValidationResponseDto
            {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch(InvalidOperationException invOpEx){
            _logger.LogError(invOpEx, "A database operation error occurred while validating student with AdmissionNumber: {AdmissionNumber}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, invOpEx.GetType().FullName, invOpEx.StackTrace);
             return new StudentValidationResponseDto
            {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch(Exception ex){
            _logger.LogError(ex, "An error occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber, ex.GetType().FullName, ex.StackTrace);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.Error,
                Message = "An error occurred during student validation",
                StudentName = null,
                Program = null
            };
        }
    }
}