using mieleApp.biz.EntitiesDTO;
using mieleApp.biz.Repository.Address;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.dal.db_context;
using template.dal.Repository;
using System.IO;
using System.Linq;

namespace mieleApp.dal.Repository.Address
{
    public class CatAddressRepository : GenericRepository<CatDireccion>, IAddressRepository
    {
        public CatAddressRepository(Db_TemplateContext context) : base(context) { }

        public List<ProductAddressDto> getAdress(int idClient)
        {
            var _consult = (from a in _context.CatDireccion
                            join s in _context.CatEstado on a.IdEstado equals s.Id
                            join t in _context.CatMunicipio on a.IdMunicipio equals t.Id
                            join p in _context.CatLocalidad on a.IdLocalidad equals p.Id
                            where a.IdCliente == idClient
                            select new ProductAddressDto
                            {
                                id_adress = a.Id,
                                address = (a.Cp != null ? a.Cp : "") + " "
                                + (a.CalleNumero != null ? a.CalleNumero : "") + " "
                                + (a.NumExt != null ? a.NumExt : "") + " "
                                + (a.NumInt != null ? a.NumInt : "") + " "
                                + (p.DescLocalidad != null ? p.DescLocalidad : "") + " "
                                + (t.DescMunicipio != null ? t.DescMunicipio : "") + " "
                                + (s.DescEstado != null ? s.DescEstado : "")
                            }).ToList();
            return _consult;
        }
        public List<GetAddressDto> getAdressFull(int idClient) {
            var _consult = (from a in _context.CatDireccion
                            join s in _context.CatEstado on a.IdEstado equals s.Id
                            join t in _context.CatMunicipio on a.IdMunicipio equals t.Id
                            join p in _context.CatLocalidad on a.IdLocalidad equals p.Id
                            where a.IdCliente == idClient
                            select new GetAddressDto
                            {
                                id_adress = a.Id,
                                address = (a.Cp != null ? a.Cp : "") + " "
                                + (a.CalleNumero != null ? a.CalleNumero : "") + " "
                                + (a.NumExt != null ? a.NumExt : "") + " "
                                + (a.NumInt != null ? a.NumInt : "") + " "
                                + (p.DescLocalidad != null ? p.DescLocalidad : "") + " "
                                + (t.DescMunicipio != null ? t.DescMunicipio : "") + " "
                                + (s.DescEstado != null ? s.DescEstado : "")
                            }).ToList();
            return _consult;
        }
    }
}
