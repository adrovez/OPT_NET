using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace OPT.Web.Libreria
{
    public class MvsApiUri
    {
        private readonly static string WebApiCoreBase = ConfigurationManager.AppSettings["UriBaseApiDefontana"].Trim();

        #region ============== AUTH ==============

        public readonly static string Auth = WebApiCoreBase + "/api/Auth";
        public readonly static string EmailLogin = WebApiCoreBase + "/api/Auth/EmailLogin";

        #endregion



        #region ============== SALE ==============
        public readonly static string Getproducts = WebApiCoreBase + "/api/Sale/Getproducts";
        #endregion
    }
}