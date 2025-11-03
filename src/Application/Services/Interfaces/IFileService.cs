using Microsoft.AspNetCore.Http.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IFileService
    {
        Task<bool> UploadAsync(IFormFile file, int productId);
        Task<bool> DeleteAsync(string publicId);
    }
}