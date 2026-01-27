using StudentPayments_API.Models;
using StudentPayments_API.DTOs.Responses;
namespace StudentPayments_API.Security.Interfaces;

public interface ITokenService
{
    //Return 2 values: the generated token string and its expiration time
    TokenResponseDto GenerateToken(Student student);
}
//Return type: TokenResponseDto
//Method: GenerateToken
//Parameter: Student student