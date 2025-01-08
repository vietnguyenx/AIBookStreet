﻿using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void BookStoreMapping()
        {
            CreateMap<BookStore, BookStoreModel>().ReverseMap();
            CreateMap<BookStoreModel, BookStoreRequest>().ReverseMap();
            CreateMap<BookStoreModel, BookStoreSearchRequest>().ReverseMap();
        }
    }
}