using TiendaUCN.Domain.Models;

namespace TiendaUCN.Infrastructure.Repositories.Interfaces;

public interface IVerificationCodeRepository
{
    Task<VerificationCode> CreateAsync(VerificationCode verificationCode);
}