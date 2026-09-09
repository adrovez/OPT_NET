using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Entidad.DTO
{
    public class OPT_UsuarioDTO
    {
        public string RutUsuario { get; set; }
        public string Nombre { get; set; }
        public string Mail { get; set; }
        public string Clave { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public List<OPT_Sucursal> Sucursal { get; set; }
        public int idRol { get; set; }
        public string Rol { get; set; }
    }
}
