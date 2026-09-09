using Microsoft.Reporting.WebForms;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using PagedList;
using System;
using System.Configuration;
using System.Web.Mvc;


namespace OPT.Web.Areas.Clientes.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class ClienteController : Controller
    {
        SessionDTO mySession;

        public ClienteController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
          
        }

        #region ==================== Listar ====================

        // GET: Empresa/Empresa
        public ActionResult Index()
        {            
            OPT_ClienteDAL Datos = new OPT_ClienteDAL();
            OPT_EmpresaDAL DatoEmpresa = new OPT_EmpresaDAL();

            int page = 1;
            int pageSize;

            try
            {                
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
               
                ViewBag.Empresa = new SelectList(DatoEmpresa.ComboEmpresa(mySession.idSucursal), "idEmpresa", "Empresa");

                var Lista = Datos.Lista(0, "", "", mySession.idSucursal);
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
            OPT_ClienteDAL Datos = new OPT_ClienteDAL();

            int page = 1;
            int pageSize;

            try
            {               
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                if (Request.QueryString["pPageNumber"] != null)
                {
                    page = int.Parse(Request.QueryString["pPageNumber"]);
                }

                int pEmpresa = myForm["idEmpresa"] == "" ? 0 : int.Parse(myForm["idEmpresa"].ToString());
                var pRut = myForm["RutCliente"].Replace(".","").ToString().Trim();
                var pNombre = myForm["Nombre"].ToString();

                var Lista = Datos.Lista(pEmpresa, pRut, pNombre, mySession.idSucursal);

                return PartialView("_ParcialListaCliente", Lista.ToPagedList(page, pageSize));
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

            Ds.dsClienteListaTableAdapters.rpt_ClienteListaTableAdapter ds = new Ds.dsClienteListaTableAdapters.rpt_ClienteListaTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsClienteLista";
                rds.Value = ds.GetData(mySession.idSucursal);

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Clientes/Rpt/rptClienteLista.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "Clientes_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

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

        [HttpGet]
        public PartialViewResult Details(string id)
        {            
            OPT_ClienteDAL Datos = new OPT_ClienteDAL();

            try
            {
                var Item = Datos.Buscar(id);

                return PartialView("_ModalDetails", Item);
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
            OPT_RegionDAL DatoRegion = new OPT_RegionDAL();
            OPT_EmpresaDAL DatoEmpresa = new OPT_EmpresaDAL();
            try
            {
                ViewBag.Region = new SelectList(DatoRegion.Lista(), "idRegion", "Region",0);
                ViewBag.Empresa = new SelectList(DatoEmpresa.ComboEmpresa(mySession.idSucursal), "idEmpresa", "Empresa",1);

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
        public ActionResult Create(OPT_Cliente Item)
        {           
            OPT_ClienteDAL Datos = new OPT_ClienteDAL();
            try
            {
                Item.RutCliente = Item.RutCliente.Replace(".", "").ToUpper();
                Item.Nombre = Item.Nombre.ToUpper();
                Item.Direccion = Item.Direccion == null ? "" : Item.Direccion.ToUpper();
                Item.Celular = Item.Celular == null ? "" : Item.Celular.ToUpper();
                Item.Mail = Item.Mail == null ? "" : Item.Mail.ToUpper();               
                Item.FechaIngreso = DateTime.Now;               

                Datos.Insertar(Item);

                return RedirectToAction("Index", "Cliente");
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
        public ActionResult Edit(string id)
        {            
            OPT_RegionDAL DatoRegion = new OPT_RegionDAL();
            OPT_ComunaDAL DatoComuna = new OPT_ComunaDAL();
            OPT_EmpresaDAL DatoEmpresa = new OPT_EmpresaDAL();
            OPT_ClienteDAL DatoCliente = new OPT_ClienteDAL();
            try
            {
               var Item = DatoCliente.Buscar(id.ToUpper().Replace(".", ""));

                ViewBag.Region = new SelectList(DatoRegion.Lista(), "idRegion", "Region", Item.OPT_Comuna.idRegion);
                ViewBag.Comuna = new SelectList(DatoComuna.Lista(Item.OPT_Comuna.idRegion), "idComuna", "Comuna", Item.idComuna);
                ViewBag.Empresa = new SelectList(DatoEmpresa.ComboEmpresa(mySession.idSucursal), "idEmpresa", "Empresa", Item.idEmpresa);

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
        public ActionResult Edit(OPT_Cliente Item)
        {            
            OPT_ClienteDAL Datos = new OPT_ClienteDAL();
            try
            {                
                Item.Nombre = Item.Nombre.ToUpper();
                Item.Direccion = Item.Direccion == null ? "" : Item.Direccion.ToUpper();

                Datos.Editar(Item);

                return RedirectToAction("Index", "Cliente");
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }

        }

        #endregion



    }
}