using AIBookStreet.Repositories.Data.Entities;
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
    public class SouvenirService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService<Souvenir>(mapper, repository, httpContextAccessor), ISouvenirService
    {
        private readonly IUnitOfWork _repository = repository;
        public async Task<Souvenir?> AddASouvenir(SouvenirModel model)
        {
            var souvenir = _mapper.Map<Souvenir>(model);
            var setSouvenir = await SetBaseEntityToCreateFunc(souvenir);
            var isSuccess = await _repository.SouvenirRepository.Add(setSouvenir);
            if (isSuccess)
            {
                return setSouvenir;
            }
            return null;
        }
        public async Task<(long, Souvenir?)> UpdateASouvenir(Guid? id, SouvenirModel model)
        {
            var existed = await _repository.SouvenirRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            if (existed.IsDeleted)
            {
                return (3, null);
            }
            existed.SouvenirName = model.SouvenirName;
            existed.Description = model.Description ?? existed.Description;
            existed = await SetBaseEntityToUpdateFunc(existed);
            return await _repository.SouvenirRepository.Update(existed) ? (2, existed) //update thanh cong
                                                                          : (3, null);       //update fail
        }
        public async Task<(long, Souvenir?)> DeleteASouvenir(Guid id)
        {
            var existed = await _repository.SouvenirRepository.GetByID(id);
            if (existed == null)
            {
                return (1, null); //khong ton tai
            }
            existed = await SetBaseEntityToUpdateFunc(existed);

            //var bookCategories = await _repository.BookAuthorRepository.GetByElement(null, id);
            //if (bookCategories != null)
            //{
            //    foreach (var bookCategory in bookCategories)
            //    {
            //        bookCategory.LastUpdatedBy = existed.LastUpdatedBy;
            //        bookCategory.LastUpdatedDate = DateTime.Now;
            //        bookCategory.IsDeleted = true;
            //    }
            //}

            return await _repository.SouvenirRepository.Delete(existed) ? (2, existed) //delete thanh cong
                                                                          : (3, null);       //delete fail
        }
        public async Task<Souvenir?> GetASouvenirById(Guid id)
        {
            return await _repository.SouvenirRepository.GetByID(id);
        }

        public async Task<(List<Souvenir>?, long)> GetAllSouvenirsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var user = await GetUserInfo();
            var isAdmin = false;
            if (user != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role.RoleName == "Quản trị viên")
                    {
                        isAdmin = true;
                    }
                }
            }
            var souvenirs = isAdmin ? await _repository.SouvenirRepository.GetAllPaginationForAdmin(key, pageNumber, pageSize, sortField, desc)
                                                       : await _repository.SouvenirRepository.GetAllPagination(key, pageNumber, pageSize, sortField, desc);
            return souvenirs.Item1.Count > 0 ? (souvenirs.Item1, souvenirs.Item2) : (null, 0);
        }

    }
}
