﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace template.biz.Entities
{
    public partial class ServicioApp
    {
        public ServicioApp()
        {
            RelServicioCategoriaApp = new HashSet<RelServicioCategoriaApp>();
            VisitaApp = new HashSet<VisitaApp>();
        }

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

        public virtual CatCategoriaServicio CatCategoriaServicio { get; set; }
        public virtual CatEstatusServicio CatEstatusServicio { get; set; }
        public virtual Clientes IdClienteNavigation { get; set; }
        public virtual CatSolicitadoPor IdSolicitadoPorNavigation { get; set; }
        public virtual CatSolicitudVia IdSolicitudViaNavigation { get; set; }
        public virtual CatTipoServicio IdTipoServicioNavigation { get; set; }
        public virtual SubCatTipoServicio SubTipoServicio { get; set; }
        public virtual ICollection<RelServicioCategoriaApp> RelServicioCategoriaApp { get; set; }
        public virtual ICollection<VisitaApp> VisitaApp { get; set; }
    }
}