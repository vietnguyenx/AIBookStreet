using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.UnitOfWork.Interface
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IAuthorRepository AuthorRepository { get; }
        IBookAuthorRepository BookAuthorRepository { get; }
        IBookCategoryRepository BookCategoryRepository { get; }
        IBookRepository BookRepository { get; }
        IBookStoreRepository BookStoreRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IEventRepository EventRepository { get; }
        IInventoryRepository InventoryRepository { get; }
        IPublisherRepository PublisherRepository { get; }
        IRoleRepository RoleRepository { get; }
        IStreetRepository StreetRepository { get; }
        IUserRepository UserRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        IZoneRepository ZoneRepository { get; }
        IImageRepository ImageRepository { get; }
    }
}
