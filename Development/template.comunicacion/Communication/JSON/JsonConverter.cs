using Newtonsoft.Json;
using System.Collections.Generic;

namespace template.comunicacion
{
    public class JsonConverter
    {
        public static List<T> Json_ObjectList<T>(string _json)
        {
            return (List<T>)JsonConvert.DeserializeObject<List<T>>(_json);
        }
        public static T Json_Object<T>(string _json)
        {
            return JsonConvert.DeserializeObject<T>(_json);
        }
        public static string Object_Json(object _object)
        {
            return JsonConvert.SerializeObject(_object);
        }

        public static List<T> ReturnList<T>(List<object> objectList)
        {
            List<T> _list = new List<T>();
            foreach (var item in objectList)
            {
                _list.Add(JsonConvert.DeserializeObject<T>(item.ToString()));
            }
            return _list;
        }
    }
}
