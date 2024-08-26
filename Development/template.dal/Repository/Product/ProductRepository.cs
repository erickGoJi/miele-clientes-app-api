using mieleApp.biz.Repository.Product;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.biz.Repository;
using template.dal.db_context;
using template.dal.Repository;

namespace mieleApp.dal.Repository.Product
{
    public class ProductRepository : GenericRepository<CatSubLineaProducto>, IProductRepository
    {
        public ProductRepository(Db_TemplateContext context) : base(context) { }
    }
}
