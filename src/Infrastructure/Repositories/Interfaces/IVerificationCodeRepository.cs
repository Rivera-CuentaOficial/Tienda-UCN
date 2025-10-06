using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces;

public interface IVerificationCodeRepository
{
    Task<VerificationCode> CreateAsync(VerificationCode verificationCode);
    Task<VerificationCode> UpdateAsync(VerificationCode verificationCode);
    Task<VerificationCode> GetByLatestUserIdAsync(int userId, CodeType codeType);
    Task<int> IncreaseAttemptsAsync(int userId, CodeType codeType);
    Task<bool> DeleteByUserIdAsync(int userId, CodeType codeType);
    Task<VerificationCode> GetLatestByUserIdAsync(int userId, CodeType codeType);
    Task<int> DeleteByUserIdAsync(int userId);
}