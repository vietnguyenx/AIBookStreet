﻿using AIBookStreet.Repositories.Data.Entities;
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
using static AIBookStreet.Services.Common.ConstantMessage;

namespace AIBookStreet.Services.Services.Service
{
    public class OrderDetailService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<OrderDetail>(mapper, repository, httpContextAccessor), IOrderDetailService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<(long, OrderDetail?)> GetByID(Guid id)
        {
            //var user = await GetUserInfo();
            //var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
            //var orderDetailIds = new List<Guid>();
            //if (userStores != null)
            //{
            //    var storeIds = new List<Guid>();
            //    foreach (var us in userStores)
            //    {
            //        storeIds.Add(us.StoreId);
            //    }
            //    if (storeIds != null)
            //    {
            //        foreach (var storeId in storeIds)
            //        {
            //            var orderDetails = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, storeId, null);
            //            if (orderDetails != null)
            //            {
            //                foreach (var orderDetail in orderDetails)
            //                {
            //                    orderDetailIds.Add(orderDetail.Id);
            //                }
            //            }
            //        }
            //    }
            //}
            //if (orderDetailIds.Contains(id))
            //{
                var orderDetail = await _repository.OrderDetailRepository.GetByID(id);
                return (2, orderDetail);
            //}
            //return (0, null);
        }
        public async Task<(long, List<OrderDetail>?)> GetCart(Guid storeId)
        {
            var user = await GetUserInfo();
            var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
            var storeIds = new List<Guid>();
            if (userStores != null)
            {                
                foreach (var us in userStores)
                {
                    storeIds.Add(us.StoreId);
                }
            }
            if (storeIds != null && storeIds.Contains(storeId))
            {
                var cart = await _repository.OrderDetailRepository.GetCart(storeId);
                return (2, cart);
            }
            return (0, null);
        }
        public async Task<List<OrderDetail>?> GetAllOrderDetail(Guid? orderId, Guid? storeId, Guid? entityId)
        {
            //var user = await GetUserInfo();
            //var userStores = await _repository.UserStoreRepository.GetByUserId(user.Id);
            //var storeIds = new List<Guid>();
            //if (userStores != null)
            //{
            //    foreach (var us in userStores)
            //    {
            //        storeIds.Add(us.StoreId);
            //    }
            //}
            return await _repository.OrderDetailRepository.GetAllOrderDetail(null, orderId, storeId, entityId);
        }
        public async Task<OrderDetail?> AddToCart(OrderDetailModel model)
        {
            try
            {
                var existed = await _repository.OrderDetailRepository.GetAllOrderDetail(null, null, model.StoreId, model.EntityId);
                if (existed == null)
                {
                    var inventory = await _repository.InventoryRepository.GetByEntityIdAndStoreId(model.EntityId, model.StoreId);
                    var orderDetail = new OrderDetail { 
                        InventoryId = inventory.Id,
                        Quantity = model.Quantity
                    };
                    var setOrderDetail = await SetBaseEntityToCreateFunc(orderDetail);
                    var isSuccess = await _repository.OrderDetailRepository.Add(setOrderDetail);
                    if (isSuccess)
                    {
                        return setOrderDetail;
                    }
                }
                existed[0].Quantity += model.Quantity;
                var setExisted = await SetBaseEntityToCreateFunc(existed[0]);
                var isDone = await _repository.OrderDetailRepository.Add(setExisted);
                if (isDone)
                {
                    return setExisted;
                }
                return null;
            } catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long,OrderDetail?)> RemoveFromCart(Guid id)
        {
            try
            {
                var existed = await _repository.OrderDetailRepository.GetByID(id);
                if (existed == null)
                {
                    return (1, null); //khong ton tai
                }

                return await _repository.OrderDetailRepository.Remove(existed) ? (2, existed) //delete thanh cong
                                                                              : (3, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
