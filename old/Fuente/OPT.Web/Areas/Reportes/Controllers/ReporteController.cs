using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;


using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using Microsoft.Reporting.WebForms;
using OPT.Web.Libreria;

namespace OPT.Web.Areas.Reportes.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class ReporteController : Controller
    {
        SessionDTO mySession;
        public ReporteController()
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
                int idEmpresa = myForm["idEmpresa"].ToString() == ""? 0 : Convert.ToInt32(myForm["idEmpresa"].ToString());
                int TipoReporte = Convert.ToInt32(myForm["TipoReporte"].ToString());
                DateTime FechaDesde = DateTime.Parse(myForm["FechaDesde"].ToString());
                DateTime FechaHasta = DateTime.Parse(myForm["FechaHasta"].ToString());

                switch (TipoReporte)
                {
                    case 1:
                        ReporteCristales(idEmpresa, FechaDesde, FechaHasta);
                        break;

                    case 2:
                        ReporteCuota(idEmpresa, FechaDesde, FechaHasta);
                        break;

                    case 3:
                        ReporteConsolidadoAnual(FechaDesde, FechaHasta);
                        break;

                    case 4:
                        ReporteConsolidadoTipoPago(FechaDesde, FechaHasta);
                        break;
                        

                }
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
            }
        }

        public void ReporteCristales(int id, DateTime pFechaDesde, DateTime pFechaHasta)
        {

            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsListaCristalesTableAdapters.OPT_OrdenTrabajoAdapter ds = new Ds.dsListaCristalesTableAdapters.OPT_OrdenTrabajoAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();
            OPT_BitacoraOtDAL oPT_BitacoraOtDAL = new OPT_BitacoraOtDAL();
            List<OPT_BitacoraOT> ListabitacoraOT = new List<OPT_BitacoraOT>();

            try
            {
                var OTs = ds.GetData(id, pFechaDesde.ToShortDateString(), pFechaHasta.ToShortDateString());

                rds.Name = "dsListaCristales";
                rds.Value = OTs;

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Reportes/Rpt/rptListaCristales.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "ListaCristales_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;



                if (OTs.Count > 0)
                {
                    OPT_BitacoraOT bitacoraOT;
                    foreach (var item in OTs)
                    {
                        bitacoraOT = new OPT_BitacoraOT();
                        bitacoraOT.idOT = item.idOT;
                        bitacoraOT.idEstado = (int)EnumEstadoOt.Montaje;
                        bitacoraOT.Fecha = DateTime.Now;
                        bitacoraOT.Responsable = mySession.Nombre;
                        bitacoraOT.Observacion = "Enviado a Cristales";
                        ListabitacoraOT.Add(bitacoraOT);
                    }
                    oPT_BitacoraOtDAL.Insertar(ListabitacoraOT);
                }

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
                throw ex;
            }

        }

        public void ReporteCuota(int id, DateTime pFechaDesde, DateTime pFechaHasta)
        {

            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsListaCuotasTableAdapters.rpt_ListaCuotaTableAdapter ds = new Ds.dsListaCuotasTableAdapters.rpt_ListaCuotaTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsListaCuota";
                rds.Value = ds.GetData(id, pFechaDesde, pFechaHasta);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Reportes/Rpt/rptListaCuota.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "ListaCuota_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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
                throw ex;
            }

        }

        public void ReporteConsolidadoAnual(DateTime pFechaDesde, DateTime pFechaHasta)
        {

            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsConsolidadoTableAdapters.rpt_ConsolidadoAnualTableAdapter ds = new Ds.dsConsolidadoTableAdapters.rpt_ConsolidadoAnualTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsConsolidadoAnual";
                rds.Value = ds.GetData(pFechaDesde.Year, pFechaHasta.Year);

                ReportParameter[] reportParameters = new ReportParameter[2];
                reportParameters[0] = new ReportParameter("pFechaInicio", pFechaDesde.Year.ToString(),false);
                reportParameters[1] = new ReportParameter("pFechaFin", pFechaHasta.Year.ToString(),false);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);                
                reportViewer1.LocalReport.ReportPath = "Areas/Reportes/Rpt/rptConsolidadoAnual.rdlc";
                reportViewer1.LocalReport.SetParameters(reportParameters);
                reportViewer1.LocalReport.Refresh();


                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "ConsolidadoAnual_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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
                throw ex;
            }

        }

        public void ReporteConsolidadoTipoPago(DateTime pFechaDesde, DateTime pFechaHasta)
        {

            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsConsolidadoTableAdapters.rpt_ConsolidadoTipoPagoTableAdapter ds = new Ds.dsConsolidadoTableAdapters.rpt_ConsolidadoTipoPagoTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsConsolidadoTipoPago";
                rds.Value = ds.GetData(pFechaDesde.Year, pFechaHasta.Year);

                ReportParameter[] reportParameters = new ReportParameter[2];
                reportParameters[0] = new ReportParameter("pFechaDesde", pFechaDesde.ToString("MM/yyyy"), false);
                reportParameters[1] = new ReportParameter("pFechaHasta", pFechaHasta.ToString("MM/yyyy"), false);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);
                reportViewer1.LocalReport.ReportPath = "Areas/Reportes/Rpt/rptConsolidadoTipoPago.rdlc";
                reportViewer1.LocalReport.SetParameters(reportParameters);
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "ConsolidadoTipoPago_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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
                throw ex;
            }

        }

        public ActionResult Atencion()
        {
            return View();
        }

    }

}