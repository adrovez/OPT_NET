using Microsoft.Reporting.WebForms;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using OPT.Web.Areas.Anamnesis.Controllers;
using OPT.Web.Areas.Clientes.Models;
using OPT.Web.Areas.OrdenTrabajo.Controllers;
using OPT.Web.Models;
using System;
using System.Web.Mvc;

namespace OPT.Web.Areas.Clientes.Controllers
{
    [Authorize]
    [Libreria.Autenticado]
    public class AtencionController : Controller
    {
        SessionDTO mySession;

        public AtencionController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }
        // GET: Clientes/Atencion
        public ActionResult Index()
        {
            OPT_AtencionDAL atencionDAL = new OPT_AtencionDAL();
            DateTime fechaHoy = DateTime.Parse(DateTime.Now.ToShortDateString());

            var ListAtencion = atencionDAL.Lista(fechaHoy);

            return View(ListAtencion);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AtencionRequest item)
        {
            IngresoController ingresoController = new IngresoController();
            AnamnesiController anamnesiController = new AnamnesiController();
            OPT_AtencionDAL atencionDAL = new OPT_AtencionDAL();
            OPT_Atencion atencion = new OPT_Atencion();
            JsonResponse<int> jsonResponse = new JsonResponse<int>();

            try
            {
                item.receta.RutCliente = item.cliente.RutCliente.ToUpper().Replace(".", "");
                item.receta.FechaIngreso = Libreria.Util.getFechaZona(); // DateTime.Now;

                item.anamnesis.RutCliente = item.cliente.RutCliente.ToUpper().Replace(".", "");
                item.anamnesis.idEmpresa = item.cliente.idEmpresa == null ? 1 : int.Parse(item.cliente.idEmpresa.ToString());

                var responseCliente = ingresoController.ClienteInsertar(item.cliente);
                var responseReceta = ingresoController.RecetaInsertar(item.receta);
                var responseAnamnesis = anamnesiController.AnamnesisInsertar(item.anamnesis);

                atencion.RutCliente = responseCliente.RutCliente;
                atencion.Responsable = mySession.Nombre;
                atencion.FechaAtencion = Libreria.Util.getFechaZona(); // DateTime.Now;
                atencion.idAnamnesis = responseAnamnesis;
                atencion.idRecetaCristales = responseReceta.idRecetaCristales;

                atencionDAL.Insertar(atencion);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                jsonResponse.ok = false;
                jsonResponse.Mensaje = ex.Message;                
            }

            return Json(jsonResponse,JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public void Reporte()
        {
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsAtencionTableAdapters.dtAtencionTableAdapter ds = new Ds.dsAtencionTableAdapters.dtAtencionTableAdapter();
            AtencionReporteRequest item = new AtencionReporteRequest();
            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {

                item.Desde = DateTime.Parse(DateTime.Now.ToShortDateString());
                item.Hasta = DateTime.Parse(DateTime.Now.ToShortDateString());

                rds.Name = "dsAtencion";
                rds.Value = ds.GetData(item.Desde, item.Hasta);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Clientes/Rpt/rptAtencion.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "Atencion_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.BufferOutput = true;
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + NomArchivo);

                Response.BinaryWrite(bytes);                            // crea el archivo....
                Response.Flush();

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ReporteAtencion(AtencionReporteRequest item)
        {
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsAtencionTableAdapters.dtAtencionTableAdapter ds = new Ds.dsAtencionTableAdapters.dtAtencionTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsAtencion";
                rds.Value = ds.GetData(item.Desde, item.Hasta);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Clientes/Rpt/rptAtencion.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "Atencion_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.BufferOutput = true;
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + NomArchivo);

                Response.BinaryWrite(bytes);                            // crea el archivo....
                Response.Flush();

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
            }
        }

    }
}