using Microsoft.AspNetCore.Mvc;
using mieleApp.api.Models.Service;
using mieleApp.biz.EntitiesDTO.Service;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.biz.Repository;

namespace mieleApp.biz.Repository.Service
{
    public interface IServiciosRepository : IGenericRepository<Servicio>
    {
        List<clsServicioDTO> getAllServiceByClient(long _idClient, bool _general);

    }
}
