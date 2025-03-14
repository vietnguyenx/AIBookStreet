﻿using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.API.Tool.Constant;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIBookStreet.API.Controllers
{
    [Route("api/bookStore")]
    [ApiController]
    

    public class StoreController : ControllerBase
    {
        private readonly IStoreService _bookStoreService;
        private readonly IMapper _mapper;

        public StoreController(IStoreService bookStoreService, IMapper mapper)
        {
            _bookStoreService = bookStoreService;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var bookStores = await _bookStoreService.GetAll();

                return bookStores switch
                {
                    null => Ok(new ItemListResponse<StoreModel>(ConstantMessage.Fail, null)),
                    not null => Ok(new ItemListResponse<StoreModel>(ConstantMessage.Success, bookStores))
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
                var bookStores = await _bookStoreService.GetAllPagination(paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);
                long totalOrigin = await _bookStoreService.GetTotalCount();
                return bookStores switch
                {
                    null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.NotFound)),
                    not null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.Success, bookStores, totalOrigin, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
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
                var bookStoreModel = await _bookStoreService.GetById(id);

                return bookStoreModel switch
                {
                    null => Ok(new ItemResponse<StoreModel>(ConstantMessage.NotFound)),
                    not null => Ok(new ItemResponse<StoreModel>(ConstantMessage.Success, bookStoreModel))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search-pagination")]
        public async Task<IActionResult> SearchPagination(PaginatedRequest<BookStoreSearchRequest> paginatedRequest)
        {
            try
            {
                var bookStore = _mapper.Map<StoreModel>(paginatedRequest.Result);
                var bookStores = await _bookStoreService.SearchPagination(bookStore, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField, paginatedRequest.SortOrder.Value);

                return bookStores.Item1 switch
                {
                    null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.NotFound, bookStores.Item1, bookStores.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField)),
                    not null => Ok(new PaginatedListResponse<StoreModel>(ConstantMessage.Success, bookStores.Item1, bookStores.Item2, paginatedRequest.PageNumber, paginatedRequest.PageSize, paginatedRequest.SortField))
                };
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            };
        }

        [HttpPost("search-without-pagination")]
        public async Task<IActionResult> SearchWithoutPagination(BookStoreSearchRequest searchRequest)
        {
            try
            {
                var bookStoreModel = _mapper.Map<StoreModel>(searchRequest);
                var bookStores = await _bookStoreService.SearchWithoutPagination(bookStoreModel);

                return bookStores == null
                    ? Ok(new ItemListResponse<StoreModel>(ConstantMessage.NotFound, null))
                    : Ok(new ItemListResponse<StoreModel>(ConstantMessage.Success, bookStores));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> Add(StoreRequest bookStoreRequest)
        {
            try
            {
                var isBookStore = await _bookStoreService.Add(_mapper.Map<StoreModel>(bookStoreRequest));

                return isBookStore switch
                {
                    true => Ok(new BaseResponse(isBookStore, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isBookStore, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update(StoreRequest bookStoreRequest)
        {
            try
            {
                var bookStoreModel = _mapper.Map<StoreModel>(bookStoreRequest);

                var isBookStore = await _bookStoreService.Update(bookStoreModel);

                return isBookStore switch
                {
                    true => Ok(new BaseResponse(isBookStore, ConstantMessage.Success)),
                    _ => Ok(new BaseResponse(isBookStore, ConstantMessage.Fail))
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id != Guid.Empty)
                {
                    var isBookStore = await _bookStoreService.Delete(id);

                    return isBookStore switch
                    {
                        true => Ok(new BaseResponse(isBookStore, ConstantMessage.Success)),
                        _ => Ok(new BaseResponse(isBookStore, ConstantMessage.Fail))
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
