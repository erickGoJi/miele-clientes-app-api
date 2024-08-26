using mieleApp.biz.EntitiesDTO;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.biz.Repository;

namespace mieleApp.biz.Repository.Address
{
    public interface IAddressRepository : IGenericRepository<CatDireccion>
    {
        List<ProductAddressDto> getAdress(int idClient);
        List<GetAddressDto> getAdressFull(int idClient);
    }
}
