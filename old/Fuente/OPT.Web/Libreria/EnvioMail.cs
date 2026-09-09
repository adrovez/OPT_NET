using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace OPT.Web.Libreria
{
    public class EnvioMail
    {
        private static string _MailFrom;
        private static string _MailError;
        private static string _MailSMTP;
        private static string _MailPuerto;
        private static bool _EnableSsl;
        private static bool _UseDefaultCredentials;
        private static string _Clave;

        public EnvioMail()
        {
            _MailFrom = ConfigurationManager.AppSettings["MailCuenta"].ToString();
            _MailError = ConfigurationManager.AppSettings["MailError"].ToString();

            _MailSMTP = ConfigurationManager.AppSettings["MailSMTP"].ToString();
            _MailPuerto = ConfigurationManager.AppSettings["MailPuerto"].ToString();
            _EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSsl"].ToString());
            _UseDefaultCredentials = bool.Parse(ConfigurationManager.AppSettings["UseDefaultCredentials"].ToString());
            _Clave = ConfigurationManager.AppSettings["MailClave"].ToString();
        }

        public void EnviarMail(string pAsunto, string pCuerpo, string pMailTo)
        {
            try
            {
                MailMessage email = new MailMessage();
                email.To.Add(new MailAddress(pMailTo));
                email.From = new MailAddress(_MailFrom);
                email.Subject = pAsunto;
                email.Body = pCuerpo;
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.UseDefaultCredentials = _UseDefaultCredentials;
                smtp.Host = _MailSMTP;
                smtp.Port = int.Parse(_MailPuerto);
                smtp.EnableSsl = _EnableSsl;

                smtp.Send(email);
                email.Dispose();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void MailError(Exception pExcep)
        {
            try
            {
                MailMessage email = new MailMessage();
                email.To.Add(new MailAddress(_MailError));
                email.From = new MailAddress(_MailFrom);
                email.Subject = "Error OPT";

                string escribir = "";
                escribir += "<table>";
                escribir += "<tr><td>============================ " + DateTime.Now.ToString() + " ============================</td></tr> ";
                escribir += "<tr><td>" + pExcep.Source.ToString() + "</td></tr>";
                escribir += "<tr><td>" + pExcep.TargetSite.ToString() + "</td></tr>";
                escribir += "<tr><td>" + pExcep.Message.ToString() + "</td></tr>";
                escribir += "<tr><td>" + pExcep.StackTrace.ToString() + "</td></tr>";
                escribir += "<tr><td>" + pExcep.InnerException == null ? "" : pExcep.InnerException.Message.ToString() + "</td></tr>";
                escribir += "</table>";


                email.Body = escribir;
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.UseDefaultCredentials = _UseDefaultCredentials;
                smtp.Host = _MailSMTP;
                smtp.Port = int.Parse(_MailPuerto);
                smtp.EnableSsl = _EnableSsl;

                smtp.Send(email);
                email.Dispose();
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}