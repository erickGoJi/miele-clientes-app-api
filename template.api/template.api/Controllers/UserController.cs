using System;
using System.Collections.Generic;
using AutoMapper;
using template.api.ActionFilter;
using template.api.Models;
using template.biz.Entities;
using template.biz.Paged;
using template.biz.Repository;
using template.biz.Servicies;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace template.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IUserRepository _userRepository;

        public UserController(
            IMapper mapper,
            ILoggerManager logger,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<ApiResponse<List<UserDto>>> GetAll()
        {
            var response = new ApiResponse<List<UserDto>>();

            try
            {
                response.Result = _mapper.Map<List<UserDto>>(_userRepository.GetAll());
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

        [HttpGet("{pageNumber}/{pageSize}")]
        public ActionResult<ApiResponse<PagedList<UserDto>>> GetPaged(int pageNumber, int pageSize)
        {
            var response = new ApiResponse<PagedList<UserDto>>();

            try
            {
                response.Result = _mapper.Map<PagedList<UserDto>>
                    (_userRepository.GetAllPaged(pageNumber, pageSize));
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

        [HttpGet("{id}", Name = "GetUser")]
        public ActionResult<ApiResponse<UserDto>> GetById(int id)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                response.Result = _mapper.Map<UserDto>(_userRepository.Find(c => c.Id == id));
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

        [HttpGet("{email}", Name = "GetUserByEmail")]
        public ActionResult<ApiResponse<UserDto>> GetByEmail(string email)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                response.Result = _mapper.Map<UserDto>(_userRepository.Find(c => c.Email == email));
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

        [HttpGet("emailRecover", Name = "RecoveryPassword")]
        public ActionResult<ApiResponse<UserDto>> RecoverPassword(string email)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                Users user = new Users();
                user.Email = email;
                user = _userRepository.Find(c => c.Email == email && c.IdApp == 3);
                if (user != null)
                {
                    user = _userRepository.change_passwor(user);
                    response.Result = _mapper.Map<UserDto>(user);

                    StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/Email.html"));
                    string body = string.Empty;
                    body = reader.ReadToEnd();
                    //body = body.Replace("{username}", $"Hola {user.Name} {response.Result.Paterno} {response.Result.Materno} Te hemos asignado el siguiente password provisional");
                    //body = body.Replace("{pass}", $" por favor cambialo en tú perfil al entrar a la aplicación. Password: {response.Result.Password}");                   
                    body = body.Replace("{username}", $"{user.Name} {response.Result.Paterno} {response.Result.Materno}");
                    body = body.Replace("{password}", $"{response.Result.Password}");
                    //body = body.Replace("{username}", $"{user.Name} {user.Paterno} {user.Materno}");
                    body = body.Replace("{id}", $"{user.Id}");
                    EmailService emailService = new EmailService();
                    emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Recupera contraseña App Miele", To = user.Email });
                    response.Success = true;
                    response.Message = "Se ha enviado a tú email la nueva contraseña";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Correo no registrado";
                }

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

        [HttpGet("{email}/{password}", Name = "Login")]
        public ActionResult<ApiResponse<UserDto>> Login(string email, string password)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                response.Result = _mapper.Map<UserDto>(_userRepository.Find(c => c.Email == email && _userRepository.VerifyPassword(c.Password, password) && c.IdApp == 3));

                if (response.Result != null)
                {
                    response.Result.idClient = _userRepository.findClient(response.Result.Id);
                    _userRepository.change_DateUpdate(response.Result.Email);
                    response.Success = true;
                    response.Message = "Se logeo";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Usuario y/o contraseña incorrecta";

                }

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

        [HttpPost("loginUser", Name = "loginUser")]
        public ActionResult<ApiResponse<UserDto>> loginUser([FromBody] UserDto _user)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                response.Result = _mapper.Map<UserDto>(_userRepository.Find(c => c.Email == _user.Email && _userRepository.VerifyPassword(c.Password, _user.Password) && c.IdApp == 3));

                if (response.Result != null)
                {
                    response.Result.idClient = _userRepository.findClient(response.Result.Id);
                    _userRepository.change_DateUpdate(response.Result.Email);
                    response.Success = true;
                    response.Message = "Se logeo";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Usuario y/o contraseña incorrecta";

                }

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
        [HttpGet("loginUserModel", Name = "loginUserModel")]
        public ActionResult<ApiResponse<UserDto>> loginUserModel()
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                response.Result = new UserDto() { };
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
        public ActionResult<ApiResponse<UserDto>> Create(UserCreateDto item)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                if (_userRepository.Exists(c => c.Email == item.Email))
                {
                    response.Success = false;
                    response.Message = $"Email: { item.Email } Already Exists";
                    //return BadRequest(response);
                    return StatusCode(200, response);
                }

                item.Actualizado = DateTime.Now;
                item.Actualizadopor = 0;
                item.Creado = DateTime.Now;
                item.Creadopor = 0;
                item.Estatus = false;
                item.FechaNacimiento = DateTime.Now;
                item.IdSucursales = 3;
                item.IdApp = 3;
                item.IdCanal = 3;
                item.IdCuenta = 3;
                item.IdRol = 10012;
                item.VisibleTickets = false;

                Users user = _userRepository.Add(_mapper.Map<Users>(item));
                Clientes client = new Clientes();
                client.IdSucursal = 0;
                client.Referidopor = 0;
                client.Actualizado = DateTime.Now;
                client.Actualizadopor = 0;
                client.Creado = DateTime.Now;
                client.Creadopor = 0;
                client.Email = user.Email;
                client.Estatus = true;
                client.Materno = user.Materno;
                client.Nombre = user.Name;
                client.Paterno = user.Paterno;
                client.TipoCliente = 1;
                client.NombreComercial = "";
                client.VisibleTickets = false;
                client.TipoPersona = "Persona fisica";
                client.Telefono = item.Telefono;

                long id_client = _userRepository.createClient(client, user.Id);
                response.Result = _mapper.Map<UserDto>(user);

                EmailService emailService = new EmailService();
                StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailConfirm.html"));
                string body = string.Empty;
                body = reader.ReadToEnd();
                body = body.Replace("{username}", $"{item.Name} {item.Paterno} {item.Materno}");
                body = body.Replace("{id}", $"{response.Result.Id}");

                emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Bienvenido a Miele App Clientes", To = user.Email });

                _userRepository.notificarRegistro(user);
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
        [HttpGet("notificationValidation", Name = "notificationValidation")]
        public bool notificationValidation([FromQuery]long idUser)
        {

            try
            {
                var item = _userRepository.Find(c => c.Id == idUser);

                EmailService emailService = new EmailService();
                var guid = Guid.NewGuid().ToString().Substring(0, 4);
                StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailConfirm.html"));
                string body = string.Empty;
                body = reader.ReadToEnd();
                body = body.Replace("{username}", $"{item.Name} {item.Paterno} {item.Materno}");
                body = body.Replace("{id}", $"{idUser}");

                emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Validación de Cuenta.", To = item.Email });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        [HttpPost("updatePassword", Name = "updatePassword")]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<UserDto>> updatePassword(Users item)
        {
            var response = new ApiResponse<UserDto>();

            try
            {

                bool user = _userRepository.updatePassword(item);

                //EmailService emailService = new EmailService();
                //var guid = Guid.NewGuid().ToString().Substring(0, 4);

                //StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/Email.html"));
                //string body = string.Empty;
                //body = reader.ReadToEnd();
                //body = body.Replace("{username}", $"Hola {item.Name} {item.Paterno} {item.Materno} Bienvenido a Miele App para clientes");
                //body = body.Replace("{pass}", $"Por favor verifica tu cuenta dando click aqui");
                //body = body.Replace("{id}", $"{response.Result.Id}");

                //emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Bienvenido a Miele App Clientes", To = user.Email });
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

        [HttpGet("confirmAccount", Name = "confirmAccount")]
        public ActionResult<bool> confirmAccount(int id)
        {
            bool response = false;

            try
            {
                response = _userRepository.confirmAccountStatusUpdate(id);

            }
            catch (Exception ex)
            {
                response = false;

                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPut("{id}")]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<UserDto>> Update(int id, UserUpdateDto item)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var user = _userRepository.Find(c => c.Id == id);

                if (user == null)
                {
                    response.Message = $"User id { id } Not Found";
                    return NotFound(response);
                }

                _mapper.Map<Users>(item);
                _userRepository.Save();
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

        [HttpDelete("{id}")]
        public ActionResult<ApiResponse<UserDto>> Delete(int id)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var user = _userRepository.Find(c => c.Id == id);

                if (user == null)
                {
                    response.Message = $"User id { id } Not Found";
                    return NotFound(response);
                }

                _userRepository.Delete(user);
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
