using AutoMapper;
using mieleApp.api.Models.Address;
using mieleApp.api.Models.Product;
using mieleApp.api.Models.Service;
using System.Collections.Generic;
using template.api.Models;
using template.biz.Entities;

namespace template.api.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            #region User
            CreateMap<Users, UserDto>().ReverseMap();
            CreateMap<Users, UserCreateDto>().ReverseMap();
            CreateMap<Users, UserUpdateDto>().ReverseMap();
            #endregion

            #region Product
            CreateMap<CatSubLineaProducto, ProductDto>().ReverseMap();
            CreateMap<UserSubLineaApp, ProductUserCreateDto>().ReverseMap();
            #endregion

            #region
            CreateMap<List<CatDireccion>, List<SaveAddressDto>>().ReverseMap();
            CreateMap<CatDireccion, SaveAddressDto>().ReverseMap();
            #endregion

            #region serviceType
            CreateMap<CatTipoServicio, ServiceTypeDto>().ReverseMap();
            CreateMap<List<CatTipoServicio>, List<ServiceTypeDto>>().ReverseMap();
            #endregion

            #region service
            CreateMap<ServicioApp, ServiceDto>().ReverseMap();
            #endregion

        }
    }
}