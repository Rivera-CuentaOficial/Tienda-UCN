using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<bool?> CreateAsync(Image file);
        Task<bool?> DeleteAsync(string publicId);
    }
}