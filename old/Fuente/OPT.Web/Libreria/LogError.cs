using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace OPT.Web.Libreria
{
    public static class LogError
    {
        public static void setLogExeption(Exception pExcep)
        {
            try
            {
                StreamWriter escribir = null;
                string Archivo = DateTime.Now.ToString("ddMMyyyy");
                string dir = HttpContext.Current.Server.MapPath("~/LogError/"); //Directory.GetCurrentDirectory();

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string path = Path.Combine(dir, Archivo + ".log");
                escribir = new StreamWriter(path, true);
                escribir.WriteLine("============================ " + DateTime.Now.ToString() + " ============================");
                escribir.WriteLine(pExcep.Source.ToString());
                escribir.WriteLine(pExcep.TargetSite.ToString());
                escribir.WriteLine(pExcep.Message.ToString());
                escribir.WriteLine(pExcep.StackTrace.ToString());
                escribir.WriteLine(pExcep.InnerException == null ? "" : pExcep.InnerException.ToString());
                escribir.Close();

                Libreria.EnvioMail EnviarMail = new EnvioMail();
                //EnviarMail.MailError(pExcep);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static void setLogExeption(Exception pExcep, string funcion)
        {
            try
            {
                StreamWriter escribir = null;
                string Archivo = DateTime.Now.ToString("ddMMyyyy");
                string dir = HttpContext.Current.Server.MapPath("~/LogError/"); //Directory.GetCurrentDirectory();

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string path = Path.Combine(dir, Archivo + ".log");
                escribir = new StreamWriter(path, true);
                escribir.WriteLine("============================ " + DateTime.Now.ToString() + " ============================");
                escribir.WriteLine(funcion);
                escribir.WriteLine(pExcep.Source.ToString());
                escribir.WriteLine(pExcep.TargetSite.ToString());
                escribir.WriteLine(pExcep.Message.ToString());
                escribir.WriteLine(pExcep.StackTrace.ToString());
                escribir.WriteLine(pExcep.InnerException == null ? "" : pExcep.InnerException.ToString());
                escribir.Close();

                Libreria.EnvioMail EnviarMail = new EnvioMail();
                //EnviarMail.MailError(pExcep);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}