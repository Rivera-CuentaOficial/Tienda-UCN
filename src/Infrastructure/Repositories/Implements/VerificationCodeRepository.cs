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
}