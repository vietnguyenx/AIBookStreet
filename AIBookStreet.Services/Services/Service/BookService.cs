using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookRepository = unitOfWork.BookRepository;
        }

        public async Task<bool> Add(BookModel bookModel)
        {
            var Book = _mapper.Map<Book>(bookModel);
            var book = await SetBaseEntityToCreateFunc(Book);
            return await _bookRepository.Add(book);
        }

        public async Task<bool> Delete(Guid id)
        {
            var entity = await _bookRepository.GetById(id);
            if (entity == null)
            {
                return false;
            }

            var Book = _mapper.Map<Book>(entity);
            return await _bookRepository.Delete(Book);
        }

        public async Task<List<BookModel>> GetAll()
        {
            var Books = await _bookRepository.GetAll();

            if (!Books.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookModel>>(Books);
        }
    }
}
