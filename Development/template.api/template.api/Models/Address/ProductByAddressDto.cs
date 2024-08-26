using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Address
{
    public class ProductByAddressDto
    {
        public int id_adress { get; set; }
        public string address { get; set; }
        public List<AddressProductDto> products { get; set; }

    }
}
