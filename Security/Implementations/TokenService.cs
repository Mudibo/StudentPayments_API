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

    //Generate token for students and admins
    public TokenResponseDto GenerateToken(Student student)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:TokenLifetimeMinutes"])); //Token expiration time

        var claims = new[]
        {
            new Claim("admissionNumber", student.AdmissionNumber),
            new Claim(ClaimTypes.Role, student.Role)
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
            Expiration = expires,
            Role = student.Role
        };
    }
    //Generate token for bank clients
    public TokenResponseDto GenerateBankClientToken(BankClient bankClient)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:TokenLifetimeMinutes"])); //Token expiration time
        var claims = new[]
        {
            new Claim("client_id", bankClient.ClientId),
            new Claim("bank_name", bankClient.BankName),
            new Claim(ClaimTypes.Role, "BankClient")
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
            Expiration = expires,
            Role = "BankClient"
        };
    }
    public TokenResponseDto GenerateOAuthToken(string clientId, string[] scopes)
    {
        var claims = new List<Claim>
        {
            new Claim("client_id", clientId)
        };
        claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));
        var expires = DateTime.UtcNow.AddMinutes(15);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"])), SecurityAlgorithms.HmacSha256)
        );
        return new TokenResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expires
        };
    }
}