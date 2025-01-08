using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
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

        [HttpPost("add")]
        public async Task<IActionResult> AddBook(BookRequest book)
        {
            try
            {
                var isBook = await _bookService.Add(_mapper.Map<BookModel>(book));

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
    }
    
}
