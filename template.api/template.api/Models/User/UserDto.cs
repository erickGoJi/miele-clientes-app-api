using System;

namespace template.api.Models
{
    public class UserDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Paterno { get; set; }
        
        public string Materno { get; set; }
       
        public string Telefono { get; set; }
        
        public string Email { get; set; }
        
       
        public string Password { get; set; }

        public DateTime Actualizado { get; set; }
        public long Actualizadopor { get; set; }

        public DateTime Creado { get; set; }
        public long Creadopor { get; set; }
        public bool Estatus { get; set; }
        public long idClient { get; set; }
    }
}
