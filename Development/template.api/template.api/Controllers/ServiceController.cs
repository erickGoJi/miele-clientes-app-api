using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mieleApp.api.Models.Service;
using mieleApp.biz.EntitiesDTO.Service;
using mieleApp.biz.Repository.Service;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using template.api.Models;
using template.biz.Entities;
using template.biz.Servicies;
using template.dal.db_context;

namespace mieleApp.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IServiceRepository _service;
        private readonly IServiceTypeRepository _serviceTypeRepository;
        private readonly IServiceFotoRepository _serviceFotoRepository;
        private readonly Db_TemplateContext _context;

        public ServiceController(
                    IMapper mapper,
                    ILoggerManager logger,
                    IServiceRepository service,
                    IServiceTypeRepository serviceTypeRepository
            //IServiceFotoRepository serviceFotoRepository
            )
        {
            _mapper = mapper;
            _logger = logger;
            _service = service;
            _serviceTypeRepository = serviceTypeRepository;
            //_serviceFotoRepository = serviceFotoRepository;
        }

        [HttpGet("getDates_new", Name = "getDates_new")]
        public ActionResult<ApiResponse<List<dto_calendario>>> getDates_new([FromQuery] int Mount, int Year, DateTime _selected, int Actividad, int[] Categorias, int no_tecnico)
        {
            var response = new ApiResponse<List<dto_calendario>>();

            try
            {
                var _dateInicio = new DateTime(Year, Mount, 1);
                var _dateFin = _dateInicio.Date.AddMonths(1).AddDays(-1);

                response.Result = new List<dto_calendario>();
                response.Result = _service.ServiceOrder_new(_dateInicio, _dateFin, _selected, Actividad, Categorias, no_tecnico);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("getDates", Name = "GetDates")]
        public ActionResult<ApiResponse<List<Visita>>> GetDates([FromQuery] int Mount, int Year, int Actividad, int[] Categorias)
        {
            var response = new ApiResponse<List<Visita>>();

            try
            {
                var _dateInicio = new DateTime(Year, Mount, 1);
                var _dateFin = _dateInicio.Date.AddMonths(1).AddDays(-1);

                response.Result = new List<Visita>();
                response.Result = _service.ServiceOrder(_dateInicio, _dateFin, Actividad, Categorias);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("mailExample", Name = "mailExample")]
        public void mailExample([FromQuery]string email)
        {
            _service.pruebaMail(email);
        }

        [HttpGet("getDetalleAllServicio", Name = "getDetalleAllServicio")]
        public Visita getDetalleAllServicio([FromQuery]long idVisita)
        {
            Visita _visita = _service.getDetalleAllServicio(idVisita);
            return _visita;
        }
        [HttpGet("sendModelgetAllCostoServicio", Name = "sendModelgetAllCostoServicio")]
        public ActionResult<ApiResponse<clsCalendar>> sendModelgetAllCostoServicio()
        {
            var response = new ApiResponse<clsCalendar>();

            try
            {
                response.Result = new clsCalendar()
                {
                    categorias = new List<clsCategorias>() {
                      new clsCategorias(){ }
                }
                };
                response.Message = "Recibe el objeto entregado";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpGet("getModelgetAllCostoServicio", Name = "getModelgetAllCostoServicio")]
        public ActionResult<ApiResponse<List<CategoriaTipoProducto>>> getModelgetAllCostoServicio()
        {
            var response = new ApiResponse<List<CategoriaTipoProducto>>();

            try
            {
                response.Result = new List<CategoriaTipoProducto>() {
                new CategoriaTipoProducto(){

                }
                };
                response.Message = "Recibe el objeto entregado";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpPost("getAllCostoServicio", Name = "getAllCostoServicio")]
        public ActionResult<ApiResponse<List<CategoriaTipoProducto>>> getAllCostoServicio(clsCalendar _calendar)
        {
            var response = new ApiResponse<List<CategoriaTipoProducto>>();

            try
            {
                response.Result = new List<CategoriaTipoProducto>();
                response.Result = _service.getAllCostoServicio(_calendar);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPost("TotalHorasServicio", Name = "TotalHorasServicio")]
        public ActionResult TotalHorasServicio(dto_total_horas dto)
        {
            ActionResult response;

            try
            {
                response = Ok(new { result = _service.total_horas_servicio(dto), Success = true });
            }
            catch (Exception ex)
            {
                response = Ok(new { result = _service.total_horas_servicio(dto), Success = true, Error = ex.ToString() });
            }

            return Ok(response);
        }

        [HttpGet("getHours", Name = "GetHours")]
        public ActionResult<ApiResponse<List<RelCategoriaProductoTipoProducto>>> GetHours([FromQuery] int Actividad, int[] Categorias, long idDireccion)
        {
            var response = new ApiResponse<List<RelCategoriaProductoTipoProducto>>();

            try
            {
                response.Result = new List<RelCategoriaProductoTipoProducto>();
                response.Result = _service.HoraServicio(Actividad, Categorias, idDireccion);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("GetDisponiblidad", Name = "GetDisponiblidad")]
        public ActionResult GetDisponiblidad(DateTime fecha_visita, int Actividad, int[] Categorias, int tipo_servicio, int horas_visita, int no_tecnicos)
        {
            try
            {

                //response.Result = _mapper.Map<List<CotizacionDTO>>(_service.get_cotizacion(id_visita));
                return Ok(new { response = _service.GetDisponibilidad(fecha_visita, Actividad, Categorias, tipo_servicio, horas_visita, no_tecnicos) });
            }
            catch (Exception ex)
            {
                return Ok(new { response = ex.ToString() });
            }
        }

        [HttpGet("getTechnical", Name = "GetTechnical")]
        public ActionResult<ApiResponse<List<Tecnicos>>> GetTechnical([FromQuery] int Mount, int Year, int Actividad, int[] Categorias)
        {
            var response = new ApiResponse<List<Tecnicos>>();

            try
            {
                var _dateInicio = new DateTime(Year, Mount, 1);
                var _dateFin = _dateInicio.Date.AddMonths(1).AddDays(-1);

                response.Result = new List<Tecnicos>();
                response.Result = _service.GetTecnicos(_dateInicio, _dateFin, Actividad, Categorias);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [HttpGet("GetCatTypeService", Name = "GetCatTypeService")]
        public ActionResult<ApiResponse<List<ServiceTypeDto>>> GetCatTypeService()
        {
            var response = new ApiResponse<List<ServiceTypeDto>>();

            try
            {

                response.Result = _mapper.Map<List<ServiceTypeDto>>(_serviceTypeRepository.GetAll());
                response.Result = response.Result.Where(c => c.Estatus == true).ToList();
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("GetCotizacion", Name = "GetCotizacion")]
        public ActionResult<ApiResponse<List<CotizacionDTO>>> GetCotizacion(int id_visita)
        {
            var response = new ApiResponse<List<CotizacionDTO>>();

            try
            {
                response.Success = true;
                response.Result = _mapper.Map<List<CotizacionDTO>>(_service.get_cotizacion(id_visita));

                #region pruebas
                //List<CotizacionDTO> _cotizacion = new List<CotizacionDTO>();
                //_cotizacion.Add(new CotizacionDTO
                //{
                //    id_materia = 456,
                //    refaccion = "termometro",
                //    precio_sin_iva = 456.7M,
                //    cantidad = 1,
                //    garantia = true,
                //    reporte_cotizacion = "/Imagenes/pdf_reportes/fa721f3f-6787-4b5e-a8b2-a8b8e8cfab38.pdf",
                //    reporte_visita = "/Imagenes/pdf_reportes/ab1705a3-a1a6-482e-bf88-dd183d8d9ed6.pdf"
                //});
                //_cotizacion.Add(new CotizacionDTO
                //{
                //    id_materia = 129,
                //    refaccion = "calcomanía",
                //    precio_sin_iva = 45.35M,
                //    cantidad = 3,
                //    garantia = true,
                //    reporte_cotizacion = "/Imagenes/pdf_reportes/fa721f3f-6787-4b5e-a8b2-a8b8e8cfab38.pdf",
                //    reporte_visita = "/Imagenes/pdf_reportes/ab1705a3-a1a6-482e-bf88-dd183d8d9ed6.pdf"
                //});
                //_cotizacion.Add(new CotizacionDTO
                //{
                //    id_materia = 129,
                //    refaccion = "Reductor de 1/2 a 3/8",
                //    precio_sin_iva = 89.80M,
                //    cantidad = 6,
                //    garantia = true,
                //    reporte_cotizacion = "/Imagenes/pdf_reportes/fa721f3f-6787-4b5e-a8b2-a8b8e8cfab38.pdf",
                //    reporte_visita = "/Imagenes/pdf_reportes/ab1705a3-a1a6-482e-bf88-dd183d8d9ed6.pdf"
                //});
                //response.Result = _cotizacion;
                #endregion

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("GetMisProductos", Name = "GetMisProductos")]
        public ActionResult GetMisProductos(int id)
        {
            try
            {

                //response.Result = _mapper.Map<List<CotizacionDTO>>(_service.get_cotizacion(id_visita));
                return Ok(new { response = _service.get_mis_productos(id) });
            }
            catch (Exception ex)
            {
                return Ok(new { response = ex.ToString() });
            }
        }

        [HttpGet("setReagendar", Name = "setReagendar")]
        public ActionResult setReagendar([FromQuery]long id, [FromQuery] bool app)
        {
            try
            {
                return Ok(new { response = _service.setReagendar(id, app) });
            }
            catch (Exception ex)
            {
                return Ok(new { response = ex.ToString() });
            }
        }
        [HttpGet("updateVisita", Name = "updateVisita")]
        public ActionResult updateVisita([FromQuery]long idVisita, [FromQuery] bool app)
        {
            try
            {
                return Ok(new { response = _service.updateVisita(idVisita, app) });
            }
            catch (Exception ex)
            {
                return Ok(new { response = ex.ToString() });
            }
        }
        [HttpGet("getHoraVisita", Name = "getHoraVisita")]
        public ActionResult getHoraVisita([FromQuery]long idVisita)
        {
            try
            {
                return Ok(new { response = _service.HoraVisita(idVisita) });
            }
            catch (Exception ex)
            {
                return Ok(new { response = ex.ToString() });
            }
        }

        [HttpPost("AddService", Name = "AddService")]
        public ActionResult<ApiResponse<ServiceDto>> AddService(ServiceAddDto service)
        {
            var response = new ApiResponse<ServiceDto>();

            try
            {
                if (!_service.comprobarMerge(service.IdCliente))
                {
                    ServiceDto servicePass = new ServiceDto();
                    servicePass.Actualizado = DateTime.Now;
                    servicePass.Actualizadopor = service.IdCliente;
                    servicePass.Creado = DateTime.Now;
                    servicePass.Creadopor = service.IdCliente;
                    servicePass.IdCategoriaServicio = 0;
                    servicePass.IdDistribuidorAutorizado = 0;
                    servicePass.IdSolicitadoPor = 1;
                    servicePass.IdSolicitudVia = 4;
                    servicePass.IdSubTipoServicio = (service.IdTipoServicio == 3) ? 17 : (service.IdTipoServicio == 1) ? 14 : (service.IdTipoServicio == 2) ? 22 : (service.IdTipoServicio == 5) ? 23 : 25;
                    servicePass.ServicioSinPago = 0;
                    servicePass.DescripcionActividades = service.DescripcionActividades;
                    servicePass.IdCliente = service.IdCliente;
                    servicePass.IdTipoServicio = service.IdTipoServicio;
                    servicePass.FechaServicio = service.FechaServicio;
                    servicePass.IdEstatusServicio = 3;
                    //servicePass.Ibs = service.DescripcionActividades;


                    response.Result = _mapper.Map<ServiceDto>(_service.Add(_mapper.Map<ServicioApp>(servicePass)));

                    ServiceAddDTO _serviceDTO = new ServiceAddDTO()
                    {
                        Id = response.Result.Id,
                        DescripcionActividades = service.DescripcionActividades,
                        IdCliente = service.IdCliente,
                        Productos = service.Productos,
                        FechaServicio = service.FechaServicio,
                        fotos = service.fotos,
                        Hour = service.Hour,
                        HourEnd = service.HourEnd,
                        IdAdress = service.IdAdress,
                        IdTecnicos = service.IdTecnicos,
                        IdTipoServicio = service.IdTipoServicio,
                        Pagado = service.Pagado,
                        Cantidad = service.cantidad,
                        Visita_pagada = service.Visita_pagada
                    };
                    bool _status = _service.ServiceOrderSaveVisita(_serviceDTO);
                    //if (response.Result != null)
                    //{
                    //    foreach (var foto in service.fotos)
                    //    {
                    //        ServicioFotos sf = new ServicioFotos();
                    //        sf.IdServicio = response.Result.Id;
                    //        sf.UrlFoto = foto;
                    //        _serviceFotoRepository.Add(sf);
                    //    }
                    //}
                }
                else
                {
                    long _idCliente = _service.obtenerMerge(service.IdCliente);

                    Servicio servicePass = new Servicio();
                    servicePass.Actualizado = DateTime.Now;
                    servicePass.Actualizadopor = service.IdCliente;
                    servicePass.Creado = DateTime.Now;
                    servicePass.Creadopor = service.IdCliente;
                    servicePass.IdCategoriaServicio = 0;
                    servicePass.IdDistribuidorAutorizado = 0;
                    servicePass.IdSolicitadoPor = 1;
                    servicePass.IdSolicitudVia = 4;
                    servicePass.IdSubTipoServicio = (service.IdTipoServicio == 3) ? 17 : (service.IdTipoServicio == 1) ? 14 : (service.IdTipoServicio == 2) ? 22 : (service.IdTipoServicio == 5) ? 23 : 25;
                    servicePass.ServicioSinPago = 0;
                    servicePass.DescripcionActividades = service.DescripcionActividades;
                    servicePass.IdCliente = service.IdCliente;
                    servicePass.IdTipoServicio = service.IdTipoServicio;
                    servicePass.FechaServicio = service.FechaServicio;
                    servicePass.IdEstatusServicio = 3;
                    //servicePass.Ibs = service.DescripcionActividades;
                    servicePass.AppService = true;

                    ServiceAddDTO _serviceLocal = new ServiceAddDTO()
                    {
                        DescripcionActividades = service.DescripcionActividades,
                        IdCliente = service.IdCliente,
                        Productos = service.Productos,
                        FechaServicio = service.FechaServicio,
                        fotos = service.fotos,
                        Hour = service.Hour,
                        HourEnd = service.HourEnd,
                        IdAdress = service.IdAdress,
                        IdTecnicos = service.IdTecnicos,
                        IdTipoServicio = service.IdTipoServicio,
                        Pagado = service.Pagado,
                        Cantidad = service.cantidad,
                        Visita_pagada = service.Visita_pagada
                    };

                    _service.addService(servicePass, _serviceLocal);
                    //bool _status = _service.ServiceOrderSaveVisita(service.IdCliente, response.Result.Id, service.Hour, service.HourEnd, service.FechaServicio, service.IdAdress, service.IdTecnicos);

                    //if (response.Result != null)
                    //{
                    //    foreach (var foto in service.fotos)
                    //    {
                    //        ServicioFotos sf = new ServicioFotos();
                    //        sf.IdServicio = response.Result.Id;
                    //        sf.UrlFoto = foto;
                    //        _serviceFotoRepository.Add(sf);
                    //    }
                    //}
                }



            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("getCancelarServicio", Name = "getCancelarServicio")]
        public ActionResult<ApiResponse<servicioDetalle>> getCancelarServicio([FromQuery] long idVisita, bool app, string exp)
        {
            var response = new ApiResponse<servicioDetalle>();

            try
            {
                response.Result = new servicioDetalle();
                response.Result = _service.getCancelarServicio(app, idVisita, exp);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpGet("getDetalleServicio", Name = "getDetalleServicio")]
        public ActionResult<ApiResponse<servicioDetalle>> getDetalleServicio([FromQuery] long idVisita, bool app)
        {
            var response = new ApiResponse<servicioDetalle>();

            try
            {
                response.Result = new servicioDetalle();
                response.Result = _service.getDetalleServicio(app, idVisita);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpGet("setEncuesta", Name = "setEncuesta")]
        public ActionResult<ApiResponse<servicioDetalle>> setEncuesta([FromQuery] long idVisita, [FromQuery] int encuesta)
        {
            var response = new ApiResponse<servicioDetalle>();

            try
            {
                response.Result = new servicioDetalle();
                _service.updateEncuesta(idVisita, encuesta);
                response.Result = _service.getDetalleServicio(false, idVisita);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("getServicesShow", Name = "getServicesShow")]
        public ActionResult<ApiResponse<List<responseServices>>> getServicesShow([FromQuery] long idCliente, bool pendientes)
        {
            var response = new ApiResponse<List<responseServices>>();

            try
            {
                response.Result = new List<responseServices>();
                response.Result = _service.GetAllServicesShow(idCliente, pendientes);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpGet("getEstatusServicioVisita", Name = "getEstatusServicioVisita")]
        public ActionResult<ApiResponse<responseServices>> getEstatusServicioVisita([FromQuery]long _idVista, [FromQuery]bool app)
        {
            var response = new ApiResponse<responseServices>();

            try
            {
                response.Result = new responseServices();
                response.Result = _service.getEstatusServicioVisita(_idVista, app);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("modelSaveCotizacion", Name = "modelSaveCotizacion")]
        public ActionResult<ApiResponse<ServiceAddDTO>> modelSaveCotizacion()
        {
            var response = new ApiResponse<ServiceAddDTO>();

            try
            {
                response.Message = "return int nueva visita generada";
                response.Result = new ServiceAddDTO()
                {
                };
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpPost("saveCotizacion", Name = "saveCotizacion")]
        public ActionResult<ApiResponse<int>> saveCotizacion(ServiceAddDTO _visita)
        {
            var response = new ApiResponse<int>();

            try
            {
                response.Result = 0;
                response.Result = _service.saveCotization(_visita);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = 0;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("pagarCotizacion", Name = "pagarCotizacion")]
        public ActionResult<ApiResponse<bool>> pagarCotizacion([FromQuery]long idVisita)
        {
            var response = new ApiResponse<bool>();

            try
            {
                response.Result = _service.pagarCotizacion(idVisita);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [HttpPost("updateCotizacion", Name = "updateCotizacion")]
        public ActionResult<ApiResponse<bool>> updateCotizacion(ServiceAddDto service)
        {
            var response = new ApiResponse<bool>();

            try
            {
                response.Result = false;
                ServiceAddDTO _serviceDTO = new ServiceAddDTO()
                {
                    Id = service.Id,
                    FechaServicio = service.FechaServicio,
                    Hour = service.Hour,
                    HourEnd = service.HourEnd
                };
                bool _status = _service.updateCotization(_serviceDTO);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("getModelClientes", Name = "getModelClientes")]
        public ActionResult<ApiResponse<Clientes>> getModelClientes()
        {
            var response = new ApiResponse<Clientes>();
            List<DatosFiscales> _dt = new List<DatosFiscales>();
            _dt.Add(new DatosFiscales()
            {
                RazonSocial = "",
                Email = "",
                Rfc = "",
                Colonia = "",
                Cp = "",
                CalleNumero = ""
            });
            response.Result = new Clientes()
            {
                Id = 0,
                Nombre = "",
                Paterno = "",
                Materno = "",
                Email = "",
                Telefono = "",
                TelefonoMovil = "",
                DatosFiscales = _dt
            };
            return response;
        }
        [HttpPost("setProfile", Name = "setProfile")]
        public ActionResult<ApiResponse<Clientes>> setProfile(Clientes _clientes)
        {
            var response = new ApiResponse<Clientes>();

            try
            {
                response.Success = true;
                response.Result = _service.setProfile(_clientes);
            }
            catch (Exception ex)
            {
                response.Result = new Clientes();
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpPost("getProfile", Name = "getProfile")]
        public ActionResult<ApiResponse<Clientes>> getProfile(Clientes _clientes)
        {
            var response = new ApiResponse<Clientes>();

            try
            {
                response.Success = true;
                response.Result = _service.getProfile(_clientes);
            }
            catch (Exception ex)
            {
                response.Result = new Clientes();
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("getModelNotification", Name = "getModelNotification")]
        public ActionResult<ApiResponse<NotificationApp>> getNotification()
        {
            var response = new ApiResponse<NotificationApp>();
            response.Result = new NotificationApp()
            {
                Id = 0,
                NameEquipo = "",
                Notification = true,
                Token = "",
                UidEquipo = "",
                UserId = 0
            };
            return response;
        }
        [HttpPost("saveToken", Name = "saveToken")]
        public ActionResult<ApiResponse<bool>> saveToken(NotificationApp _notification)
        {
            var response = new ApiResponse<bool>();

            try
            {
                response.Success = true;
                response.Result = _service.saveToken(_notification);
            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpPost("getNotification", Name = "getNotification")]
        public ActionResult<ApiResponse<NotificationApp>> getNotification(NotificationApp _notification)
        {
            var response = new ApiResponse<NotificationApp>();

            try
            {
                response.Success = true;
                response.Result = _service.getNotification(_notification);
            }
            catch (Exception ex)
            {
                response.Result = new NotificationApp();
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpPost("setNotification", Name = "setNotification")]
        public ActionResult<ApiResponse<NotificationApp>> setNotification(NotificationApp _notification)
        {
            var response = new ApiResponse<NotificationApp>();

            try
            {
                response.Success = true;
                response.Result = _service.setNotification(_notification);
            }
            catch (Exception ex)
            {
                response.Result = new NotificationApp();
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("sendNotification", Name = "sendNotification")]
        public ActionResult<ApiResponse<bool>> sendNotification([FromQuery]int tipo)
        {
            var response = new ApiResponse<bool>();

            try
            {
                response.Success = true;
                response.Result = _service.sendNotification(tipo);
            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("sendNotificationClient", Name = "sendNotificationClient")]
        public ActionResult<ApiResponse<bool>> sendNotificationClient([FromQuery]long idVisita)
        {
            var response = new ApiResponse<bool>();

            try
            {
                response.Success = true;
                response.Result = _service.sendNotificationClient(idVisita);
            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        //[HttpPost("Upload_xls", Name = "Upload_xls")]
        ////public async Task<IActionResult> Upload_xls(List<IFormFile> file)
        //public IActionResult Upload_xls(List<IFormFile> file)
        //{
        //    IActionResult response;

        //    try
        //    {
        //        var selRuta = _context.ParametroArchivos.FirstOrDefault(p => p.Id == 3);
        //        string ruta;
        //        if (selRuta == null) ruta = "Error";
        //        else
        //        {
        //            long size = file.Sum(f => f.Length);

        //            var filePath = Environment.CurrentDirectory;
        //            var extencion = file[0].FileName.Split(".");
        //            var _guid = Guid.NewGuid();
        //            var path = "/Imagenes/carga_xls/" + _guid + "." + extencion[extencion.Length - 1].ToLower();

        //            foreach (var formFile in file)
        //            {


        //                if (formFile.Length > 0)
        //                {
        //                    //using (var stream = new FileStream(filePath + path, FileMode.Create))
        //                    //{
        //                    //    await formFile.CopyToAsync(stream);
        //                    //}

        //                    if (formFile.ContentType.StartsWith("image"))
        //                    {
        //                        using (var stream = new FileStream(filePath + "/Imagenes/carga_xls/_del" + _guid + "." + extencion[extencion.Length - 1], FileMode.Create))
        //                        {
        //                            formFile.CopyTo(stream);
        //                            System.Drawing.Image image = redimensionar(stream);
        //                            image.Save(filePath + path, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                        }
        //                        //recortar(filePath + "/Imagenes/carga_xls/_del_" + _guid + "." + extencion[extencion.Length - 1]);
        //                    }
        //                    else
        //                    {
        //                        using (var stream = new FileStream(filePath + path, FileMode.Create))
        //                        {
        //                            formFile.CopyTo(stream);
        //                        }
        //                    }
        //                }
        //            }
        //            ruta = path;
        //        }

        //        response = Ok(new { url = ruta });

        //    }
        //    catch (Exception ex)
        //    {
        //        response = Ok(new { url = ex.Message.ToString() });
        //    }

        //    return new ObjectResult(response);
        //}


        private Image redimensionar(FileStream imagen)
        {
            Image img = Image.FromStream(imagen);
            //var res = img.GetPropertyItem(274).Value[0];
            //.PropertyIdList.Contains(Orientation);
            var orientation = (int)img.GetPropertyItem(274).Value[0];
            switch (orientation)
            {
                case 1:
                    // No rotation required.
                    break;
                case 2:
                    img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 3:
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:
                    img.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case 5:
                    img.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;
                case 6:
                    img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:
                    img.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;
                case 8:
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            // This EXIF data is now invalid and should be removed.
            img.RemovePropertyItem(274);


            const int max = 1024;
            int h = img.Height;
            int w = img.Width;
            int newH, newW;



            if (h > w && h > max)
            {
                // Si la imagen es vertical y la altura es mayor que max,
                // se redefinen las dimensiones.
                newH = max;
                newW = (w * max) / h;
            }
            else if (w > h && w > max)
            {
                // Si la imagen es horizontal y la anchura es mayor que max,
                // se redefinen las dimensiones.
                newW = max;
                newH = (h * max) / w;
            }
            else
            {
                newH = h;
                newW = w;
            }
            if (h != newH && w != newW)
            {
                // Si las dimensiones cambiaron, se modifica la imagen
                Bitmap newImg = new Bitmap(img, newW, newH);
                Graphics g = Graphics.FromImage(newImg);
                g.InterpolationMode =
                  System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.DrawImage(img, 0, 0, newImg.Width, newImg.Height);
                return newImg;
            }
            else
                return img;
        }

        [HttpPost("Upload", Name = "Upload"), DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        #region Restructura
        [HttpGet("ModelgetAllServiceByClient", Name = "ModelgetAllServiceByClient")]
        public ActionResult<ApiResponse<List<clsServicioDTO>>> ModelgetAllServiceByClient()
        {
            var response = new ApiResponse<List<clsServicioDTO>>();
            try
            {
                response.Result = new List<clsServicioDTO>() {
                    new clsServicioDTO() {
                        Visitas =
                            new List<clsVisitaDTO>(){
                                 new clsVisitaDTO(){
                                    Productos = new List<clsProductoDTO>(){
                                        new clsProductoDTO(){ }
                                    }
                                  }
                            }
                    }
                };
                response.Success = true;
                response.Message = "?idCliente[long]=1002&general[bool]=true";

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpGet("getAllServiceByClient", Name = "getAllServiceByClient")]
        public ActionResult<ApiResponse<List<clsServicioDTO>>> getAllServiceByClient([FromQuery]long idCliente, [FromQuery]bool general)
        {
            var response = new ApiResponse<List<clsServicioDTO>>();

            try
            {
                response.Result = _service.getAllServiceByClient(idCliente, general);
                response.Success = true;
                response.Message = "";

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("modelgetAllDetalleVisita", Name = "modelgetAllDetalleVisita")]
        public ActionResult<ApiResponse<clsVisitaServicioDTO>> modelgetAllDetalleVisita()
        {
            var response = new ApiResponse<clsVisitaServicioDTO>();
            try
            {
                response.Result = new clsVisitaServicioDTO()
                {
                    costo = new List<CotizacionDTO>() {
                        new CotizacionDTO(){ }
                    },
                    productos = new List<productoDTO>() {
                        new productoDTO() {
                            Cotizacion = new List<CotizacionDTO>(){
                                new CotizacionDTO(){ }
                            }
                        }
                    },
                    tecnicos = new List<tecnicoDTO>() {
                        new tecnicoDTO() { }
                    },
                    Visitas = new List<clsVisitaDTO>(){
                                 new clsVisitaDTO(){
                                    Productos = new List<clsProductoDTO>(){
                                        new clsProductoDTO(){
                                        }
                                    }
                                  }
                            }
                };
                response.Success = true;
                response.Message = "?idVisita[long]=11941&app=false";

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        [HttpGet("getAllDetalleVisita", Name = "getAllDetalleVisita")]
        public ActionResult<ApiResponse<clsVisitaServicioDTO>> getAllDetalleVisita([FromQuery]long idVisita, [FromQuery]bool app)
        {
            var response = new ApiResponse<clsVisitaServicioDTO>();

            try
            {
                response.Result = _service.getAllDetalleVisita(idVisita, app);
                response.Success = true;
                response.Message = "";

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        #endregion


    }
}
