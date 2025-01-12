using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.Repositories.Repository;
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
    public class PublisherService : BaseService<Publisher>, IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;

        public PublisherService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork, httpContextAccessor)
        {
            _publisherRepository = unitOfWork.PublisherRepository;
        }

        public async Task<List<PublisherModel>> GetAll()
        {
            var Publishers = await _publisherRepository.GetAll();

            if (!Publishers.Any())
            {
                return null;
            }

            return _mapper.Map<List<PublisherModel>>(Publishers);
        }

        public async Task<List<PublisherModel>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var publishers = await _publisherRepository.GetAllPagination(pageNumber, pageSize, sortField, sortOrder);

            if (!publishers.Any())
            {
                return null;
            }

            return _mapper.Map<List<PublisherModel>>(publishers);
        }

        public async Task<PublisherModel?> GetById(Guid id)
        {
            var publisher = await _publisherRepository.GetById(id);

            if (publisher == null)
            {
                return null;
            }

            return _mapper.Map<PublisherModel>(publisher);
        }

        public async Task<(List<PublisherModel>?, long)> Search(PublisherModel publisherModel, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var publisher = _mapper.Map<Publisher>(publisherModel);
            var publishersWithTotalOrigin = await _publisherRepository.Search(publisher, pageNumber, pageSize, sortField, sortOrder);

            if (!publishersWithTotalOrigin.Item1.Any())
            {
                return (null, publishersWithTotalOrigin.Item2);
            }
            var publisherModels = _mapper.Map<List<PublisherModel>>(publishersWithTotalOrigin.Item1);

            return (publisherModels, publishersWithTotalOrigin.Item2);
        }

        public async Task<bool> Add(PublisherModel publisherModel)
        {

            var publisher = _mapper.Map<Publisher>(publisherModel);
            var setPublisher = await SetBaseEntityToCreateFunc(publisher);
            return await _publisherRepository.Add(setPublisher);

        }

        public async Task<bool> Update(PublisherModel publisherModel)
        {

            var entity = await _publisherRepository.GetById(publisherModel.Id);

            if (entity == null)
            {
                return false;
            }

            _mapper.Map(publisherModel, entity);
            entity = await SetBaseEntityToUpdateFunc(entity);

            return await _publisherRepository.Update(entity);
        }

        public async Task<bool> Delete(Guid id)
        {
            var entity = await _publisherRepository.GetById(id);
            if (entity == null)
            {
                return false;
            }

            var publisher = _mapper.Map<Publisher>(entity);
            return await _publisherRepository.Delete(publisher);
        }

        
    }
}
