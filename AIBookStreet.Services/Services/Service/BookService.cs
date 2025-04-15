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
using Microsoft.Extensions.Logging;

namespace AIBookStreet.Services.Services.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IImageService _imageService;
        private readonly IGoogleBookService _googleBookService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<BookService> _logger;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IImageService imageService, IGoogleBookService googleBookService, ILogger<BookService> logger) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _bookRepository = unitOfWork.BookRepository;
            _imageService = imageService;
            _googleBookService = googleBookService;
            _httpContextAccessor = httpContextAccessor;
            _imageRepository = unitOfWork.ImageRepository;
            _logger = logger;
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
            if (book == null) return null;
            
            // Basic book information
            var bookModel = new BookModel
            {
                Id = book.Id,
                ISBN = book.ISBN,
                Title = book.Title,
                PublicationDate = book.PublicationDate,
                Price = book.Price,
                Languages = book.Languages,
                Description = book.Description,
                Size = book.Size,
                Status = book.Status,
                PublisherId = book.PublisherId,
                CreatedBy = book.CreatedBy,
                CreatedDate = book.CreatedDate,
                LastUpdatedBy = book.LastUpdatedBy,
                LastUpdatedDate = book.LastUpdatedDate,
                IsDeleted = book.IsDeleted
            };
            
            // Publisher information - simplified to just what's needed
            if (book.Publisher != null)
            {
                bookModel.Publisher = new PublisherModel 
                { 
                    Id = book.Publisher.Id,
                    PublisherName = book.Publisher.PublisherName,
                    Address = book.Publisher.Address,
                    Phone = book.Publisher.Phone,
                    Email = book.Publisher.Email,
                    Description = book.Publisher.Description,
                    Website = book.Publisher.Website
                };
            }
            
            // Add simplified image information - just main data without nested objects
            if (book.Images != null && book.Images.Any())
            {
                bookModel.Images = book.Images.Select(i => new Image
                {
                    Id = i.Id,
                    Url = i.Url,
                    Type = i.Type,
                    AltText = i.AltText,
                    EntityId = i.EntityId
                }).ToList();
            }
            
            // Initialize empty collections
            bookModel.BookAuthors = new List<BookAuthorModel>();
            bookModel.BookCategories = new List<BookCategoryModel>();
            
            // Add author information - simplified
            if (book.BookAuthors != null && book.BookAuthors.Any())
            {
                foreach (var bookAuthor in book.BookAuthors)
                {
                    var author = await _unitOfWork.AuthorRepository.GetById(bookAuthor.AuthorId);
                    if (author != null)
                    {
                        bookModel.BookAuthors.Add(new BookAuthorModel
                        {
                            AuthorId = author.Id,
                            AuthorName = author.AuthorName,
                            BookId = book.Id
                        });
                    }
                }
            }
            
            // Add category information - simplified
            if (book.BookCategories != null && book.BookCategories.Any())
            {
                foreach (var bookCategory in book.BookCategories)
                {
                    var category = await _unitOfWork.CategoryRepository.GetById(bookCategory.CategoryId);
                    if (category != null)
                    {
                        bookModel.BookCategories.Add(new BookCategoryModel
                        {
                            CategoryId = category.Id,
                            CategoryName = category.CategoryName,
                            BookId = book.Id
                        });
                    }
                }
            }
            
            // Add inventory information - simplified
            if (book.Inventories != null && book.Inventories.Any())
            {
                bookModel.Inventories = book.Inventories.Select(i => new InventoryModel
                {
                    Id = i.Id,
                    EntityId = i.EntityId,
                    StoreId = i.StoreId,
                    Quantity = i.Quantity,
                    IsInStock = i.IsInStock
                }).ToList();
            }
            
            return bookModel;
        }

        public async Task<(List<BookModel>?, long)> SearchPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var books = _mapper.Map<Book>(bookModel);
            var (bookList, total) = await _bookRepository.SearchPagination(books, startDate, endDate, minPrice, maxPrice, pageNumber, pageSize, sortField, sortOrder);
            
            if (!bookList.Any()) 
                return (null, total);
            
            var resultList = new List<BookModel>();
            
            // Create simplified book models
            foreach (var book in bookList)
            {
                // Basic book information
                var simplifiedBook = new BookModel
                {
                    Id = book.Id,
                    ISBN = book.ISBN,
                    Title = book.Title,
                    PublicationDate = book.PublicationDate,
                    Price = book.Price,
                    Languages = book.Languages,
                    Description = book.Description,
                    Size = book.Size,
                    Status = book.Status,
                    PublisherId = book.PublisherId
                };
                
                // Publisher information - simplified
                if (book.PublisherId.HasValue)
                {
                    var publisher = await _unitOfWork.PublisherRepository.GetById(book.PublisherId.Value);
                    if (publisher != null)
                    {
                        simplifiedBook.Publisher = new PublisherModel 
                        { 
                            Id = publisher.Id,
                            PublisherName = publisher.PublisherName,
                            Address = publisher.Address,
                            Phone = publisher.Phone,
                            Email = publisher.Email,
                            Description = publisher.Description,
                            Website = publisher.Website,
                        };
                    }
                }
                
                // Add simplified image information
                if (book.Images != null && book.Images.Any())
                {
                    simplifiedBook.Images = book.Images.Select(i => new Image
                    {
                        Id = i.Id,
                        Url = i.Url,
                        Type = i.Type,
                        AltText = i.AltText
                    }).ToList();
                }
                
                // Initialize empty collections
                simplifiedBook.BookAuthors = new List<BookAuthorModel>();
                simplifiedBook.BookCategories = new List<BookCategoryModel>();
                
                // Add author information - simplified
                if (book.BookAuthors != null && book.BookAuthors.Any())
                {
                    foreach (var bookAuthor in book.BookAuthors)
                    {
                        var author = await _unitOfWork.AuthorRepository.GetById(bookAuthor.AuthorId);
                        if (author != null)
                        {
                            simplifiedBook.BookAuthors.Add(new BookAuthorModel
                            {
                                AuthorId = author.Id,
                                AuthorName = author.AuthorName,
                                BookId = book.Id
                            });
                        }
                    }
                }
                
                // Add category information - simplified
                if (book.BookCategories != null && book.BookCategories.Any())
                {
                    foreach (var bookCategory in book.BookCategories)
                    {
                        var category = await _unitOfWork.CategoryRepository.GetById(bookCategory.CategoryId);
                        if (category != null)
                        {
                            simplifiedBook.BookCategories.Add(new BookCategoryModel
                            {
                                CategoryId = category.Id,
                                CategoryName = category.CategoryName,
                                BookId = book.Id
                            });
                        }
                    }
                }
                
                resultList.Add(simplifiedBook);
            }
            
            return (resultList, total);
        }

        public async Task<List<BookModel>?> SearchWithoutPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice)
        {
            var bookEntity = _mapper.Map<Book>(bookModel);
            var books = await _bookRepository.SearchWithoutPagination(bookEntity, startDate, endDate, minPrice, maxPrice);
            
            if (!books.Any()) 
                return null;
            
            var resultList = new List<BookModel>();
            
            // Create simplified book models
            foreach (var book in books)
            {
                // Basic book information
                var simplifiedBook = new BookModel
                {
                    Id = book.Id,
                    ISBN = book.ISBN,
                    Title = book.Title,
                    PublicationDate = book.PublicationDate,
                    Price = book.Price,
                    Languages = book.Languages,
                    Description = book.Description,
                    Size = book.Size,
                    Status = book.Status,
                    PublisherId = book.PublisherId
                };
                
                // Publisher information - simplified
                if (book.PublisherId.HasValue)
                {
                    var publisher = await _unitOfWork.PublisherRepository.GetById(book.PublisherId.Value);
                    if (publisher != null)
                    {
                        simplifiedBook.Publisher = new PublisherModel 
                        {
                            Id = publisher.Id,
                            PublisherName = publisher.PublisherName,
                            Address = publisher.Address,
                            Phone = publisher.Phone,
                            Email = publisher.Email,
                            Description = publisher.Description,
                            Website = publisher.Website,
                        };
                    }
                }
                
                // Add simplified image information
                if (book.Images != null && book.Images.Any())
                {
                    simplifiedBook.Images = book.Images.Select(i => new Image
                    {
                        Id = i.Id,
                        Url = i.Url,
                        Type = i.Type,
                        AltText = i.AltText
                    }).ToList();
                }
                
                // Initialize empty collections
                simplifiedBook.BookAuthors = new List<BookAuthorModel>();
                simplifiedBook.BookCategories = new List<BookCategoryModel>();
                
                // Add author information - simplified
                var bookAuthors = await _unitOfWork.BookAuthorRepository.GetByElement(book.Id, null);
                if (bookAuthors != null && bookAuthors.Any())
                {
                    foreach (var bookAuthor in bookAuthors)
                    {
                        var author = await _unitOfWork.AuthorRepository.GetById(bookAuthor.AuthorId);
                        if (author != null)
                        {
                            simplifiedBook.BookAuthors.Add(new BookAuthorModel
                            {
                                AuthorId = author.Id,
                                AuthorName = author.AuthorName,
                                BookId = book.Id
                            });
                        }
                    }
                }
                
                // Add category information - simplified
                var bookCategories = await _unitOfWork.BookCategoryRepository.GetByElement(book.Id, null);
                if (bookCategories != null && bookCategories.Any())
                {
                    foreach (var bookCategory in bookCategories)
                    {
                        var category = await _unitOfWork.CategoryRepository.GetById(bookCategory.CategoryId);
                        if (category != null)
                        {
                            simplifiedBook.BookCategories.Add(new BookCategoryModel
                            {
                                CategoryId = category.Id,
                                CategoryName = category.CategoryName,
                                BookId = book.Id
                            });
                        }
                    }
                }
                
                resultList.Add(simplifiedBook);
            }
            
            return resultList;
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
                _logger.LogInformation($"Received book data from Google Books API");
                _logger.LogInformation($"Book has {googleBook.Images?.Count ?? 0} images");
                if (googleBook.Images?.Any() == true)
                {
                    _logger.LogInformation($"First image URL: {googleBook.Images.First().Url}");
                }

                bookModel.Title = string.IsNullOrEmpty(bookModel.Title) ? googleBook.Title : bookModel.Title;
                bookModel.PublicationDate = bookModel.PublicationDate ?? googleBook.PublicationDate;
                bookModel.Price = bookModel.Price ?? googleBook.Price;
                bookModel.Languages = bookModel.Languages ?? googleBook.Languages;
                bookModel.Description = bookModel.Description ?? googleBook.Description;
                bookModel.Size = bookModel.Size ?? googleBook.Size;
                bookModel.Status = bookModel.Status ?? googleBook.Status;

                if (bookModel.BookAuthors == null || !bookModel.BookAuthors.Any())
                    bookModel.BookAuthors = googleBook.BookAuthors;

                if (bookModel.BookCategories == null || !bookModel.BookCategories.Any())
                    bookModel.BookCategories = googleBook.BookCategories;

                if (bookModel.Publisher == null && googleBook.Publisher != null)
                    bookModel.Publisher = googleBook.Publisher;

                if (bookModel.Images == null || !bookModel.Images.Any())
                {
                    _logger.LogInformation("Adding images from Google Books API to book model");
                    bookModel.Images = googleBook.Images;
                }
            }

            if (string.IsNullOrWhiteSpace(bookModel.Title))
                return (null, ConstantMessage.Book.EmptyTitle);

            var bookEntity = _mapper.Map<Book>(bookModel);
            bookEntity = await SetBaseEntityToCreateFunc(bookEntity);

            // Handle publisher
            if (bookModel.Publisher != null && !string.IsNullOrEmpty(bookModel.Publisher.PublisherName))
            {
                // Check if publisher exists by exact name match
                var existingPublishers = await _unitOfWork.PublisherRepository.SearchWithoutPagination(new Publisher { PublisherName = bookModel.Publisher.PublisherName });
                
                // Filter for exact name match (case insensitive)
                var publisher = existingPublishers?.FirstOrDefault(p => 
                    string.Equals(p.PublisherName, bookModel.Publisher.PublisherName, StringComparison.OrdinalIgnoreCase));

                // If publisher already has an ID, try to find by ID first
                if (bookModel.Publisher.Id != Guid.Empty)
                {
                    var publisherById = await _unitOfWork.PublisherRepository.GetById(bookModel.Publisher.Id);
                    if (publisherById != null)
                    {
                        publisher = publisherById;
                    }
                }

                if (publisher == null)
                {
                    // Create new publisher if not exists
                    var newPublisher = new Publisher
                    {
                        PublisherName = bookModel.Publisher.PublisherName,
                        IsDeleted = false,
                        CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    var publisherAdded = await _unitOfWork.PublisherRepository.Add(newPublisher);
                    if (!publisherAdded)
                        return (null, ConstantMessage.Common.AddFail);
                    publisher = newPublisher;
                }

                bookEntity.PublisherId = publisher.Id;
                bookEntity.Publisher = null; // Set to null to prevent EF from inserting a duplicate publisher
            }

            if (bookModel.BookAuthors?.Any() == true)
            {
                var bookAuthors = new List<BookAuthor>();
                foreach (var bookAuthorModel in bookModel.BookAuthors)
                {
                    // Check if author exists by name
                    var existingAuthors = await _unitOfWork.AuthorRepository.GetAll(bookAuthorModel.AuthorName, null);
                    var author = existingAuthors?.FirstOrDefault();

                    if (author == null)
                    {
                        // Create new author if not exists
                        var newAuthor = new Author
                        {
                            AuthorName = bookAuthorModel.AuthorName,
                            IsDeleted = false,
                            CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                            CreatedDate = DateTime.UtcNow
                        };
                        var authorAdded = await _unitOfWork.AuthorRepository.Add(newAuthor);
                        if (!authorAdded)
                            return (null, ConstantMessage.Common.AddFail);
                        author = newAuthor;
                    }

                    // Create book-author relationship
                    var bookAuthor = new BookAuthor
                    {
                        Id = Guid.NewGuid(),
                        AuthorId = author.Id,
                        BookId = bookEntity.Id,
                        CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    bookAuthors.Add(bookAuthor);
                }
                bookEntity.BookAuthors = bookAuthors;
            }

            if (bookModel.BookCategories?.Any() == true)
            {
                var bookCategories = new List<BookCategory>();
                foreach (var bookCategoryModel in bookModel.BookCategories)
                {
                    // Check if cate exists by name
                    var existingCategoriess = await _unitOfWork.CategoryRepository.GetAll(bookCategoryModel.CategoryName, null);
                    var category = existingCategoriess?.FirstOrDefault();

                    if (category == null)
                    {
                        // Create new cate if not exists
                        var newCategory = new Category
                        {
                            CategoryName = bookCategoryModel.CategoryName,
                            IsDeleted = false,
                            CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                            CreatedDate = DateTime.UtcNow
                        };
                        var categoryAdded = await _unitOfWork.CategoryRepository.Add(newCategory);
                        if (!categoryAdded)
                            return (null, ConstantMessage.Common.AddFail);
                        category = newCategory;
                    }

                    // Create book-category relationship
                    var bookCategory = new BookCategory
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = category.Id,
                        BookId = bookEntity.Id,
                        CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    bookCategories.Add(bookCategory);
                }
                bookEntity.BookCategories = bookCategories;
            }

            if (bookModel.MainImageFile != null)
            {
                _logger.LogInformation("Processing main image file");
                var imageModel = new FileModel
                {
                    File = bookModel.MainImageFile,
                    Type = "book_main",
                    AltText = bookModel.Title ?? bookModel.MainImageFile.FileName,
                    EntityId = bookEntity.Id
                };

                var images = await _imageService.AddImages(new List<FileModel> { imageModel });
                bookEntity.BaseImgUrl = images?.FirstOrDefault()?.Url;
            }
            else if (bookModel.Images?.Any() == true)
            {
                _logger.LogInformation($"Processing {bookModel.Images.Count} images from Google Books API");
                
                // Get existing images for this book
                var existingImages = await _imageService.GetImagesByTypeAndEntityID(null, bookEntity.Id);
                var existingUrls = existingImages?.Select(i => i.Url).ToList() ?? new List<string>();

                foreach (var image in bookModel.Images)
                {
                    if (string.IsNullOrEmpty(image.Url)) 
                    {
                        _logger.LogWarning("Empty image URL found, skipping");
                        continue;
                    }

                    // Skip if image URL already exists
                    if (existingUrls.Contains(image.Url))
                    {
                        _logger.LogInformation($"Image with URL {image.Url} already exists, skipping");
                        continue;
                    }

                    _logger.LogInformation($"Adding image with URL: {image.Url}");
                    var newImage = new Image
                    {
                        Url = image.Url,
                        Type = image.Type,
                        AltText = bookModel.Title ?? "Book Cover",
                        EntityId = bookEntity.Id,
                        CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    var imageAdded = await _imageRepository.Add(newImage);
                    if (!imageAdded)
                    {
                        _logger.LogError($"Failed to add image for book {bookEntity.Id}");
                    }
                    else
                    {
                        _logger.LogInformation($"Successfully added image for book {bookEntity.Id}");
                        existingUrls.Add(image.Url); // Add to existing URLs to prevent duplicates
                    }
                }
                bookEntity.BaseImgUrl = bookModel.Images.FirstOrDefault(i => i.Type == "book_main")?.Url;
                
                // Important: Clear the Images collection to prevent Entity Framework from inserting them again
                bookEntity.Images = null;
            }
            else
            {
                _logger.LogWarning("No images found in book model");
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
                    _logger.LogInformation($"Received book data from Google Books API");
                    _logger.LogInformation($"Book has {googleBook.Images?.Count ?? 0} images");
                    if (googleBook.Images?.Any() == true)
                    {
                        _logger.LogInformation($"First image URL: {googleBook.Images.First().Url}");
                    }

                    // Update fields only if they are empty in the current model
                    bookModel.Title = string.IsNullOrEmpty(bookModel.Title) ? googleBook.Title : bookModel.Title;
                    bookModel.Description = string.IsNullOrEmpty(bookModel.Description) ? googleBook.Description : bookModel.Description;
                    bookModel.Languages = string.IsNullOrEmpty(bookModel.Languages) ? googleBook.Languages : bookModel.Languages;
                    bookModel.PublicationDate = bookModel.PublicationDate ?? googleBook.PublicationDate;
                    bookModel.Size = string.IsNullOrEmpty(bookModel.Size) ? googleBook.Size : bookModel.Size;

                    if (bookModel.Images == null || !bookModel.Images.Any())
                    {
                        _logger.LogInformation("Adding images from Google Books API to book model");
                        bookModel.Images = googleBook.Images;
                    }
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
                
                // Handle publisher - similar to Add method
                if (bookModel.Publisher != null && !string.IsNullOrEmpty(bookModel.Publisher.PublisherName))
                {
                    // Check if publisher exists by exact name match
                    var existingPublishers = await _unitOfWork.PublisherRepository.SearchWithoutPagination(new Publisher { PublisherName = bookModel.Publisher.PublisherName });
                    
                    // Filter for exact name match (case insensitive)
                    var publisher = existingPublishers?.FirstOrDefault(p => 
                        string.Equals(p.PublisherName, bookModel.Publisher.PublisherName, StringComparison.OrdinalIgnoreCase));

                    // If publisher already has an ID, try to find by ID first
                    if (bookModel.Publisher.Id != Guid.Empty)
                    {
                        var publisherById = await _unitOfWork.PublisherRepository.GetById(bookModel.Publisher.Id);
                        if (publisherById != null)
                        {
                            publisher = publisherById;
                        }
                    }

                    if (publisher == null)
                    {
                        // Create new publisher if not exists
                        var newPublisher = new Publisher
                        {
                            PublisherName = bookModel.Publisher.PublisherName,
                            IsDeleted = false,
                            CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                            CreatedDate = DateTime.UtcNow
                        };
                        var publisherAdded = await _unitOfWork.PublisherRepository.Add(newPublisher);
                        if (!publisherAdded)
                            return (null, ConstantMessage.Common.AddFail);
                        publisher = newPublisher;
                    }

                    existingBook.PublisherId = publisher.Id;
                    existingBook.Publisher = null; // Set to null to prevent EF from inserting a duplicate publisher
                }
                
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
                            return (null, ConstantMessage.Image.MainImageUploadFailed);

                        updatedBook.BaseImgUrl = mainImages.First().Url;
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

                //var existingMainImages = await _imageService.GetImagesByTypeAndEntityID("book_main", bookId);
                //var existingAdditionalImages = await _imageService.GetImagesByTypeAndEntityID("book_additional", bookId);

                //if (existingMainImages != null)
                //{
                //    foreach (var image in existingMainImages)
                //    {
                //        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                //        if (deleteResult.Item1 != 2)
                //            return (null, ConstantMessage.Image.MainImageUploadFailed);
                //    }
                //}

                //if (existingAdditionalImages != null)
                //{
                //    foreach (var image in existingAdditionalImages)
                //    {
                //        var deleteResult = await _imageService.DeleteAnImage(image.Id);
                //        if (deleteResult.Item1 != 2)
                //            return (null, ConstantMessage.Image.SubImageUploadFailed);
                //    }
                //}

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

        public async Task<(long, Book?)> DeleteABook(Guid id)
        {
            var existed = await _bookRepository.GetById(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            var isSuccess = await _bookRepository.Remove(existed);
            if (isSuccess)
            {
                try
                {
                    // Delete images from database first
                    var images = await _imageService.GetImagesByTypeAndEntityID(null, existed.Id);
                    if (images != null)
                    {
                        foreach (var image in images)
                        {
                            // Delete from Firebase Storage if it's a Firebase URL
                            if (image.Url.Contains("firebasestorage.googleapis.com"))
                            {
                                // Implementation of _firebaseStorageService.DeleteFileAsync(string url)
                            }
                            
                            // Delete from database
                            await _imageService.DeleteAnImage(image.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while deleting book images: {ex.Message}");
                    throw new Exception($"Error while deleting book: {ex.Message}");
                }
                return (2, existed);//delete thanh cong
            }
            return (3, null);       //delete fail
        }
    }
}


