using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Address
{
    public class AddressDto
    {
        public List<StateDto> estados { get; set; }

        public List<TownDto> municipios { get; set; }

        public List<PlaceDto> localidades { get; set; }

    }
}
