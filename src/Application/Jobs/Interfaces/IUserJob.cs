using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Application.Jobs.Interfaces
{
    public interface IUserJob
    {
        Task DeleteUnconfirmedAsync();
    }
}