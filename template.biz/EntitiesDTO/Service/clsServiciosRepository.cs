using mieleApp.api.Models.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace mieleApp.biz.EntitiesDTO.Service
{
    class clsServiciosRepository
    {
    }
    public class clsServicioDTO
    {
        public int IdEstatusServicio { get; set; }
        public string DescripcionEstatusServicio { get; set; }
        public string DescripcionEstatusProductos { get; set; }
        public int Semaforo { get; set; }
        public int IdServicio { get; set; }
        public int IdTipoServicio { get; set; }
        public string DescripcionTipoServicio { get; set; }
        public string DescripcionServicio { get; set; }
        public bool app { get; set; }
        public byte EstatusEncuesta { get; set; }
        public List<clsVisitaDTO> Visitas { get; set; }
    }
    public class clsVisitaDTO
    {
        public int IdVisita { get; set; }
        public int IdEstatusVisita { get; set; }
        public string DescripcionEstatus { get; set; }
        public int[] EstatusProductos { get; set; }
        public int IdDireccion { get; set; }
        public int Semaforo { get; set; }
        public bool VisitaPagada { get; set; }
        public bool CotizacionPagada { get; set; }
        public List<clsProductoDTO> Productos { get; set; }
    }
    public class clsProductoDTO
    {
        public bool Garantia { get; set; }
        public int Estatusproducto { get; set; }
    }

    public class clsVisitaServicioDTO : clsServicioDTO
    {
        public string descripcion { get; set; }
        public string direccion { get; set; }
        public DateTime fecha { get; set; }
        public string hora { get; set; }
        public int encuesta { get; set; }
        public bool pagadoVisita { get; set; }
        public bool cotizacionPagada { get; set; }
        public int idEstatusVisita { get; set; }
        public int[] EstatusProductos { get; set; }
        public int SemaforoVisita { get; set; }
        public int IdDireccion { get; set; }
        public string urlReporteVisita { get; set; }
        public string urlReporteCotizacion { get; set; }
        public decimal subTotalProductos { get; set; }
        public decimal TotalVisita { get; set; }
        public decimal Viaticos { get; set; }
        public decimal Iva { get; set; }
        public bool isAgendado { get; set; }
        public List<CotizacionDTO> costo { get; set; }
        public List<tecnicoDTO> tecnicos { get; set; }
        public List<productoDTO> productos { get; set; }
    }

    public class clsCalendar
    {
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
        public int tipoServicio { get; set; }
        public int idDireccion { get; set; }
        public List<clsCategorias> categorias { get; set; }
    }
    public class clsCategorias
    {
        public int idCategoria { get; set; }
        public int cantidad { get; set; }
    }
}
