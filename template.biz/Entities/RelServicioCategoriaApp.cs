﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace template.biz.Entities
{
    public partial class RelServicioCategoriaApp
    {
        public int Id { get; set; }
        public long IdServicio { get; set; }
        public long IdVisita { get; set; }
        public int IdSubLinea { get; set; }
        public int Cantidad { get; set; }

        public virtual ServicioApp IdServicioNavigation { get; set; }
        public virtual CatSubLineaProducto IdSubLineaNavigation { get; set; }
        public virtual VisitaApp IdVisitaNavigation { get; set; }
    }
}