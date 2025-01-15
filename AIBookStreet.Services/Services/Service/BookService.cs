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
            var books = await _bookRepository.GetAll();

            if (!books.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookModel>>(books);
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
            var book = await _bookRepository.GetById(id);

            if (book == null)
            {
                return null;
            }

            return _mapper.Map<BookModel>(book);
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
            var mappedBook = _mapper.Map<Book>(bookModel);
            var newBook = await SetBaseEntityToCreateFunc(mappedBook);
            return await _bookRepository.Add(newBook);
        }

        public async Task<bool> Update(BookModel bookModel)
        {
            var existingBook = await _bookRepository.GetById(bookModel.Id);

            if (existingBook == null)
            {
                return false;
            }

            _mapper.Map(bookModel, existingBook);
            var updatedBook = await SetBaseEntityToUpdateFunc(existingBook);

            return await _bookRepository.Update(updatedBook);
        }

        public async Task<bool> Delete(Guid bookId)
        {
            var existingBook = await _bookRepository.GetById(bookId);
            if (existingBook == null)
            {
                return false;
            }

            var mappedBook = _mapper.Map<Book>(existingBook);
            return await _bookRepository.Delete(mappedBook);
        }

    }
}
