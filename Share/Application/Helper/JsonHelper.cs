using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class JsonHelper
    {
        public static string ConvertToJson(this object obj)
        {
            if (obj == null)
                return string.Empty;
            return JsonConvert.SerializeObject(obj);
        }
        static bool ValidateJson<T>(this string text) where T : class
        {
            try
            {
                JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public static T ConvertToObject<T>(this string text) where T : class
        {
            if (text.ValidateJson<T>())
                return JsonConvert.DeserializeObject<T>(text);
            return null;
        }
    }
}
