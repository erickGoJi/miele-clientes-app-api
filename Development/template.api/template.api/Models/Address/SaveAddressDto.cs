using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mieleApp.api.Models.Address
{
    public class SaveAddressDto
    {
        public int Id { get; set; }
        public string FechaEstimada { get; set; }
        public DateTime Actualizado { get; set; }
        public long Actualizadopor { get; set; }
        public string CalleNumero { get; set; }
        public string Colonia { get; set; }
        public string Cp { get; set; }
        public DateTime Creado { get; set; }
        public long Creadopor { get; set; }
        public bool Estatus { get; set; }
        public long IdCliente { get; set; }
        public int IdEstado { get; set; }
        public int IdLocalidad { get; set; }
        public int IdMunicipio { get; set; }
        public int IdPrefijoCalle { get; set; }
        public string Nombrecontacto { get; set; }
        public string NumExt { get; set; }
        public string NumInt { get; set; }
        public string Telefono { get; set; }
        public string TelefonoMovil { get; set; }
        public int? TipoDireccion { get; set; }
    }
}
