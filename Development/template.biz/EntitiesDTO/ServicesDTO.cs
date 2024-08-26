using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using template.biz.Entities;

namespace mieleApp.api.Models.Service
{
    public class ServiceAddDTO
    {
        public long Id { get; set; }
        public string DescripcionActividades { get; set; }
        public DateTime FechaServicio { get; set; }
        public long IdCliente { get; set; }
        public int IdTipoServicio { get; set; }
        public string[] fotos { get; set; }
        public string Hour { get; set; }
        public string HourEnd { get; set; }
        public int IdAdress { get; set; }
        public bool Pagado { get; set; }
        public decimal Cantidad { get; set; }
        public long[] IdTecnicos { get; set; }
        public decimal Visita_pagada { get; set; }
        public List<productoDTO> Productos { get; set; }
    }
    public class responseServices
    {
        public long id { get; set; }
        //public List<tecnicoDTO> tecnico { get; set; }
        public string estado { get; set; }
        public int idEstado { get; set; }
        public string descripcion { get; set; }
        //public List<Visita> visitas { get; set; }    
        public DateTime actualizado { get; set; }
        public List<VisitaDTO> visitas { get; set; }
    }
    public class VisitaDTO : Visita
    {
        public bool app { get; set; }
        public bool pagadaCotizacion { get; set; }
        public List<tecnicoDTO> tecnico { get; set; }
        public string estatusDescripcion { get; set; }
    }
    public class Disponibilidad
    {
        public int disponible { get; set; }
        public List<tecnico_disponibilidad> tecnico_disponibilidad { get; set; }
    }
    public class dto_calendario
    {
        public List<Visita> visita{ get; set; }
        public List<Tecnicos> tecnico { get; set; }
    }
    public class tecnico_disponibilidad
    {
        public long id { get; set; }
        public string vehiculoInfo { get; set; }
        public string vehiculoPlacas { get; set; }
        public string nombre { get; set; }
        public string noalmacen { get; set; }
    }

    public class dto_total_horas
    {
        public int tipo_servicio { get; set; }
        public List<productos_total> productos { get; set; }
    }
    public class productos_total
    {
        public int id_sublinea { get; set; }
        public int cantidad { get; set; }
    }

    public class tecnicoDTO
    {
        public long idTecnico { get; set; }
        public string nombreTecnico { get; set; }
        public bool responsable { get; set; }
        public string automovil { get; set; }
        public string placas { get; set; }
        public string avatar { get; set; }
    }
    public class productoDTO
    {
        public long idProducto { get; set; }
        public string nombre { get; set; }
        public int cantidad { get; set; }
        public int lineId { get; set; }
        public int estatus { get; set; }
        public bool garantiaProducto { get; set; }
        public string descEstatus { get; set; }
        public decimal SubtotalRefacciones { get; set; }
        public decimal SubtotalProducto { get; set; }
        public List<ManObraDTO> mano_de_obra { get; set; }
        public List<CotizacionDTO> Cotizacion { get; set; }
    }
    public class servicioDetalle
    {
        public string descripcion { get; set; }
        public string direccion { get; set; }
        public DateTime fecha { get; set; }
        public string hora { get; set; }
        public int encuesta { get; set; }
        public List<tecnicoDTO> tecnicos { get; set; }
        public List<productoDTO> productos { get; set; }
    }

    public class CotizacionDTO
    {
        public int id { get; set; }
        public int id_materia { get; set; }
        public string refaccion { get; set; }
        public int cantidad { get; set; }
        public decimal precio_sin_iva { get; set; }
        public int total_cantidad { get; set; }
        public decimal total_precio { get; set; }
        public string reporte_visita { get; set; }
        public string reporte_cotizacion { get; set; }
        public bool garantia { get; set; }
        public bool garantiaProducto { get; set; }
        public decimal SubTotal { get; set; }
        public List<ManObraDTO> mano_de_obra { get; set; }
    }

    public class ManObraDTO
    {
        public decimal precio_hora_tecnico { get; set; }
        public int PrecioVisita { get; set; }
    }

    public class CategoriaTipoProducto : RelCategoriaProductoTipoProducto
    {
        public decimal iva { get; set; }
        public decimal subTotal { get; set; }
        public decimal Total { get; set; }

    }
}
