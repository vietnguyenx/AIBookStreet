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

        public BookController(IBookService bookService, IMapper mapper)
        {
            _bookService = bookService;
            _mapper = mapper;
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

                // Add category to book model if CategoryId is provided
                if (paginatedRequest.Result.CategoryId.HasValue)
                {
                    book.BookCategories = new List<BookCategoryModel>
                    {
                        new BookCategoryModel { CategoryId = paginatedRequest.Result.CategoryId.Value }
                    };
                }

                if (paginatedRequest.Result.AuthorId.HasValue)
                {
                    book.BookAuthors = new List<BookAuthorModel>
                    {
                        new BookAuthorModel { AuthorId = paginatedRequest.Result.AuthorId.Value }
                    };
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
    }
}
