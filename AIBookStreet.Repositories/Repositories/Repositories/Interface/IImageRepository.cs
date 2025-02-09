using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IImageRepository : IBaseRepository<Image>
    {
        Task<Image?> GetByID(Guid? id);
        Task<List<Image>?> GetByTypeAndEntityID(string? type, Guid? entityID);
    }
}
