using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Product
{
    public class ProductUserCreateDto
    {
        public long? IdUser { get; set; }
        public int? IdCatSublineaProducto { get; set; }
        public int? IdDireccioin { get; set; }
        public int? Cantidad { get; set; }
    }
}
