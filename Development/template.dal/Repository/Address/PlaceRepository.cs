using mieleApp.biz.Repository.Address;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.dal.db_context;
using template.dal.Repository;

namespace mieleApp.dal.Repository.Address
{
    public class PlaceRepository : GenericRepository<CatLocalidad>, IPlaceRepository
    {
        public PlaceRepository(Db_TemplateContext context) : base(context) { }
    }
}
