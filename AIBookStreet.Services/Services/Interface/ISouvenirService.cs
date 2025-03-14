using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface ISouvenirService
    {
        Task<Souvenir?> AddASouvenir(SouvenirModel model);
        Task<(long, Souvenir?)> UpdateASouvenir(Guid id, SouvenirModel model);
        Task<(long, Souvenir?)> DeleteASouvenir(Guid id);
        Task<Souvenir?> GetASouvenirById(Guid id);
        Task<(List<Souvenir>?, long)> GetAllSouvenirsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
