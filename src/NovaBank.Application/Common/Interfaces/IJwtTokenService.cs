namespace NovaBank.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string token, DateTime expiresAt) CreateToken(Guid customerId, string role);
}

