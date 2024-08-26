using System.ComponentModel;

namespace template.comunicacion
{
    public enum eContentType
    {
        [Description("text")] Text,
        [Description("text/plain")] TextPlain,
        [Description("application/json; charset=utf-8")] JSON,
        [Description("application/javascript")] javaScript,
        [Description("application/xml")] XML,
        [Description("text/xml")] TextXML,
        [Description("text/html")] TextHTML
    }
    public enum eMethodType
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }
    public enum eAccept
    {
        [Description("text/html")] TextHTML,
        [Description("image/*")] ImageAll,
        [Description("text/html, application/xhtml+xml, application/xml;q=0.9, */*; q = 0.8")] AllFormat,
        [Description("application/json")] JSON,
        [Description("application/xml")] XML,
    }
    public class EnumMethods
    {
        public string getContentType(eContentType _contentType)
        {
            return ((DescriptionAttribute[])_contentType.GetType().GetField(_contentType.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false))[0].Description;
        }
        public string getAccept(eAccept _accept)
        {
            return ((DescriptionAttribute[])_accept.GetType().GetField(_accept.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false))[0].Description;
        }
        public string getMethodType(eMethodType _methodType)
        {
            return _methodType.ToString();
        }
    }
}
