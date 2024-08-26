using System;
using System.Collections.Generic;
using System.Text;

namespace mieleApp.biz.EntitiesDTO
{

    public class Notification
    {
        public string title { get; set; }
        public string body { get; set; }
        public string sound { get; set; }
        public string click_action { get; set; }
        public string icon { get; set; }
        public string image { get; set; }
        public string url { get; set; }
        public int tipo { get; set; }
        public int idVisita { get; set; }
        public int estatus { get; set; }
        public bool app { get; set; }
        public bool pagado { get; set; }
    }

    public class Info
    {
        public string mensaje { get; set; }
        public string esto { get; set; }
    }

    public class Data
    {
        public string title { get; set; }
        public string body { get; set; }
        public string landing_page { get; set; }
        public Info info { get; set; }
        public string image { get; set; }
        public string url { get; set; }
        public int tipo { get; set; }
        public int idVisita { get; set; }
        public int estatus { get; set; }
        public bool app { get; set; }
        public bool pagado { get; set; }
    }

    public class NotificacionesApp
    {
        public Notification notification { get; set; }
        public Data data { get; set; }
        public string to { get; set; }
        public string priority { get; set; }
        public string restricted_package_name { get; set; }
    }
    public class configLocal
    {
        public ConfigNotificacion ConfigNotificacion { get; set; }
        public ConfiguractionSendMail SendMail { get; set; }
    }
    public class ConfiguractionSendMail
    {
        public string url { get; set; }
        public string controller { get; set; }
        public List<Headers> headers { get; set; }
    }
    public class ConfigNotificacion
    {
        public string url { get; set; }
        public string urlImagen { get; set; }
        public string controller { get; set; }
        public List<Headers> headers { get; set; }
    }
    public class Headers
    {
        public string key { get; set; }
        public string Value { get; set; }
    }
}
