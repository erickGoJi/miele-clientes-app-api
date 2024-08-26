using mieleApp.biz.Repository.Service;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.dal.db_context;
using template.dal.Repository;

namespace mieleApp.dal.Repository.Service
{
    public class ServiceFotoRepository : GenericRepository<ServicioFotos>, IServiceFotoRepository
    {
        public ServiceFotoRepository(Db_TemplateContext context) : base(context) { }
    }
}
