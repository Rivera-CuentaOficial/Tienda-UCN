using Microsoft.EntityFrameworkCore;
using TiendaUCN.Domain.Models;
using TiendaUCN.Infrastructure.Data;

namespace TiendaUCN.Infrastructure.Repositories.Interfaces;

public class VerificationCodeRepository : IVerificationCodeRepository
{
    private readonly DataContext _dataContext;

    public VerificationCodeRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<VerificationCode> CreateAsync(VerificationCode verificationCode)
    {
        await _dataContext.VerificationCodes.AddAsync(verificationCode);
        await _dataContext.SaveChangesAsync();
        return verificationCode;
    }

    public async Task<VerificationCode> UpdateAsync(VerificationCode verificationCode)
    {
        await _dataContext
            .VerificationCodes.Where(vc => vc.Id == verificationCode.Id)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(vc => vc.Code, verificationCode.Code)
                    .SetProperty(vc => vc.Expiration, verificationCode.Expiration)
                    .SetProperty(vc => vc.AttemptCount, verificationCode.AttemptCount)
            );
        var newVerificationCode = await _dataContext
            .VerificationCodes.AsNoTracking()
            .FirstOrDefaultAsync(vc => vc.Id == verificationCode.Id);
        return newVerificationCode!;
    }

    public async Task<VerificationCode> GetByLatestUserIdAsync(int userId, CodeType codeType)
    {
        var verificationCode = await _dataContext
            .VerificationCodes.Where(vc => vc.UserId == userId && vc.Type == codeType)
            .OrderByDescending(vc => vc.CreatedAt)
            .FirstOrDefaultAsync();
        return verificationCode!;
    }

    public async Task<int> IncreaseAttemptsAsync(int userId, CodeType codeType)
    {
        await _dataContext
            .VerificationCodes.Where(vc => vc.UserId == userId && vc.Type == codeType)
            .OrderByDescending(vc => vc.CreatedAt)
            .ExecuteUpdateAsync(vc => vc.SetProperty(v => v.AttemptCount, v => v.AttemptCount + 1));
        return await _dataContext
            .VerificationCodes.Where(vc => vc.UserId == userId && vc.Type == codeType)
            .OrderByDescending(vc => vc.CreatedAt)
            .Select(vc => vc.AttemptCount)
            .FirstAsync();
    }

    public async Task<bool> DeleteByUserIdAsync(int userId, CodeType codeType)
    {
        await _dataContext
            .VerificationCodes.Where(vc => vc.UserId == userId && vc.Type == codeType)
            .ExecuteDeleteAsync();
        var exists = await _dataContext.VerificationCodes.AnyAsync(vc =>
            vc.UserId == userId && vc.Type == codeType
        );
        return !exists;
    }

    public async Task<VerificationCode> GetLatestByUserIdAsync(int userId, CodeType codeType)
    {
        var verificationCode = await _dataContext
            .VerificationCodes.Where(vc => vc.UserId == userId && vc.Type == codeType)
            .OrderByDescending(vc => vc.CreatedAt)
            .FirstOrDefaultAsync();
        return verificationCode!;
    }
}