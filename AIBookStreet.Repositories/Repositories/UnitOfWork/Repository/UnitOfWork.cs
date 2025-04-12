using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.UnitOfWork.Repository
{
    public class UnitOfWork : BaseUnitOfWork<BSDbContext>, IUnitOfWork
    {
        public UnitOfWork(BSDbContext context, IServiceProvider serviceProvider) : base(context, serviceProvider) 
        {
        }

        public IAuthorRepository AuthorRepository => GetRepository<IAuthorRepository>();
        public IBookAuthorRepository BookAuthorRepository => GetRepository<IBookAuthorRepository>();
        public IBookCategoryRepository BookCategoryRepository => GetRepository<IBookCategoryRepository>();
        public IBookRepository BookRepository => GetRepository<IBookRepository>();
        public IStoreRepository StoreRepository => GetRepository<IStoreRepository>();
        public ICategoryRepository CategoryRepository => GetRepository<ICategoryRepository>();
        public IEventRepository EventRepository => GetRepository<IEventRepository>();
        public IInventoryRepository InventoryRepository => GetRepository<IInventoryRepository>();
        public IPublisherRepository PublisherRepository => GetRepository<IPublisherRepository>();
        public IRoleRepository RoleRepository => GetRepository<IRoleRepository>();
        public IStreetRepository StreetRepository => GetRepository<IStreetRepository>();
        public IUserRepository UserRepository => GetRepository<IUserRepository>();
        public IUserRoleRepository UserRoleRepository => GetRepository<IUserRoleRepository>();
        public IUserStoreRepository UserStoreRepository => GetRepository<IUserStoreRepository>();
        public IZoneRepository ZoneRepository => GetRepository<IZoneRepository>();
        public IImageRepository ImageRepository => GetRepository<IImageRepository>();
        public ISouvenirRepository SouvenirRepository => GetRepository<ISouvenirRepository>();
        public IOrderRepository OrderRepository => GetRepository<IOrderRepository>();
        public IOrderDetailRepository OrderDetailRepository => GetRepository<IOrderDetailRepository>();
        public IStoreScheduleRepository StoreScheduleRepository => GetRepository<IStoreScheduleRepository>();
        public IPersonRepository PersonRepository => GetRepository<IPersonRepository>();
    }
}
