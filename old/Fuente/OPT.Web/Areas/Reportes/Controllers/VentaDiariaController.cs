using Microsoft.Reporting.WebForms;
using OPT.Dato.DAL;
using OPT.Entidad.DTO;
using System;
using System.Web.Mvc;

namespace OPT.Web.Areas.Reportes.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class VentaDiariaController : Controller
    {
        SessionDTO mySession;
        public VentaDiariaController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        // GET: Reportes/Reporte
        public ActionResult Index()
        {
            OPT_EmpresaDAL DatoEmpresa = new OPT_EmpresaDAL();
            try
            {
                ViewBag.Empresa = new SelectList(DatoEmpresa.ComboEmpresa(mySession.idSucursal), "idEmpresa", "Empresa");

                return View();
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Reporte(FormCollection myForm)
        {
            try
            {
                Ds.dsVentaDiariaTableAdapters.rptVentaDiariaTableAdapter dt = new Ds.dsVentaDiariaTableAdapters.rptVentaDiariaTableAdapter();

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string extension = "xls";

                ReportViewer reportViewer1 = new ReportViewer();
                ReportDataSource rds = new ReportDataSource();
               
                DateTime? pFechaDesde = null;
                DateTime? pFechaHasta = null;

                pFechaDesde = DateTime.Parse(myForm["pFechaDesde"].ToString());
                pFechaHasta = DateTime.Parse(myForm["pFechaHasta"].ToString());

                rds.Name = "dsVentaDiaria";
                rds.Value = dt.GetData(pFechaDesde
                                      , pFechaHasta);

                reportViewer1.ProcessingMode = ProcessingMode.Local;
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);
                reportViewer1.LocalReport.ReportPath = "Areas/Reportes/Rpt/rptVentasDiarias.rdlc";
                reportViewer1.LocalReport.Refresh();


                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                                , null
                                                                , out mimeType
                                                                , out encoding
                                                                , out extension
                                                                , out streamids
                                                                , out warnings);

                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.BufferOutput = true;
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + "VentasDiarias" + "_" + DateTime.Now.ToShortDateString() + ".xls");

                Response.BinaryWrite(bytes);
                Response.Flush();
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }


    }
}