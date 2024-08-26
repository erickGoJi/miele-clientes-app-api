using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using mieleApp.biz.EntitiesDTO.Service;
using mieleApp.biz.Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using template.api.Models;
using template.biz.Servicies;

namespace mieleApp.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IServiciosRepository _service;

        public ServiciosController(
                    IMapper mapper,
                    ILoggerManager logger,
                    IServiciosRepository service
            )
        {
            _mapper = mapper;
            _logger = logger;
            _service = service;
        }
        //[HttpGet("ModelgetAllServiceByClient", Name = "ModelgetAllServiceByClient")]
        //public ActionResult<ApiResponse<List<clsServicioDTO>>> ModelgetAllServiceByClient()
        //{
        //    var response = new ApiResponse<List<clsServicioDTO>>();
        //    try
        //    {
        //        response.Result = new List<clsServicioDTO>() {
        //            new clsServicioDTO() {
        //                Visitas =
        //                    new List<clsVisitaDTO>(){
        //                         new clsVisitaDTO(){
        //                            Productos = new List<clsProductoDTO>(){
        //                                new clsProductoDTO(){ }
        //                            }
        //                          }
        //                    }
        //            }
        //        };
        //        response.Success = true;
        //        response.Message = "?idCliente[long]=1002&general[bool]=true";

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = null;
        //        response.Success = false;
        //        response.Message = "Internal server error";
        //        _logger.LogError($"Something went wrong: { ex.ToString() }");
        //        return StatusCode(500, response);
        //    }

        //    return Ok(response);
        //}
        //[HttpGet("getAllServiceByClient", Name = "getAllServiceByClient")]
        //public ActionResult<ApiResponse<List<clsServicioDTO>>> getAllServiceByClient([FromQuery]long idCliente, [FromQuery]bool general)
        //{
        //    var response = new ApiResponse<List<clsServicioDTO>>();

        //    try
        //    {
        //        response.Result = _service.getAllServiceByClient(idCliente, general);
        //        response.Success = true;
        //        response.Message = "";

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = null;
        //        response.Success = false;
        //        response.Message = "Internal server error";
        //        _logger.LogError($"Something went wrong: { ex.ToString() }");
        //        return StatusCode(500, response);
        //    }

        //    return Ok(response);
        //}
    }
}
