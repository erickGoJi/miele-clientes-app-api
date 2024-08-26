using mieleApp.biz.Repository.Product;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.dal.db_context;
using template.dal.Repository;

namespace mieleApp.dal.Repository.Product
{
    public class ProductUserRepository : GenericRepository<UserSubLineaApp>, IProductUserRepository
    {
        public ProductUserRepository(Db_TemplateContext context) : base(context) { }

        public override UserSubLineaApp Update(UserSubLineaApp userSublinea, object id)
        {
            
            return base.Update(userSublinea, id);
        }
    }
}
