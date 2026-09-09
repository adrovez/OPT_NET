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
using OPT.Web.Models;

namespace OPT.Web.Controllers
{
    [Authorize]
    [Libreria.Autenticado]
    public class HomeController : Controller
    {
        SessionDTO mySession;

        public HomeController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        public ActionResult Index()
        {
            OPT_EmpresaDAL DatoEmpresa = new OPT_EmpresaDAL();
            OPT_ClienteDAL DatosCliente = new OPT_ClienteDAL();
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();

            try
            {
                ViewBag.Empresa = DatoEmpresa.ComboEmpresa(mySession.idSucursal).Count();
                ViewBag.Cliente = DatosCliente.Lista(0, "", "", mySession.idSucursal).Count();
                ViewBag.OT = DatoOT.Lista(0
                                        , 0
                                        , ""                                       
                                        , mySession.idSucursal).Count();

                ViewBag.Deudores = DatoOT.ListaDeudores().Count();

                return View();
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        public JsonResult OTPeriodo()
        {
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();

            try
            {
                int Anio = DateTime.Now.Year;
                int Mes = DateTime.Now.Month;
                List<OTPeriodoPieDTO> Listado = new List<OTPeriodoPieDTO>();
                decimal Total = 0;

                if (mySession.idRol == 1)
                {
                    Decimal.TryParse(DatoOT.SumaPeriodo(Anio).ToString(), out Total);
                    Listado = DatoOT.ListaPeriodoPie(Anio);
                }
                else
                {
                    Decimal.TryParse(DatoOT.SumaMes(Anio, Mes, mySession.idSucursal).ToString(), out Total);
                    Listado = DatoOT.PeriodoMes(Anio, Mes, mySession.idSucursal);
                }

                if (Listado.Count == 0)
                {
                    OTPeriodoPieDTO Item = new OTPeriodoPieDTO();
                    Item.value = "";
                    Item.label = "";
                    Listado.Add(Item);
                }

                return Json(new { ok = true, respuesta = Listado, total = Total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult OTPeriodo2()
        {
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();

            try
            {
                List<OTPeriodoPieDTO> Listado = new List<OTPeriodoPieDTO>();               
                decimal Total = 0;

                if (mySession.idRol == 1)
                {
                    int Anio = DateTime.Now.Year - 1;
                    Decimal.TryParse(DatoOT.SumaPeriodo(Anio).ToString(), out Total);
                    Listado = DatoOT.ListaPeriodoPie(Anio);
                }
                else
                {
                    DateTime Feccha = DateTime.Now.AddMonths(-1);
                    int Anio = Feccha.Year;
                    int Mes = Feccha.Month;
                    Decimal.TryParse(DatoOT.SumaMes(Anio, Mes, mySession.idSucursal).ToString(), out Total);
                    Listado = DatoOT.PeriodoMes(Anio, Mes, mySession.idSucursal);
                }            

                if (Listado.Count == 0)
                {
                    OTPeriodoPieDTO Item = new OTPeriodoPieDTO();
                    Item.value = "";
                    Item.label = "";
                    Listado.Add(Item);
                }

                return Json(new { ok = true, respuesta = Listado, total = Total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult OTPeriodo3()
        {
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();

            try
            {
                List<OTPeriodoPieDTO> Listado = new List<OTPeriodoPieDTO>();
                
                decimal Total = 0;

                if (mySession.idRol == 1)
                {
                    int Anio = DateTime.Now.Year -2;
                    Decimal.TryParse(DatoOT.SumaPeriodo(Anio).ToString(), out Total);
                    Listado = DatoOT.ListaPeriodoPie(Anio);
                }
                else
                {
                    DateTime Feccha = DateTime.Now.AddMonths(-2);
                    int Anio = Feccha.Year;
                    int Mes = Feccha.Month;
                    Decimal.TryParse(DatoOT.SumaMes(Anio, Mes, mySession.idSucursal).ToString(), out Total);
                    Listado = DatoOT.PeriodoMes(Anio, Mes, mySession.idSucursal);
                }

                if (Listado.Count == 0)
                {
                    OTPeriodoPieDTO Item = new OTPeriodoPieDTO();
                    Item.value = "";
                    Item.label = "";
                    Listado.Add(Item);
                }

                return Json(new { ok = true, respuesta = Listado, total = Total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult KeepActiveSession()
        {
            JsonResponse<int> response = new JsonResponse<int>();

            if(mySession ==null)
            {
                response.ok = false;
            }

            return Json(response,JsonRequestBehavior.AllowGet);
        }


    }
}