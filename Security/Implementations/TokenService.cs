using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentPayments_API.Models;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Security.Interfaces;

namespace StudentPayments_API.Security.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config; //Hold application configuration
    public TokenService(IConfiguration config)
    {
        _config = config;
    }
    public TokenResponseDto GenerateToken(Student student)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:TokenLifetimeMinutes"])); //Token expiration time

        var claims = new[]
        {
            new Claim("admissionNumber", student.AdmissionNumber),
            new Claim("program", student.Program.ToString()),
            new Claim("mobileNumber", student.MobileNumber)
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new TokenResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            Expiration = expires                        
        };
    }
}