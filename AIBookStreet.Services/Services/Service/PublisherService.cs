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
            var publishers = await _publisherRepository.GetAll();

            if (!publishers.Any())
            {
                return null;
            }

            return _mapper.Map<List<PublisherModel>>(publishers);
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
            var publishers = _mapper.Map<Publisher>(publisherModel);
            var publishersWithTotalOrigin = await _publisherRepository.Search(publishers, pageNumber, pageSize, sortField, sortOrder);

            if (!publishersWithTotalOrigin.Item1.Any())
            {
                return (null, publishersWithTotalOrigin.Item2);
            }
            var publisherModels = _mapper.Map<List<PublisherModel>>(publishersWithTotalOrigin.Item1);

            return (publisherModels, publishersWithTotalOrigin.Item2);
        }

        public async Task<bool> Add(PublisherModel publisherModel)
        {
            var mappedPublisher = _mapper.Map<Publisher>(publisherModel);
            var newPublisher = await SetBaseEntityToCreateFunc(mappedPublisher);
            return await _publisherRepository.Add(newPublisher);
        }

        public async Task<bool> Update(PublisherModel publisherModel)
        {
            var existingPublisher = await _publisherRepository.GetById(publisherModel.Id);

            if (existingPublisher == null)
            {
                return false;
            }

            _mapper.Map(publisherModel, existingPublisher);
            var updatedPublisher = await SetBaseEntityToUpdateFunc(existingPublisher);

            return await _publisherRepository.Update(updatedPublisher);
        }

        public async Task<bool> Delete(Guid publisherId)
        {
            var existingPublisher = await _publisherRepository.GetById(publisherId);
            if (existingPublisher == null)
            {
                return false;
            }

            var mappedPublisher = _mapper.Map<Publisher>(existingPublisher);
            return await _publisherRepository.Delete(mappedPublisher);
        }


    }
}
