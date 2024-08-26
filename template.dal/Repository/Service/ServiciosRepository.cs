using mieleApp.biz.EntitiesDTO.Service;
using mieleApp.biz.Repository.Service;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.dal.db_context;
using template.dal.Repository;
using System.Linq;

namespace mieleApp.dal.Repository.Service
{
    public class ServiciosRepository : GenericRepository<Servicio>, IServiciosRepository
    {
        public ServiciosRepository(Db_TemplateContext context) : base(context) { }

        public List<clsServicioDTO> getAllServiceByClient(long _idClient, bool _general)
        {
            List<clsServicioDTO> _servicios = new List<clsServicioDTO>();

            RelUserUserApp _ClientWeb = _context.RelUserUserApp.FirstOrDefault(c => c.IdClientApp == _idClient);
            long? _idClientWeb = 0;
            if (_ClientWeb != null)
                _idClientWeb = _ClientWeb.IdClient;

            List<clsServicioDTO> _serviciosWEB = new List<clsServicioDTO>();
            if (_idClientWeb != 0)
                _serviciosWEB = (from servicio in _context.Servicio
                                 join estatusServicio in _context.CatEstatusServicio on servicio.IdEstatusServicio equals estatusServicio.Id
                                 join tipoServicio in _context.CatTipoServicio on servicio.IdTipoServicio equals tipoServicio.Id
                                 where servicio.IdCliente == _idClientWeb
                                 select new clsServicioDTO
                                 {
                                     IdEstatusServicio = Convert.ToInt32(servicio.IdEstatusServicio),
                                     DescripcionEstatusServicio = estatusServicio.DescEstatusServicio.ToUpper(),
                                     IdServicio = Convert.ToInt32(servicio.Id),
                                     IdTipoServicio = servicio.IdTipoServicio,
                                     DescripcionTipoServicio = tipoServicio.DescTipoServicio.ToUpper(),
                                     DescripcionServicio = servicio.DescripcionActividades,
                                     app = false,
                                     EstatusEncuesta = Convert.ToByte(servicio.Encuesta),
                                     Visitas = (from visita in _context.Visita
                                                join estatusVisita in _context.CatEstatusVisita on visita.Estatus equals estatusVisita.Id
                                                where visita.IdServicio == servicio.Id
                                                select new clsVisitaDTO
                                                {
                                                    IdVisita = Convert.ToInt32(visita.Id),
                                                    IdEstatusVisita = Convert.ToInt32(visita.Estatus),
                                                    DescripcionEstatus = estatusVisita.DescEstatusVisita.ToUpper(),
                                                    Productos = (from relServicioProducto in _context.RelServicioProducto
                                                                 where relServicioProducto.IdVista == visita.Id
                                                                 select new clsProductoDTO
                                                                 {
                                                                     Garantia = relServicioProducto.Garantia,
                                                                     Estatusproducto = (from relServicioRefaccion in _context.RelServicioRefaccion
                                                                                        where relServicioRefaccion.IdVista == visita.Id && relServicioRefaccion.IdProducto == relServicioProducto.IdProducto
                                                                                        select relServicioRefaccion.Estatus).FirstOrDefault()
                                                                 }).ToList()
                                                }).ToList()
                                 }).ToList();
            var _serviciosApp = (from servicio in _context.ServicioApp
                                 join estatusServicio in _context.CatEstatusServicio on servicio.IdEstatusServicio equals estatusServicio.Id
                                 join tipoServicio in _context.CatTipoServicio on servicio.IdTipoServicio equals tipoServicio.Id
                                 where servicio.IdCliente == _idClient
                                 select new clsServicioDTO
                                 {
                                     IdEstatusServicio = Convert.ToInt32(servicio.IdEstatusServicio),
                                     DescripcionEstatusServicio = estatusServicio.DescEstatusServicio.ToUpper(),
                                     IdServicio = Convert.ToInt32(servicio.Id),
                                     IdTipoServicio = servicio.IdTipoServicio,
                                     DescripcionTipoServicio = tipoServicio.DescTipoServicio.ToUpper(),
                                     DescripcionServicio = servicio.DescripcionActividades,
                                     app = true,
                                     EstatusEncuesta = 0,
                                     Visitas = (from visita in _context.VisitaApp
                                                join estatusVisita in _context.CatEstatusVisita on visita.Estatus equals estatusVisita.Id
                                                where visita.IdServicio == servicio.Id
                                                select new clsVisitaDTO
                                                {
                                                    IdVisita = Convert.ToInt32(visita.Id),
                                                    IdEstatusVisita = Convert.ToInt32(visita.Estatus),
                                                    DescripcionEstatus = estatusVisita.DescEstatusVisita.ToUpper(),
                                                    Productos = (from relServicioCategoriaApp in _context.RelServicioCategoriaApp
                                                                 where relServicioCategoriaApp.IdVisita == visita.Id
                                                                 select new clsProductoDTO
                                                                 {
                                                                     Garantia = false,
                                                                     Estatusproducto = 0
                                                                 }).ToList()
                                                }).ToList()
                                 }).ToList();

            for (int i = 0; i < _serviciosWEB.Count; i++)
            {
                _servicios.Add(new clsServicioDTO()
                {
                    IdEstatusServicio = Convert.ToInt16(_serviciosWEB[i].IdEstatusServicio),
                    DescripcionEstatusServicio = _serviciosWEB[i].DescripcionEstatusServicio,
                    IdServicio = Convert.ToInt16(_serviciosWEB[i].IdServicio),
                    IdTipoServicio = _serviciosWEB[i].IdTipoServicio,
                    DescripcionTipoServicio = _serviciosWEB[i].DescripcionTipoServicio,
                    DescripcionServicio = _serviciosWEB[i].DescripcionServicio,
                    app = _serviciosWEB[i].app,
                    EstatusEncuesta = Convert.ToByte(_serviciosWEB[i].EstatusEncuesta),
                    DescripcionEstatusProductos = getMensajeSemaforo(_serviciosWEB[i].Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(Convert.ToInt16(_serviciosWEB[i].IdEstatusServicio)),
                    Visitas = _serviciosWEB[i].Visitas
                });
            }
            for (int i = 0; i < _serviciosApp.Count; i++)
            {
                _servicios.Add(new clsServicioDTO()
                {
                    IdEstatusServicio = Convert.ToInt16(_serviciosApp[i].IdEstatusServicio),
                    DescripcionEstatusServicio = _serviciosApp[i].DescripcionEstatusServicio,
                    IdServicio = Convert.ToInt16(_serviciosApp[i].IdServicio),
                    IdTipoServicio = _serviciosApp[i].IdTipoServicio,
                    DescripcionTipoServicio = _serviciosApp[i].DescripcionTipoServicio,
                    DescripcionServicio = _serviciosApp[i].DescripcionServicio,
                    app = _serviciosApp[i].app,
                    EstatusEncuesta = Convert.ToByte(_serviciosApp[i].EstatusEncuesta),
                    DescripcionEstatusProductos = getMensajeSemaforo(_serviciosApp[i].Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(Convert.ToInt16(_serviciosApp[i].IdEstatusServicio)),
                    Visitas = _serviciosApp[i].Visitas
                });
            }
            if (!_general)
                _servicios = _servicios.Where(c => c.IdEstatusServicio < 15).ToList();

            return _servicios;
        }
        #region utilities       
        private string getMensajeSemaforo(List<clsProductoDTO> _productos)
        {
            string _mensaje = string.Empty;
            List<clsProductoDTO> _validacion = _productos.Where(c => c.Estatusproducto == 0).ToList();
            if (_validacion.Count == _productos.Count)
            {
                _mensaje = "En espera de Visita";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 5).ToList();
            if (_validacion.Count == _productos.Count)
            {
                _mensaje = "Visita Completada";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 4).ToList();
            if (_validacion.Count == _productos.Count)
            {
                _mensaje = "Cancelado por Cliente";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 3).ToList();
            if (_validacion.Count == _productos.Count)
            {
                _mensaje = "Estamos esperando tu pago para continuar";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 1008).ToList();
            if (_validacion.Count > 0 && _validacion.Count == _productos.Count)
            {
                _mensaje = "Tus refacciones estan listas";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 1008).ToList();
            if (_validacion.Count > 0 && _validacion.Count < _productos.Count)
            {
                _mensaje = "Algunas de tus refacciones estan listas";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 1).ToList();
            if (_validacion.Count < _productos.Count)
            {
                _mensaje = "Estamos solicitando tus refacciones";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 2).ToList();
            if (_validacion.Count > 0 && _validacion.Count < _productos.Count)
            {
                _mensaje = "Algunas de tus refacciones estan siendo enviadas";
            }
            _validacion = _productos.Where(c => c.Estatusproducto == 1008).ToList();
            if (_validacion.Count == _productos.Count)
            {
                _mensaje = "Tus refacciones estan llegando";
            }
            return _mensaje;
        }
        private int getEstatusSemaforo(int _estatusServicio)
        {
            if (_estatusServicio == 15)
                return 3;
            if (_estatusServicio == 13)
                return 1;
            else
                return 2;
        }
        #endregion

    }
}
