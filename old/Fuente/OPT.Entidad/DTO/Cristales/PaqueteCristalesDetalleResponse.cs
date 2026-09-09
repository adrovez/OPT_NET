using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Entidad.DTO.Cristales
{
    public class PaqueteCristalesDetalleResponse
    {
        public long idOT { get; set; }
        public string Sucursal { get; set; }
        public string Empresa { get; set; }
        public string Beneficiario { get; set; }
        public string RutCliente { get; set; }
        public string Cliente { get; set; }
        public Nullable<bool> CheckLejos { get; set; }
        public Nullable<bool> CheckCerca { get; set; }
        public Nullable<bool> CheckCristalesLaboratorio { get; set; }
        public Nullable<bool> CheckUrgente { get; set; }
    }
}
