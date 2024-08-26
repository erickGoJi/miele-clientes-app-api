using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public bool Estatus { get; set; }
        public int IdLineaProducto { get; set; }
        public float HpHoras { get; set; }
        public bool ShowApp { get; set; }
    }
}
