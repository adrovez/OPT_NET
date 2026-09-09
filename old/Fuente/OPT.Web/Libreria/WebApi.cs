using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OPT.Web.Libreria
{
    internal static class WebApi<T> where T : new()
    {

        internal static T LlamarWebApi(string url, object parametros = null, EnumVerboHttp tipo = EnumVerboHttp.Get, string Token = null)
        {

            try
            {
                var stringParametros = String.Empty;
                RestClient client;
                RestRequest request;
                var requestUri = url;
                bool Header = false;

                switch (tipo)
                {
                    case EnumVerboHttp.Get:
                        requestUri = parametros == null ? url : $"{url}?{GetQueryString(parametros)}";
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.GET);
                        break;
                    case EnumVerboHttp.Post:
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.POST);
                        Header = true;
                        break;
                    case EnumVerboHttp.Put:
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.PUT);
                        Header = true;
                        break;
                    case EnumVerboHttp.Delete:
                        requestUri = parametros == null ? url : $"{url}?{GetQueryString(parametros)}";
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.DELETE);
                        break;
                    default:
                        throw new Exception($"valor de parametro tipo '{tipo}' es invalido");
                }

                client.Timeout = -1;

                if (parametros != null)
                    stringParametros = JsonConvert.SerializeObject(parametros);


                if (Token != null)
                {
                    request.AddHeader("Authorization", "bearer " + Token);
                }

                if (Header)
                {
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", stringParametros, ParameterType.RequestBody);

                }


                IRestResponse response = client.Execute(request);

                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception ex)
            {
                LogError.setLogExeption(ex, "WebApi - LlamarWebApi");
                throw ex;
            }
        }

        internal static async Task<T> LlamarWebApiAsync(string url, object parametros = null, EnumVerboHttp tipo = EnumVerboHttp.Get, string Token = null)
        {

            try
            {
                var stringParametros = String.Empty;
                RestClient client;
                RestRequest request;
                var requestUri = url;
                bool Header = false;

                switch (tipo)
                {
                    case EnumVerboHttp.Get:
                        requestUri = parametros == null ? url : $"{url}?{GetQueryString(parametros)}";
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.GET);
                        break;
                    case EnumVerboHttp.Post:
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.POST);
                        Header = true;
                        break;
                    case EnumVerboHttp.Put:
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.PUT);
                        Header = true;
                        break;
                    case EnumVerboHttp.Delete:
                        requestUri = parametros == null ? url : $"{url}?{GetQueryString(parametros)}";
                        client = new RestClient(requestUri);
                        request = new RestRequest(Method.DELETE);
                        break;
                    default:
                        throw new Exception($"valor de parametro tipo '{tipo}' es invalido");
                }

                client.Timeout = -1;

                if (parametros != null)
                    stringParametros = JsonConvert.SerializeObject(parametros);


                if (Token != null)
                {
                    request.AddHeader("Authorization", "bearer " + Token);
                }

                if (Header)
                {
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", stringParametros, ParameterType.RequestBody);

                }


                IRestResponse response = await client.ExecuteAsync(request);

                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception ex)
            {
                LogError.setLogExeption(ex, "WebApi - LlamarWebApiAsync");
                throw ex;
            }
        }

        /// <summary>
        /// Formatea los parametros para agregarlos a la URL de la API
        /// </summary>
        /// <param name="obj">parametros a formatear</param>
        /// <returns>URL de parametros formateada para ingresarla a la URL de la api</returns>
        private static string GetQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + (("") + ((HttpUtility.UrlEncode(p.GetValue(obj, null).ToString())) == "" ? "" : HttpUtility.UrlEncode(p.GetValue(obj, null).ToString())) + ("")).Replace("+", " ");

            return String.Join("&", properties.ToArray());
        }

    }
}