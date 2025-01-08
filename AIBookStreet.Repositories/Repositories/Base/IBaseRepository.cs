using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Base
{
    public interface IBaseRepository
    {
    }
    public interface IBaseRepository<TEntity> : IBaseRepository
        where TEntity : BaseEntity
    {
        IQueryable<TEntity> GetQueryable(CancellationToken cancellationToken = default);
        IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> predicate);

        Task<long> GetTotalCount();

        Task<IList<TEntity>> GetAll(CancellationToken cancellationToken = default);

        Task<TEntity> GetById(Guid id);

        Task<IList<TEntity>> GetAllById(List<Guid> id);

        Task<bool> Add(TEntity entity);
        Task<bool> AddRange(List<TEntity> entities);
        Task<bool> Update(TEntity entity);
        Task<bool> UpdateRange(List<TEntity> entities);
        Task<bool> Delete(TEntity entity);
        Task<bool> DeleteRange(List<TEntity> entities);
        void CheckCancellationToken(CancellationToken cancellationToken = default);
    }
}
