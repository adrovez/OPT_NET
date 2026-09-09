using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data;
using System.IO;
using System.Configuration;
using Microsoft.Reporting.WebForms;

using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;


namespace OPT.Web.Areas.OrdenTrabajo.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class DeudaController : Controller
    {
        SessionDTO mySession;
        public DeudaController()
        {           
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ==================== LISTAR ====================
        // GET: OrdenTrabajo/Deuda
        public ActionResult Index()
        {          
            OPT_OrdenDeTrabajoDAL Dato = new OPT_OrdenDeTrabajoDAL();                 

            try
            {
                var Lista = Dato.ListaDeudores();
                return View(Lista);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpGet]
        public void Reporte()
        {
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsListarDeudaTableAdapters.sp_ListaDeudoresTableAdapter ds = new Ds.dsListarDeudaTableAdapters.sp_ListaDeudoresTableAdapter();


            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsListarDeuda";
                rds.Value = ds.GetData();

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/OrdenTrabajo/Rpt/rptDeudaListar.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("PDF"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "ListaDeudores_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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

        #endregion

        #region ==================== DETALLE ====================

        [HttpGet]
        public PartialViewResult Details(int id)
        {           
            OPT_EmpresaDAL DatosEmpresa = new OPT_EmpresaDAL();
            OPT_PagoDAL DatoPago = new OPT_PagoDAL();
            try
            {
               ViewBag.Empresa = DatosEmpresa.Buscar(id);
                var Lista = DatoPago.DetalleDeudores(id);

                return PartialView("_ModalDetails", Lista);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpGet]
        public void ReporteDetalleDeudores(int id)
        {
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsListaDeudoresDetalleTableAdapters.sp_DetalleDeudoresTableAdapter ds = new Ds.dsListaDeudoresDetalleTableAdapters.sp_DetalleDeudoresTableAdapter();


            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsListaDeudoresDetalle2";
                rds.Value = ds.GetData(id);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/OrdenTrabajo/Rpt/rptListaDeudoresDetalle2.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("EXCEL"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "ListaDetalleDeudores_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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

        #endregion



    }
}
