using System;
using System.Collections.Generic;
using System.Text;

namespace mieleApp.biz.EntitiesDTO
{
    public class ProductAddressDto
    {
        public int id_adress { get; set; }
        public string address { get; set; }
        public List<ProductDTO> products { get; set; }
    }
    public class ProductDTO
    {
        public int? idAdress { get; set; }
        public int? idProduct { get; set; }
        public string productName { get; set; }
        public int? quantity { get; set; }
        public int? lineId { get; set; }
    }
    public class GetAddressDto
    {
        public int id_adress { get; set; }
        public string address { get; set; }
    }
}
