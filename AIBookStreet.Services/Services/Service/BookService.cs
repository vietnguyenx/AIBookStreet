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
        private readonly IGoogleBookService _googleBookService;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService, IGoogleBookService googleBookService) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookRepository = unitOfWork.BookRepository;
            _imageService = imageService;
            _googleBookService = googleBookService;
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

        public async Task<(List<BookModel>?, long)> SearchPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = _mapper.Map<Book>(bookModel);
            var (bookList, total) = await _bookRepository.SearchPagination(books, startDate, endDate, minPrice, maxPrice, pageNumber, pageSize, sortField, sortOrder);
            return !bookList.Any() ? (null, total) : (_mapper.Map<List<BookModel>>(bookList), total);
        }

        public async Task<List<BookModel>?> SearchWithoutPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice)
        {
            var bookEntity = _mapper.Map<Book>(bookModel);
            var books = await _bookRepository.SearchWithoutPagination(bookEntity, startDate, endDate, minPrice, maxPrice);
            return !books.Any() ? null : _mapper.Map<List<BookModel>>(books);
        }

        public async Task<(BookModel?, string)> Add(BookModel bookModel)
        {
            if (bookModel == null || string.IsNullOrWhiteSpace(bookModel.ISBN))
                return (null, ConstantMessage.Book.EmptyCode);

            var existing = await _bookRepository.SearchWithoutPagination(new Book { ISBN = bookModel.ISBN }, null, null, null, null);
            if (existing?.Any() == true)
                return (null, ConstantMessage.Book.CodeExists);

            var googleBook = await _googleBookService.SearchBookByISBN(bookModel.ISBN);
            if (googleBook != null)
            {
                bookModel.Title = bookModel.Title ?? googleBook.Title;
                bookModel.Subtitle = bookModel.Subtitle ?? googleBook.Subtitle;
                bookModel.Description = bookModel.Description ?? googleBook.Description;
                bookModel.Languages = bookModel.Languages ?? googleBook.Languages;
                bookModel.PublicationDate = bookModel.PublicationDate ?? googleBook.PublicationDate;
                bookModel.Size = bookModel.Size ?? googleBook.Size;
                bookModel.PageCount = bookModel.PageCount ?? googleBook.PageCount;
                bookModel.PreviewLink = bookModel.PreviewLink ?? googleBook.PreviewLink;
                bookModel.InfoLink = bookModel.InfoLink ?? googleBook.InfoLink;
                bookModel.Publisher.PublisherName = bookModel.Publisher.PublisherName ?? googleBook.Publisher.PublisherName;

                if (bookModel.BookAuthors == null || !bookModel.BookAuthors.Any())
                    bookModel.BookAuthors = googleBook.BookAuthors;

                if (bookModel.BookCategories == null || !bookModel.BookCategories.Any())
                    bookModel.BookCategories = googleBook.BookCategories;
            }

            if (string.IsNullOrWhiteSpace(bookModel.Title))
                return (null, ConstantMessage.Book.EmptyTitle);

            var bookEntity = _mapper.Map<Book>(bookModel);
            bookEntity = await SetBaseEntityToCreateFunc(bookEntity);

            if (bookModel.BookAuthors?.Any() == true)
            {
                bookEntity.BookAuthors = bookModel.BookAuthors.Select(ba => new BookAuthor
                {
                    Id = Guid.NewGuid(),
                    AuthorId = ba.AuthorId,
                    BookId = bookEntity.Id
                }).ToList();
            }

            if (bookModel.BookCategories?.Any() == true)
            {
                bookEntity.BookCategories = bookModel.BookCategories.Select(bc => new BookCategory
                {
                    Id = Guid.NewGuid(),
                    CategoryId = bc.CategoryId,
                    BookId = bookEntity.Id
                }).ToList();
            }

            if (bookModel.MainImageFile != null)
            {
                var imageModel = new FileModel
                {
                    File = bookModel.MainImageFile,
                    Type = "book_main",
                    AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                    EntityId = bookEntity.Id
                };

                var images = await _imageService.AddImages(new List<FileModel> { imageModel });
                bookEntity.ThumbnailUrl = images?.FirstOrDefault()?.Url;
            }

            if (bookModel.AdditionalImageFiles?.Any() == true)
            {
                var additionalImageModels = bookModel.AdditionalImageFiles.Select(file => new FileModel
                {
                    File = file,
                    Type = "book_additional",
                    AltText = bookModel.Title ?? file.FileName,
                    EntityId = bookEntity.Id
                }).ToList();

                await _imageService.AddImages(additionalImageModels);
            }

            var success = await _bookRepository.Add(bookEntity);
            return success ? (_mapper.Map<BookModel>(bookEntity), ConstantMessage.Common.AddSuccess)
                           : (null, ConstantMessage.Common.AddFail);
        }

        public async Task<(BookModel?, string)> Update(BookModel bookModel)
        {
            try
            {
                if (bookModel == null)
                    return (null, ConstantMessage.Common.EmptyInfo);

                if (bookModel.Id == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingBook = await _bookRepository.GetById(bookModel.Id);
                if (existingBook == null)
                    return (null, ConstantMessage.Common.NotFoundForUpdate);

                // Try to get updated book info from Google Books API
                var googleBook = await _googleBookService.SearchBookByISBN(bookModel.ISBN);
                if (googleBook != null)
                {
                    // Update fields only if they are empty in the current model
                    bookModel.Title = string.IsNullOrEmpty(bookModel.Title) ? googleBook.Title : bookModel.Title;
                    bookModel.Description = string.IsNullOrEmpty(bookModel.Description) ? googleBook.Description : bookModel.Description;
                    bookModel.Languages = string.IsNullOrEmpty(bookModel.Languages) ? googleBook.Languages : bookModel.Languages;
                    bookModel.PublicationDate = bookModel.PublicationDate ?? googleBook.PublicationDate;
                    bookModel.Size = string.IsNullOrEmpty(bookModel.Size) ? googleBook.Size : bookModel.Size;
                }

                if (!string.IsNullOrEmpty(bookModel.ISBN) && bookModel.ISBN != existingBook.ISBN)
                {
                    var bookWithSameCode = await _bookRepository.SearchWithoutPagination(new Book { ISBN = bookModel.ISBN }, null, null, null, null);
                    if (bookWithSameCode?.Any() == true)
                        return (null, ConstantMessage.Book.CodeExists);
                }

                if (string.IsNullOrEmpty(bookModel.ISBN))
                    bookModel.ISBN = existingBook.ISBN;

                _mapper.Map(bookModel, existingBook);
                var updatedBook = await SetBaseEntityToUpdateFunc(existingBook);

                if (bookModel.MainImageFile != null)
                {
                    if (bookModel.MainImageFile.Length > 10 * 1024 * 1024)
                        return (null, ConstantMessage.Image.MainImageSizeExceeded);

                    if (!bookModel.MainImageFile.ContentType.StartsWith("image/"))
                        return (null, ConstantMessage.Image.InvalidMainImageFormat);

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
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedBook.ThumbnailUrl = updateResult.Item2.Url;
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
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedBook.ThumbnailUrl = mainImages.First().Url;
                    }
                }

                if (bookModel.AdditionalImageFiles?.Any() == true)
                {
                    foreach (var file in bookModel.AdditionalImageFiles)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                            return (null, ConstantMessage.Image.SubImageSizeExceeded);

                        if (!file.ContentType.StartsWith("image/"))
                            return (null, ConstantMessage.Image.InvalidSubImageFormat);
                    }

                    var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", updatedBook.Id);
                    if (existingAdditionalImages?.Any() == true)
                    {
                        foreach (var image in existingAdditionalImages)
                        {
                            var deleteResult = await _imageService.DeleteAnImage(image.Id);
                            if (deleteResult.Item1 != 2)
                                return (null, ConstantMessage.Image.SubImageUploadFailed);
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
                        return (null, ConstantMessage.Image.SubImageUploadFailed);
                }

                var result = await _bookRepository.Update(updatedBook);
                if (!result)
                    return (null, ConstantMessage.Common.UpdateFail);

                return (_mapper.Map<BookModel>(updatedBook), ConstantMessage.Common.UpdateSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while updating book: {ex.Message}");
            }
        }

        public async Task<(BookModel?, string)> Delete(Guid bookId)
        {
            try
            {
                if (bookId == Guid.Empty)
                    return (null, ConstantMessage.EmptyId);

                var existingBook = await _bookRepository.GetById(bookId);
                if (existingBook == null)
                    return (null, ConstantMessage.Common.NotFoundForDelete);

                var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("book_main", bookId);
                var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", bookId);

                if (existingMainImages != null)
                {
                    foreach (var image in existingMainImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (null, ConstantMessage.Image.MainImageUploadFailed);
                    }
                }

                if (existingAdditionalImages != null)
                {
                    foreach (var image in existingAdditionalImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2)
                            return (null, ConstantMessage.Image.SubImageUploadFailed);
                    }
                }

                var result = await _bookRepository.Delete(existingBook);
                if (!result)
                    return (null, ConstantMessage.Common.DeleteFail);

                return (_mapper.Map<BookModel>(existingBook), ConstantMessage.Common.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return (null, $"Error while deleting book: {ex.Message}");
            }
        }

        public async Task<long> GetTotalCount()
        {
            return await _bookRepository.GetTotalCount();
        }
    }
}

