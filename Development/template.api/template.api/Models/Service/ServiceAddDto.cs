using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Service
{
    public class ServiceAddDto
    {
        public int Id { get; set; }
        public string DescripcionActividades { get; set; }
        public DateTime FechaServicio { get; set; }
        public long IdCliente { get; set; }
        public int IdTipoServicio { get; set; }
        public string[] fotos { get; set; }
        public string Hour { get; set; }
        public string HourEnd { get; set; }
        public int IdAdress { get; set; }
        public long[] IdTecnicos { get; set; }
        public bool Pagado { get; set; }
        public decimal cantidad { get; set; }
        public decimal Visita_pagada { get; set; }
        public List<productoDTO> Productos { get; set; }
    }
}
