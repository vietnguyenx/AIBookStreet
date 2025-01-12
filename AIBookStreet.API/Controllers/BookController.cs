using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
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
    [Route("api/book")]
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

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var books = await _bookService.GetAll();

                return books switch
                {
                    null => Ok(new ItemListResponse<BookModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<BookModel>(ConstantMessage.Success, books))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("get-all-pagination")]
        public async Task<IActionResult> GetAllPagination(PaginatedRequest paginatedRequest)
        {
            try
            {
                var books = await _bookService.GetAllPagination(paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);
                long totalOrigin = await _bookService.GetTotalCount();
                return books switch
                {
                    null => Ok(new PaginatedListResponse<BookModel>(ConstantMessage.NotFound)),
                    not null => Ok(new PaginatedListResponse<BookModel>(ConstantMessage.Success, books, totalOrigin, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Id is empty");
                }
                var bookModel = await _bookService.GetById(id);

                return bookModel switch
                {
                    null => Ok(new ItemResponse<BookModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<BookModel>(ConstantMessage.Success, bookModel))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(PaginatedRequest<BookSearchRequest> paginatedRequest)
        {
            try
            {
                var book = _mapper.Map<BookModel>(paginatedRequest.Result);
                var books = await _bookService.Search(book, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

                return books.Item1 switch
                {
                    null => Ok(new PaginatedListResponse<BookModel>(ConstantMessage.NotFound, books.Item1, books.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField)),
                    not null => Ok(new PaginatedListResponse<BookModel>(ConstantMessage.Success, books.Item1, books.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(BookRequest bookRequest)
        {
            try
            {
                var isBook = await _bookService.Add(_mapper.Map<BookModel>(bookRequest));

                return isBook switch
                {
                    true => Ok(new BaseResponse(isBook, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isBook, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(BookRequest bookRequest)
        {
            try
            {
                var bookModel = _mapper.Map<BookModel>(bookRequest);

                var isBook = await _bookService.Update(bookModel);

                return isBook switch
                {
                    true => Ok(new BaseResponse(isBook, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isBook, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id != Guid.Empty)
                {
                    var isBook = await _bookService.Delete(id);

                    return isBook switch
                    {
                        true => Ok(new BaseResponse(isBook, ConstantMessage.Success)),
                        _ => Ok(new BaseResponse(isBook, ConstantMessage.Fail))
                    };
                }
                else
                {
                    return BadRequest("It's not empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
    
}
