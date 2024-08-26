using System;
using System.ComponentModel.DataAnnotations;

namespace template.api.Models
{
    public class UserUpdateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Paterno { get; set; }
        [Required]
        public string Materno { get; set; }
        [Required]
        public string Telefono { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [StringLength(18, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        public DateTime Actualizado { get; set; }
        public long Actualizadopor { get; set; }
        public string Avatar { get; set; }
        public DateTime Creado { get; set; }
        public long Creadopor { get; set; }
        public bool Estatus { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int IdSucursales { get; set; }
        public long IdApp { get; set; }
        public int IdCanal { get; set; }
        public int IdCuenta { get; set; }
        public long IdRol { get; set; }
        public string Nivel { get; set; }
        public string TelefonoMovil { get; set; }
        public string Username { get; set; }
    }
}
