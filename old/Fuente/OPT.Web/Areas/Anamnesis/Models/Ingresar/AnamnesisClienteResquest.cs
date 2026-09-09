using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Anamnesis.Models.Ingresar
{
    public class AnamnesisClienteResquest
    {
        public long IdAnamnesis { get; set; }

        public string RutCliente { get; set; }
        public string Nombre { get; set; }
        public int idEmpresa { get; set; }
        public string Direccion { get; set; }
        public Nullable<int> idComuna { get; set; }
        public string Celular { get; set; }
        public string Mail { get; set; }
        public Nullable<System.DateTime> FechaIngreso { get; set; }
    }
}