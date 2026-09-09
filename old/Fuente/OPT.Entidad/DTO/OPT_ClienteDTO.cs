using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Entidad.DTO
{
    public class OPT_ClienteDTO
    {
        public string RutCliente { get; set; }
        public Nullable<int> idEmpresa { get; set; }
        public string Empresa { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public int idComuna { get; set; }
        public string Comuna { get; set; }
        public int idRegion { get; set; }
        public string Region { get; set; }
        public string Celular { get; set; }
        public string Mail { get; set; }
        public System.DateTime? FechaIngreso { get; set; }
        public System.DateTime? FechaNacimiento { get; set; }
    }
}
