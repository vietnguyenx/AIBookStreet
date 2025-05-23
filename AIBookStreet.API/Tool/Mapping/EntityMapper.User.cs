﻿using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void UserMapping()
        {
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<UserModel, UserRequest>().ReverseMap();
            CreateMap<UserModel, UserSearchRequest>().ReverseMap()
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            CreateMap<Role, RoleModel>().ReverseMap();
        }
    }
}
