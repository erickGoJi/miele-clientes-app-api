using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mieleApp.api.Models.Product;
using mieleApp.biz.Repository.Product;
using template.api.ActionFilter;
using template.api.Models;
using template.biz.Entities;
using template.biz.Servicies;

namespace mieleApp.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductUserController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IProductUserRepository _productUserRepository;


        public ProductUserController(
         IMapper mapper,
         ILoggerManager logger,
         IProductUserRepository productUserRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _productUserRepository = productUserRepository;
        }

        // GET: api/ProductUser
      

        // POST: api/ProductUser
        [HttpPost]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<ProducUserDto>> create(ProductUserCreateDto item)
        {
            var response = new ApiResponse<UserDto>();

            try
            {

                UserSubLineaApp productUser = _productUserRepository.Add(_mapper.Map<UserSubLineaApp>(item));
                response.Result = _mapper.Map<UserDto>(productUser);

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = ex.Message.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return StatusCode(201, response);
        }

        // PUT: api/ProductUser/5

        
        
    }
}
