using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Models
{
    public class JsonResponse<T> where T : new()
    {
        public JsonResponse()
        {
            Respuesta = new T();
            ok = true;
        }

        public bool ok { get; set; }
        public string Mensaje { get; set; }
        public T Respuesta { get; set; }

    }
}