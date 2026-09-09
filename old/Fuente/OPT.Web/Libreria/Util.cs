using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace OPT.Web.Libreria
{
    public static class Util
    {
        public enum Proceso
        {
             CONTESTACION = 1
            ,AUDIENCIA_PREPARATORIA = 2
            ,AUDIENCIA_JUICIO = 3

        }

        public enum TipoCalculoDia
        {
            SUMAR = 0
            , RESTAR = 1
        }

        public enum Rol
        {
            ABOGADO = 1
           , SECRETARIA = 2
        }

        public enum TipoMovimiento
        {
            SALDO = 0
           , ENTRADA = 1
           , SALIDA = 2
        }

        public static DateTime getFechaZona()
        {
            DateTime dt = new DateTime();
            var info = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            dt = localTime.DateTime;
            return dt;


        }

    }
}