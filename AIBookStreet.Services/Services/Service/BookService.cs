﻿using AIBookStreet.Repositories.Data.Entities;
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
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookRepository = unitOfWork.BookRepository;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
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

        public async Task<(List<BookModel>?, long)> SearchPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = _mapper.Map<Book>(bookModel);
            var booksWithTotalOrigin = await _bookRepository.SearchPagination(books, startDate, endDate, pageNumber, pageSize, sortField, sortOrder);

            if (!booksWithTotalOrigin.Item1.Any())
            {
                return (null, booksWithTotalOrigin.Item2);
            }
            var bookModels = _mapper.Map<List<BookModel>>(booksWithTotalOrigin.Item1);

            return (bookModels, booksWithTotalOrigin.Item2);
        }

        public async Task<List<BookModel>?> SearchWithoutPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate)
        {
            var bookEntity = _mapper.Map<Book>(bookModel);
            var books = await _bookRepository.SearchWithoutPagination(bookEntity, startDate, endDate);

            if (!books.Any())
            {
                return null;
            }

            return _mapper.Map<List<BookModel>>(books);
        }


        public async Task<bool> Add(BookModel bookModel)
        {
            var mappedBook = _mapper.Map<Book>(bookModel);
            var newBook = await SetBaseEntityToCreateFunc(mappedBook);

            if (bookModel.BookAuthors != null && bookModel.BookAuthors.Any())
            {
                newBook.BookAuthors = bookModel.BookAuthors.Select(ba => new BookAuthor
                {
                    Id = Guid.NewGuid(),
                    BookId = newBook.Id,
                    AuthorId = ba.AuthorId
                }).ToList();
            }

            if (bookModel.BookCategories != null && bookModel.BookCategories.Any())
            {
                newBook.BookCategories = bookModel.BookCategories.Select(bc => new BookCategory
                {
                    Id = Guid.NewGuid(),
                    BookId = newBook.Id,
                    CategoryId = bc.CategoryId
                }).ToList();
            }

            // Handle main image upload
            if (bookModel.MainImageFile != null)
            {
                var mainImageModel = new FileModel
                {
                    File = bookModel.MainImageFile,
                    Type = "book_main",
                    AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                    EntityId = newBook.Id
                };

                var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                if (mainImages == null)
                {
                    return false;
                }

                // Set BaseImgUrl to the main image URL
                newBook.BaseImgUrl = mainImages.First().Url;
            }

            // Handle additional images upload
            if (bookModel.AdditionalImageFiles != null && bookModel.AdditionalImageFiles.Any())
            {
                var additionalImageModels = bookModel.AdditionalImageFiles.Select(file => new FileModel
                {
                    File = file,
                    Type = "book_additional",
                    AltText = bookModel.Title ?? file.FileName,
                    EntityId = newBook.Id
                }).ToList();

                var additionalImages = await _imageService.AddImages(additionalImageModels);
                if (additionalImages == null)
                {
                    return false;
                }
            }

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

            // Handle main image update
            if (bookModel.MainImageFile != null)
            {
                // Get existing main image
                var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("book_main", bookModel.Id);
                if (existingMainImages != null && existingMainImages.Any())
                {
                    // Update existing main image
                    var mainImageModel = new FileModel
                    {
                        File = bookModel.MainImageFile,
                        Type = "book_main",
                        AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                        EntityId = bookModel.Id
                    };

                    var updateResult = await _imageService.UpdateAnImage(existingMainImages.First().Id, mainImageModel);
                    if (updateResult.Item1 != 2) // 2 means success
                    {
                        return false;
                    }

                    // Update BaseImgUrl
                    updatedBook.BaseImgUrl = updateResult.Item2.Url;
                }
                else
                {
                    // Add new main image if no existing main image
                    var mainImageModel = new FileModel
                    {
                        File = bookModel.MainImageFile,
                        Type = "book_main",
                        AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                        EntityId = bookModel.Id
                    };

                    var mainImages = await _imageService.AddImages(new List<FileModel> { mainImageModel });
                    if (mainImages == null)
                    {
                        return false;
                    }

                    // Update BaseImgUrl
                    updatedBook.BaseImgUrl = mainImages.First().Url;
                }
            }

            // Handle additional images update
            if (bookModel.AdditionalImageFiles != null && bookModel.AdditionalImageFiles.Any())
            {
                // Get existing additional images
                var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", bookModel.Id);
                if (existingAdditionalImages != null && existingAdditionalImages.Any())
                {
                    // Delete existing additional images
                    foreach (var image in existingAdditionalImages)
                    {
                        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                        if (deleteResult.Item1 != 2) // 2 means success
                        {
                            return false;
                        }
                    }
                }

                // Add new additional images
                var additionalImageModels = bookModel.AdditionalImageFiles.Select(file => new FileModel
                {
                    File = file,
                    Type = "book_additional",
                    AltText = bookModel.Title ?? file.FileName,
                    EntityId = bookModel.Id
                }).ToList();

                var additionalImages = await _imageService.AddImages(additionalImageModels);
                if (additionalImages == null)
                {
                    return false;
                }
            }

            return await _bookRepository.Update(updatedBook);
        }

        public async Task<bool> Delete(Guid bookId)
        {
            var existingBook = await _bookRepository.GetById(bookId);
            if (existingBook == null)
            {
                return false;
            }

            // Delete associated images (both main and additional)
            var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("book_main", bookId);
            var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", bookId);

            if (existingMainImages != null)
            {
                foreach (var image in existingMainImages)
                {
                    var deleteResult = await _imageService.DeleteAnImage(image.Id);
                    if (deleteResult.Item1 != 2) // 2 means success
                    {
                        return false;
                    }
                }
            }

            if (existingAdditionalImages != null)
            {
                foreach (var image in existingAdditionalImages)
                {
                    var deleteResult = await _imageService.DeleteAnImage(image.Id);
                    if (deleteResult.Item1 != 2) // 2 means success
                    {
                        return false;
                    }
                }
            }

            var mappedBook = _mapper.Map<Book>(existingBook);
            return await _bookRepository.Delete(mappedBook);
        }

    }
}
