using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Entidad.DTO
{
    public class SessionDTO
    {
        public SessionDTO()
        {
            Sucursal = new List<OPT_Sucursal>();
            Defontana = new OPT_Defontana();
        }

        public long idUsuario { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string Mail { get; set; }
        public string Celular { get; set; }
        public List<OPT_Sucursal> Sucursal { get; set; }
        public int idSucursal { get; set; }
        public int idRol { get; set; }
        public OPT_Defontana Defontana { get; set; }
        public string Token { get; set; }
    }
}
