using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
    public class ProductController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IProductRepository _productRepository;
        private readonly IProductUserRepository _productUserRepository;

        public ProductController(
          IMapper mapper,
          ILoggerManager logger,
          IProductRepository productRepository,
          IProductUserRepository productUserRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _productRepository = productRepository;
            _productUserRepository = productUserRepository;
        }

        // GET: api/Product
        [HttpGet("getAllProducts", Name = "GetAll")]
        public ActionResult<ApiResponse<List<ProductDto>>> GetAll()
        {
            var response = new ApiResponse<List<ProductDto>>();

            try
            {
                response.Result = _mapper.Map<List<ProductDto>>(_productRepository.GetAll());
                response.Result = response.Result.Where(c => c.ShowApp).ToList();
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

        [HttpPost]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<bool>> SaveProduct(ProductUserCreateDto product)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var exist = (from p in _productUserRepository.GetAll()
                             where p.IdCatSublineaProducto == product.IdCatSublineaProducto && p.IdUser == product.IdUser && p.IdDireccioin == product.IdDireccioin select p).FirstOrDefault();
                if(exist != null)
                {
                    exist.Cantidad = exist.Cantidad + product.Cantidad;
                    _productUserRepository.Update(exist, exist.Id);
                    response.Result = true;
                    return StatusCode(201, response);

                }
                
                UserSubLineaApp productAdd = _productUserRepository.Add(_mapper.Map<UserSubLineaApp>(product));
                response.Result = true;

            }
            catch (Exception ex)
            {
                response.Result = false;
                response.Success = false;
                response.Message = ex.Message.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return StatusCode(201, response);
        }
    }
}
