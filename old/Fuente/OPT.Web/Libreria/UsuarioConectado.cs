using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OPT.Entidad.DTO;
using System.Web.Security;

namespace OPT.Web.Libreria
{
    public static class UsuarioConectado
    {

        public static void ValidaSession(ref SessionDTO mySession)
        {
            if (GetExpiraSession())
            {
                mySession = null;
            }
            else
            {
                mySession = GetMySession();
            }
        }

        /// <summary>
        /// Valida si session esta activa
        /// </summary>
        /// <returns>Retorna verdadero o falso</returns>
        public static bool GetExpiraSession()
        {
            bool expira = false;

            //if (HttpContext.Current.Session["_AnexoSession"] == null || ((bool)HttpContext.Current.Session["_AnexoSession"] != true))
            if (HttpContext.Current.Session["OPT"] == null || HttpContext.Current.Request.IsAuthenticated == false || HttpContext.Current.User == null)
            {
                ClearSession();
                expira = true;
            }

            return expira;
        }

        /// <summary>
        /// Carga session con modelo cargado
        /// </summary>
        /// <param name="pItems">Recibe Modelo como parametro</param>
        public static void SetMySession(SessionDTO pItems)
        {
            HttpContext.Current.Session["OPT"] = pItems;
        }

        /// <summary>
        /// Escribe sesion con modelo
        /// </summary>
        /// <returns></returns>
        public static SessionDTO GetMySession()
        {
            SessionDTO _Modelo = (SessionDTO)HttpContext.Current.Session["OPT"];

            return _Modelo;
        }

        public static void ClearSession()
        {
            //Mata la Autentificacion (Cookie)
            FormsAuthentication.SignOut();
            //Mata la Session.
            HttpContext.Current.Session.Clear();
        }
    }
}