using AIBookStreet.Services.Common;
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
        private readonly IImageService _imageService;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookRepository = unitOfWork.BookRepository;
            _imageService = imageService;
        }

        public async Task<List<BookModel>> GetAll()
        {
            var books = await _bookRepository.GetAll();
            return !books.Any() ? null : _mapper.Map<List<BookModel>>(books);
        }

        public async Task<List<BookModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = await _bookRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);
            return !books.Any() ? null : _mapper.Map<List<BookModel>>(books);
        }

        public async Task<BookModel?> GetById(Guid id)
        {
            var book = await _bookRepository.GetById(id);
            return book == null ? null : _mapper.Map<BookModel>(book);
        }

        public async Task<(List<BookModel>?, long)> SearchPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = _mapper.Map<Book>(bookModel);
            var (bookList, total) = await _bookRepository.SearchPagination(books, startDate, endDate, pageNumber, pageSize, sortField, sortOrder);
            return !bookList.Any() ? (null, total) : (_mapper.Map<List<BookModel>>(bookList), total);
        }

        public async Task<List<BookModel>?> SearchWithoutPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate)
        {
            var bookEntity = _mapper.Map<Book>(bookModel);
            var books = await _bookRepository.SearchWithoutPagination(bookEntity, startDate, endDate);
            return !books.Any() ? null : _mapper.Map<List<BookModel>>(books);
        }

        public async Task<(bool, string)> Add(BookModel bookModel)
        {
            try
            {
                if (bookModel == null)
                    return (false, ConstantMessage.Book.EmptyInfo);

                if (string.IsNullOrEmpty(bookModel.Code))
                    return (false, ConstantMessage.Book.EmptyCode);

                if (string.IsNullOrEmpty(bookModel.Title))
                    return (false, ConstantMessage.Book.EmptyTitle);

                var existingBook = await _bookRepository.SearchWithoutPagination(new Book { Code = bookModel.Code }, null, null);
                if (existingBook?.Any() == true)
                    return (false, ConstantMessage.Book.CodeExists);

                var mappedBook = _mapper.Map<Book>(bookModel);
                var newBook = await SetBaseEntityToCreateFunc(mappedBook);

                if (bookModel.BookAuthors?.Any() == true)
                {
                    foreach (var author in bookModel.BookAuthors)
                    {
                        if (author.AuthorId == Guid.Empty)
                            return (false, ConstantMessage.Book.InvalidAuthorId);
                    }

                    newBook.BookAuthors = bookModel.BookAuthors.Select(ba => new BookAuthor
                    {
                        Id = Guid.NewGuid(),
                        BookId = newBook.Id,
                        AuthorId = ba.AuthorId
                    }).ToList();
                }

                if (bookModel.BookCategories?.Any() == true)
                {
                    foreach (var category in bookModel.BookCategories)
                    {
                        if (category.CategoryId == Guid.Empty)
                            return (false, ConstantMessage.Book.InvalidCategoryId);
                    }

                    newBook.BookCategories = bookModel.BookCategories.Select(bc => new BookCategory
                    {
                        Id = Guid.NewGuid(),
                        BookId = newBook.Id,
                        CategoryId = bc.CategoryId
                    }).ToList();
                }

                if (bookModel.MainImageFile != null)
                {
                    if (bookModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (false, ConstantMessage.Book.MainImageSizeExceeded);

                    if (!bookModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (false, ConstantMessage.Book.InvalidMainImageFormat);

                    var mainImageModel = new FileModel
                    {
                        File = bookModel.MainImageFile,
                        Type = "book_main",
                        AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                        EntityId = newBook.Id
                    };

                    var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                    if (mainImages == null || !mainImages.Any())
                        return (false, ConstantMessage.Book.MainImageUploadFailed);

                    newBook.BaseImgUrl = mainImages.First().Url;
                }

                if (bookModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in bookModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (false, ConstantMessage.Book.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (false, ConstantMessage.Book.InvalidSubImageFormat);
                    }

                    var additionalImageModels = bookModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "book_additional",
                        AltText = bookModel.Title ?? file.FileName,
                        EntityId = newBook.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (false, ConstantMessage.Book.SubImageUploadFailed);
                }

                var result = await _bookRepository.Add(newBook);
                return result ? (true, ConstantMessage.Book.AddSuccess) : (false, ConstantMessage.Book.AddFail);
            }
            catch (Exception ex)
            {
                return (false, $"Error while adding book: {ex.Message}");
            }
        }

        public async Task<(bool, string)> Update(BookModel bookModel)
        {
            try
            {
                if (bookModel == null)
                    return (false, ConstantMessage.Book.EmptyInfo);

                if (bookModel.Id == Guid.Empty)
                    return (false, ConstantMessage.EmptyId);

                var existingBook = await _bookRepository.GetById(bookModel.Id);
                if (existingBook == null)
                    return (false, ConstantMessage.Book.NotFoundForUpdate);

                if (!string.IsNullOrEmpty(bookModel.Code) && bookModel.Code != existingBook.Code)
                {
                    var bookWithSameCode = await _bookRepository.SearchWithoutPagination(new Book { Code = bookModel.Code }, null, null);
                    if (bookWithSameCode?.Any() == true)
                        return (false, ConstantMessage.Book.CodeExists);
                }

                if (string.IsNullOrEmpty(bookModel.Code))
                    bookModel.Code = existingBook.Code;

                _mapper.Map(bookModel, existingBook);
                var updatedBook = await SetBaseEntityToUpdateFunc(existingBook);

                if (bookModel.MainImageFile != null)
                {
                    if (bookModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (false, ConstantMessage.Book.MainImageSizeExceeded);

                    if (!bookModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (false, ConstantMessage.Book.InvalidMainImageFormat);

                    var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("book_main", updatedBook.Id);
                    if (existingMainImages?.Any() == true)
                    {
                        var mainImageModel = new FileModel
                        {
                            File = bookModel.MainImageFile,
                            Type = "book_main",
                            AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                            EntityId = updatedBook.Id
                        };

                        var updateResult = await _imageService.UpdateAnImage(existingMainImages.First().Id, mainImageModel);
                        if (updateResult.Item1 != 2)
                            return (false, ConstantMessage.Book.MainImageUploadFailed);

                        updatedBook.BaseImgUrl = updateResult.Item2.Url;
                    }
                    else
                    {
                        var mainImageModel = new FileModel
                        {
                            File = bookModel.MainImageFile,
                            Type = "book_main",
                            AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                            EntityId = updatedBook.Id
                        };

                        var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                        if (mainImages == null)
                            return (false, ConstantMessage.Book.MainImageUploadFailed);

                        updatedBook.BaseImgUrl = mainImages.First().Url;
                    }
                }

                if (bookModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in bookModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (false, ConstantMessage.Book.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (false, ConstantMessage.Book.InvalidSubImageFormat);
                    }

                    var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", updatedBook.Id);
                    if (existingAdditionalImages?.Any() == true)
                    {
                        foreach (var image in existingAdditionalImages)
                        {
                            var deleteResult = await _imageService.DeleteAnImage(image.Id);
                            if (deleteResult.Item1 != 2)
                                return (false, ConstantMessage.Book.SubImageUploadFailed);
                        }
                    }

                    var additionalImageModels = bookModel.AdditionalImageFiles.Select(file => new FileModel
                    {
                        File = file,
                        Type = "book_additional",
                        AltText = bookModel.Title ?? file.FileName,
                        EntityId = updatedBook.Id
                    }).ToList();

                    var additionalImages = await _imageService.AddImages(additionalImageModels);
                    if (additionalImages == null)
                        return (false, ConstantMessage.Book.SubImageUploadFailed);
                }

                var result = await _bookRepository.Update(updatedBook);
                return result ? (true, ConstantMessage.Book.UpdateSuccess) : (false, ConstantMessage.Book.UpdateFail);
            }
            catch (Exception ex)
            {
                return (false, $"Error while updating book: {ex.Message}");
            }
        }

        public async Task<(bool, string)> Delete(Guid bookId)
        {
            try
            {
                if (bookId == Guid.Empty)
                    return (false, ConstantMessage.EmptyId);

                var existingBook = await _bookRepository.GetById(bookId);
                if (existingBook == null)
                    return (false, ConstantMessage.Book.NotFoundForDelete);

                var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("book_main", bookId);
                var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", bookId);

                if (existingMainImages != null)
                {
                    foreach (var image in existingMainImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (false, ConstantMessage.Book.MainImageUploadFailed);
                    }
                }

                if (existingAdditionalImages != null)
                {
                    foreach (var image in existingAdditionalImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (false, ConstantMessage.Book.SubImageUploadFailed);
                    }
                }

                var result = await _bookRepository.Delete(existingBook);
                return result ? (true, ConstantMessage.Book.DeleteSuccess) : (false, ConstantMessage.Book.DeleteFail);
            }
            catch (Exception ex)
            {
                return (false, $"Error while deleting book: {ex.Message}");
            }
        }

        public async Task<long> GetTotalCount()
        {
            return await _bookRepository.GetTotalCount();
        }
    }
}

