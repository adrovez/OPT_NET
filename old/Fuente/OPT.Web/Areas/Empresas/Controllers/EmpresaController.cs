using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Configuration;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using PagedList;
using Microsoft.Reporting.WebForms;


namespace OPT.Web.Areas.Empresas.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class EmpresaController : Controller
    {
        SessionDTO mySession;
        public EmpresaController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ==================== Listar ====================
        // GET: Empresa/Empresa
        public ActionResult Index()
        {

            OPT_EmpresaDAL Datos = new OPT_EmpresaDAL();
            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());

                var Lista = Datos.Lista("", mySession.idSucursal);
                return View(Lista.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Paginar(FormCollection myForm)
        {
            OPT_EmpresaDAL Datos = new OPT_EmpresaDAL();

            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                if (Request.QueryString["pPageNumber"] != null)
                {
                    page = int.Parse(Request.QueryString["pPageNumber"]);
                }

                var pNombre = myForm["Nombre"].ToString();

                var Lista = Datos.Lista(pNombre, mySession.idSucursal);

                return PartialView("_ParcialListaEmpresa", Lista.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }


        }

        #endregion

        #region ==================== CREATE ====================

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
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
        public ActionResult Create(OPT_Empresa Item)
        {
            OPT_EmpresaDAL Datos = new OPT_EmpresaDAL();
            OPT_EmpresaSucursalDAL empresaSucursalDAL = new OPT_EmpresaSucursalDAL();
            List<OPT_EmpresaSucursal> _EmpresaSucursal = new List<OPT_EmpresaSucursal>();

            try
            {
                Item.Empresa = Item.Empresa.ToUpper();
                Item.Direccion = Item.Direccion.ToUpper();
                Item.Contacto = Item.Contacto == null ? "" : Item.Contacto.ToUpper();

                Datos.Insertar(Item);

                if (mySession.idRol == 1)
                {
                    foreach (var x in mySession.Sucursal)
                    {
                        _EmpresaSucursal.Add(new OPT_EmpresaSucursal
                        {
                            idEmpresa = Item.idEmpresa,
                            idSucursal = x.idSucursal

                        });
                    }
                }
                else
                {
                    foreach (var x in mySession.Sucursal)
                    {
                        _EmpresaSucursal.Add(new OPT_EmpresaSucursal
                        {
                            idEmpresa = Item.idEmpresa,
                            idSucursal = x.idSucursal
                        });
                    }

                    //Si el usuario conectado no tiene asignado la casa mastriz
                    if(!mySession.Sucursal.Any(x=>x.Matriz == true))
                    {
                        _EmpresaSucursal.Add(new OPT_EmpresaSucursal
                        {
                            idEmpresa = Item.idEmpresa,
                            idSucursal = 1 // Casa Matring
                        });
                    }
                }


                empresaSucursalDAL.Insertar(_EmpresaSucursal);


                return RedirectToAction("Index", "Empresa");
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }

        }

        #endregion

        #region ==================== EDITAR ====================

        [HttpGet]
        public ActionResult Edit(int id)
        {
            OPT_EmpresaDAL Dato = new OPT_EmpresaDAL();
            try
            {
                var Item = Dato.Buscar(id);

                return View(Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OPT_Empresa Item)
        {
            OPT_EmpresaDAL Datos = new OPT_EmpresaDAL();
            try
            {
                Item.Empresa = Item.Empresa.ToUpper();
                Item.Direccion = Item.Direccion.ToUpper();
                Item.Contacto = Item.Contacto == null ? "" : Item.Contacto.ToUpper();

                Datos.Editar(Item);

                return RedirectToAction("Index", "Empresa");
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }

        }

        #endregion

        [HttpGet]
        public void Reporte()
        {
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsEmpresaListaTableAdapters.OPT_EmpresaTableAdapter ds = new Ds.dsEmpresaListaTableAdapters.OPT_EmpresaTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsEmpresaLista";
                rds.Value = ds.GetData(mySession.idSucursal);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Empresas/Rpt/rptEmpresaLista.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "Empresas_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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