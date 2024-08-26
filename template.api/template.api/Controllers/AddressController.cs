using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mieleApp.api.Models.Address;
using mieleApp.biz.Repository.Address;
using mieleApp.biz.Repository.Product;
using template.api.ActionFilter;
using template.api.Models;
using template.biz.Entities;
using template.biz.Servicies;
using mieleApp.biz.EntitiesDTO;

namespace mieleApp.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IStateRepository _stateRepository;
        private readonly ITownRepository _townRepository;
        private readonly IPlaceRepository _placeRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IProductUserRepository _productUserRepository;
        private readonly IProductRepository _productRepository;

        public AddressController(
            IMapper mapper,
            ILoggerManager logger,
            IStateRepository stateRepository,
            ITownRepository townRepository,
            IPlaceRepository placeRepository,
            IAddressRepository addressRepository,
            IProductUserRepository productUserRepository,
            IProductRepository productRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _stateRepository = stateRepository;
            _townRepository = townRepository;
            _placeRepository = placeRepository;
            _addressRepository = addressRepository;
            _productUserRepository = productUserRepository;
            _productRepository = productRepository;
        }

        // GET: api/Address
        [HttpGet("getAdress", Name = "GetAddress")]
        public ActionResult<ApiResponse<AddressDto>> GetAddress(long cp)
        {
            var response = new ApiResponse<AddressDto>();

            try
            {
                var state = _stateRepository.GetAll();
                var town = _townRepository.GetAll();
                var place = _placeRepository.GetAll();

                response.Result = new AddressDto();
                response.Result.estados = (from e in state
                                           join t in town on e.Id equals t.Estadoid
                                           join p in place on t.Id equals p.Municipioid
                                           where p.Cp == cp
                                           group e by e.DescEstado into g
                                           //select new StateDto { id_state = g.Select(i => i.Id).SingleOrDefault(), description_state = g.Select(i => i.DescEstado).SingleOrDefault() }).ToList();
                                           select new StateDto { id_state = g.FirstOrDefault().Id, description_state = g.FirstOrDefault().DescEstado }).ToList();
                response.Result.municipios = (from e in state
                                              join t in town on e.Id equals t.Estadoid
                                              join p in place on t.Id equals p.Municipioid
                                              where p.Cp == cp
                                              group t by t.Id into g
                                              select new TownDto { id_town = g.Key, description_town = g.FirstOrDefault(c => c.Id == g.Key).DescMunicipio }).ToList();
                response.Result.localidades = (from e in state
                                               join t in town on e.Id equals t.Estadoid
                                               join p in place on t.Id equals p.Municipioid
                                               where p.Cp == cp
                                               select new PlaceDto { id_place = p.Id, description_place = p.DescLocalidad }).ToList();
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

        [HttpPost]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<bool>> SaveAddress(SaveAddressDto address)
        {
            var response = new ApiResponse<bool>();

            try
            {
                address.Actualizado = DateTime.Now;
                address.Creado = DateTime.Now;
                address.Estatus = true;
                address.IdPrefijoCalle = 0;
                address.TipoDireccion = 1;


                CatDireccion catAddress = _addressRepository.Add(_mapper.Map<CatDireccion>(address));
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

        [HttpGet("{id_client}", Name = "GetAddressById")]
        public ActionResult<ApiResponse<List<GetAddressDto>>> GetAddressById(int id_client)
        {
            var response = new ApiResponse<List<GetAddressDto>>();

            try
            {

                //response.Result = (from a in _addressRepository.GetAll()
                //                   join s in _stateRepository.GetAll() on a.IdEstado equals s.Id
                //                   join t in _townRepository.GetAll() on a.IdMunicipio equals t.Id
                //                   join p in _placeRepository.GetAll() on a.IdLocalidad equals p.Id
                //                   where a.IdCliente == id_client
                //                   select new GetFullAddressDto { id_adress = a.Id, address = a.Cp + " " + a.CalleNumero + " " + a.NumInt + " " + a.NumExt + " " + p.DescLocalidad + " " + t.DescMunicipio + " " + s.DescEstado }).ToList();
                response.Result = _addressRepository.getAdressFull(id_client);
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

        [HttpGet(Name = "GetAddressProductsByClient")]
        public ActionResult<ApiResponse<List<ProductAddressDto>>> GetAddressProductsByClient(int idClient, int idUser)
        {
            var response = new ApiResponse<List<ProductAddressDto>>();

            try
            {
                var addresses = _addressRepository.getAdress(idClient);
                var products = (from ps in _productUserRepository.GetAll()
                                join cp in _productRepository.GetAll() on ps.IdCatSublineaProducto equals cp.Id
                                where ps.IdUser == idUser
                                select new ProductDTO { idAdress = ps.IdDireccioin, productName = cp.Descripcion, quantity = ps.Cantidad, idProduct = ps.IdCatSublineaProducto, lineId = cp.IdLineaProducto }).ToList();
                foreach (var address in addresses)
                {
                    address.products = new List<ProductDTO>();
                    foreach (var product in products)
                    {
                        if (address.id_adress == product.idAdress)
                        {
                            address.products.Add(product);
                        }
                    }
                }

                response.Result = addresses;
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

    }
}
