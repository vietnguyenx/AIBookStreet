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

        public async Task<List<BookModel>> GetAll()
        {
            var Books = await _bookRepository.GetAll();

            if (!Books.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookModel>>(Books);
        }

        public async Task<List<BookModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = await _bookRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!books.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookModel>>(books);

        }

        public async Task<BookModel?> GetById(Guid id)
        {
            var Book = await _bookRepository.GetById(id);

            if (Book == null)
            {
                return null;
            }

            return _mapper.Map<BookModel>(Book);
        }

        public async Task<(List<BookModel>?, long)> Search(BookModel bookModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = _mapper.Map<Book>(bookModel);
            var booksWithTotalOrigin = await _bookRepository.Search(books, pageNumber, pageSize, sortField, sortOrder);

            if (!booksWithTotalOrigin.Item1.Any())
            {
                return (null, booksWithTotalOrigin.Item2);
            }
            var bookModels = _mapper.Map<List<BookModel>>(booksWithTotalOrigin.Item1);

            return (bookModels, booksWithTotalOrigin.Item2);
        }

        public async Task<bool> Add(BookModel bookModel)
        {
            var Book = _mapper.Map<Book>(bookModel);
            var book = await SetBaseEntityToCreateFunc(Book);
            return await _bookRepository.Add(book);
        }

        public async Task<bool> Update(BookModel bookModel)
        {
            var entity = await _bookRepository.GetById(bookModel.Id);

            if (entity == null)
            {
                return false;
            }

            _mapper.Map(bookModel, entity);
            entity = await SetBaseEntityToUpdateFunc(entity);

            return await _bookRepository.Update(entity);
        }

        public async Task<bool> Delete(Guid id)
        {
            var entity = await _bookRepository.GetById(id);
            if (entity == null)
            {
                return false;
            }

            var book = _mapper.Map<Book>(entity);
            return await _bookRepository.Delete(book);
        }
        
    }
}
