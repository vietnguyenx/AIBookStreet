using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Services.Common;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AIBookStreet.Services.Services.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IMapper _mapper;
        private readonly IGoogleBookService _googleBookService;

        public BookController(IBookService bookService, IMapper mapper, IGoogleBookService googleBookService)
        {
            _bookService = bookService;
            _mapper = mapper;
            _googleBookService = googleBookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var books = await _bookService.GetAll();
                return books switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemListResponse<BookModel>(ConstantMessage.NotFound, null)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemListResponse<BookModel>(ConstantMessage.Success, books))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetAllPagination(PaginatedRequest paginatedRequest)
        {
            try
            {
                var books = await _bookService.GetAllPagination(paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);
                long totalOrigin = await _bookService.GetTotalCount();
                return books switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new PaginatedListResponse<BookModel>(ConstantMessage.NotFound)),
                    not null => StatusCode(ConstantHttpStatus.OK, new PaginatedListResponse<BookModel>(ConstantMessage.Success, books, totalOrigin, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }
                var bookModel = await _bookService.GetById(id);

                return bookModel switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemResponse<BookModel>(ConstantMessage.NotFound)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<BookModel>(ConstantMessage.Success, bookModel))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("search/paginated")]
        public async Task<IActionResult> SearchPagination(PaginatedRequest<BookSearchRequest> paginatedRequest)
        {
            try
            {
                var book = _mapper.Map<BookModel>(paginatedRequest.Result);
                var startDate = paginatedRequest.Result.StartDate;
                var endDate = paginatedRequest.Result.EndDate;
                var minPrice = paginatedRequest.Result.MinPrice;
                var maxPrice = paginatedRequest.Result.MaxPrice;

                // Handle categories (if any)
                if (paginatedRequest.Result.CategoryIds?.Any() == true)
                {
                    book.BookCategories = new List<BookCategoryModel>();
                    
                    foreach (var categoryId in paginatedRequest.Result.CategoryIds)
                    {
                        // Skip default/empty GUIDs
                        if (categoryId != Guid.Empty)
                        {
                            book.BookCategories.Add(new BookCategoryModel
                            {
                                CategoryId = categoryId
                            });
                        }
                    }
                }

                // Handle authors (if any)
                if (paginatedRequest.Result.AuthorIds?.Any() == true)
                {
                    book.BookAuthors = new List<BookAuthorModel>();
                    
                    foreach (var authorId in paginatedRequest.Result.AuthorIds)
                    {
                        // Skip default/empty GUIDs
                        if (authorId != Guid.Empty)
                        {
                            book.BookAuthors.Add(new BookAuthorModel
                            {
                                AuthorId = authorId
                            });
                        }
                    }
                }

                // Handle languages (if any)
                if (paginatedRequest.Result.LanguagesList?.Any() == true)
                {
                    // Filter out empty strings and join languages with comma
                    var filteredLanguages = paginatedRequest.Result.LanguagesList
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();
                    
                    if (filteredLanguages.Any())
                    {
                        book.Languages = string.Join(",", filteredLanguages);
                    }
                }

                var books = await _bookService.SearchPagination(book, startDate, endDate, minPrice, maxPrice, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

                return books.Item1 switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new PaginatedListResponse<BookModel>(ConstantMessage.NotFound, books.Item1, books.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField)),
                    not null => StatusCode(ConstantHttpStatus.OK, new PaginatedListResponse<BookModel>(ConstantMessage.Success, books.Item1, books.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchWithoutPagination(BookSearchRequest searchRequest)
        {
            try
            {
                var bookModel = _mapper.Map<BookModel>(searchRequest);
                var startDate = searchRequest.StartDate;
                var endDate = searchRequest.EndDate;
                var minPrice = searchRequest.MinPrice;
                var maxPrice = searchRequest.MaxPrice;

                // Handle categories (if any)
                if (searchRequest.CategoryIds?.Any() == true)
                {
                    bookModel.BookCategories = new List<BookCategoryModel>();
                    
                    foreach (var categoryId in searchRequest.CategoryIds)
                    {
                        // Skip default/empty GUIDs
                        if (categoryId != Guid.Empty)
                        {
                            bookModel.BookCategories.Add(new BookCategoryModel
                            {
                                CategoryId = categoryId
                            });
                        }
                    }
                }

                // Handle authors (if any)
                if (searchRequest.AuthorIds?.Any() == true)
                {
                    bookModel.BookAuthors = new List<BookAuthorModel>();
                    
                    foreach (var authorId in searchRequest.AuthorIds)
                    {
                        // Skip default/empty GUIDs
                        if (authorId != Guid.Empty)
                        {
                            bookModel.BookAuthors.Add(new BookAuthorModel
                            {
                                AuthorId = authorId
                            });
                        }
                    }
                }

                // Handle languages (if any)
                if (searchRequest.LanguagesList?.Any() == true)
                {
                    // Filter out empty strings and join languages with comma
                    var filteredLanguages = searchRequest.LanguagesList
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();
                    
                    if (filteredLanguages.Any())
                    {
                        bookModel.Languages = string.Join(",", filteredLanguages);
                    }
                }

                var books = await _bookService.SearchWithoutPagination(bookModel, startDate, endDate, minPrice, maxPrice);

                return books switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemListResponse<BookModel>(ConstantMessage.NotFound, null)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemListResponse<BookModel>(ConstantMessage.Success, books))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] BookRequest bookRequest)
        {
            try
            {
                var bookModel = _mapper.Map<BookModel>(bookRequest);
                bookModel.MainImageFile = bookRequest.MainImageFile;
                bookModel.AdditionalImageFiles = bookRequest.AdditionalImageFiles;

                if (bookRequest.AuthorIds?.Any() == true)
                {
                    bookModel.BookAuthors = bookRequest.AuthorIds.Select(authorId => new BookAuthorModel
                    {
                        AuthorId = authorId
                    }).ToList();
                }

                if (bookRequest.CategoryIds?.Any() == true)
                {
                    bookModel.BookCategories = bookRequest.CategoryIds.Select(categoryId => new BookCategoryModel
                    {
                        CategoryId = categoryId
                    }).ToList();
                }

                var (result, message) = await _bookService.Add(bookModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.CREATED, new ItemResponse<BookModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] BookRequest bookRequest)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var bookModel = _mapper.Map<BookModel>(bookRequest);
                bookModel.Id = id;
                bookModel.MainImageFile = bookRequest.MainImageFile;
                bookModel.AdditionalImageFiles = bookRequest.AdditionalImageFiles;

                var (result, message) = await _bookService.Update(bookModel);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<BookModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var (result, message) = await _bookService.Delete(id);
                return result switch
                {
                    null => StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, message)),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<BookModel>(message, result))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpGet("google/{isbn}")]
        public async Task<IActionResult> GetBookFromGoogle(string isbn)
        {
            try
            {
                if (string.IsNullOrEmpty(isbn))
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, "ISBN is required"));
                }

                var book = await _googleBookService.SearchBookByISBN(isbn);
                return book switch
                {
                    null => StatusCode(ConstantHttpStatus.NOT_FOUND, new BaseResponse(false, "Book not found in Google Books")),
                    not null => StatusCode(ConstantHttpStatus.OK, new ItemResponse<BookModel>("Success", book))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }

        [HttpGet("{id}/random-same-category")]
        public async Task<IActionResult> GetRandomBooksBySameCategory(Guid id, [FromQuery] int count = 5)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ConstantMessage.EmptyId));
                }

                var book = await _bookService.GetById(id);
                if (book == null)
                {
                    return StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemResponse<BookModel>(ConstantMessage.NotFound));
                }

                if (book.BookCategories == null || !book.BookCategories.Any())
                {
                    return StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemListResponse<BookModel>(ConstantMessage.NotFound, null));
                }

                var categoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList();

                var searchModel = new BookModel
                {
                    BookCategories = categoryIds.Select(categoryId => new BookCategoryModel { CategoryId = categoryId }).ToList()
                };

                var books = await _bookService.SearchWithoutPagination(searchModel, null, null, null, null);
                
                if (books == null || !books.Any())
                {
                    return StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemListResponse<BookModel>(ConstantMessage.NotFound, null));
                }

                var randomBooks = books
                    .Where(b => b.Id != id) 
                    .OrderBy(_ => Guid.NewGuid()) 
                    .Take(count)
                    .ToList();

                return randomBooks.Any() 
                    ? StatusCode(ConstantHttpStatus.OK, new ItemListResponse<BookModel>(ConstantMessage.Success, randomBooks))
                    : StatusCode(ConstantHttpStatus.NOT_FOUND, new ItemListResponse<BookModel>(ConstantMessage.NotFound, null));
            }
            catch (Exception ex)
            {
                return StatusCode(ConstantHttpStatus.BAD_REQUEST, new BaseResponse(false, ex.Message));
            }
        }
    }
}
