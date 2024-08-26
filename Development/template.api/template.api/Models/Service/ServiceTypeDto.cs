using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Service
{
    public class ServiceTypeDto
    {
        public int Id { get; set; }
        public string DescTipoServicio { get; set; }
        public bool Estatus { get; set; }
    }
}
