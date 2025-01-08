using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.UnitOfWork.Interface
{
    public interface IBaseUnitOfWork : IDisposable
    {
        IBaseRepository<TEntity> GetRepositoryByEntity<TEntity>() where TEntity : BaseEntity;

        TRepository GetRepository<TRepository>() where TRepository : IBaseRepository;

        Task<bool> SaveChanges(CancellationToken cancellationToken = default);
    }
}
