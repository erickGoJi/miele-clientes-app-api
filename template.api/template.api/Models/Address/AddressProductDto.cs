using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Address
{
    public class AddressProductDto
    {
        public int? idAdress { get; set; }
        public int? idProduct { get; set; }
        public string productName { get; set; }
        public int? quantity { get; set; }
        public int ? lineId { get; set; }
    }
}
