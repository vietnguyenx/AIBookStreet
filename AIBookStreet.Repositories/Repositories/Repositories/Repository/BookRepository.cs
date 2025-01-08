using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        private readonly BSDbContext _context;

        public BookRepository(BSDbContext context) : base(context) 
        {
            _context = context;
        }
    }
}
