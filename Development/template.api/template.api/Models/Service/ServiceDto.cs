using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using template.biz.Entities;

namespace mieleApp.api.Models.Service
{
    public class ServiceDto
    {

        public long Id { get; set; }
        public int? CatCategoriaServicioid { get; set; }
        public int? CatEstatusServicioid { get; set; }
        public string Ibs { get; set; }
        public bool? ActivarCredito { get; set; }
        public DateTime Actualizado { get; set; }
        public long Actualizadopor { get; set; }
        public string Contacto { get; set; }
        public DateTime Creado { get; set; }
        public long Creadopor { get; set; }
        public string DescripcionActividades { get; set; }
        public DateTime FechaServicio { get; set; }
        public int IdCategoriaServicio { get; set; }
        public long IdCliente { get; set; }
        public long IdDistribuidorAutorizado { get; set; }
        public int? IdEstatusServicio { get; set; }
        public int? IdMotivoCierre { get; set; }
        public int IdSolicitadoPor { get; set; }
        public int IdSolicitudVia { get; set; }
        public int IdSubTipoServicio { get; set; }
        public int IdTipoServicio { get; set; }
        public string NoServicio { get; set; }
        public int? SubTipoServicioid { get; set; }
        public int ServicioSinPago { get; set; }
    }

    public class CalendarioDTO
    {
        public string fecha { get; set; }
        public string horaInicial { get; set; }
        public string horaFinal { get; set; }
    }
}
