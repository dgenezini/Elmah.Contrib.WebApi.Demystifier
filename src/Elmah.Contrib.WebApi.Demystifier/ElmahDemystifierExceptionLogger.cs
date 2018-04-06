// Based on https://github.com/rdingwall/elmah-contrib-webapi/blob/master/src/Elmah.Contrib.WebApi/ElmahExceptionLogger.cs

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace Elmah.Contrib.WebApi.Demystifier
{
    public class ElmahDemystifierExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            // Retrieve the current HttpContext instance for this request.
            HttpContext httpContext = GetHttpContext(context.Request);

            if (httpContext == null)
                return;

            Exception ex = context.Exception.Demystify();

            // Wrap the exception in an HttpUnhandledException so that ELMAH can capture the original error page.
            Exception exceptionToRaise = new HttpUnhandledException(ex.Message, ex);

            // Send the exception to ELMAH (for logging, mailing, filtering, etc.).
            ErrorSignal signal = ErrorSignal.FromContext(httpContext);
            signal.Raise(exceptionToRaise, httpContext);
        }

        private static HttpContext GetHttpContext(HttpRequestMessage request)
        {
            if (request == null)
                return null;

            object value;
            if (request.Properties.TryGetValue("MS_HttpContext", out value))
            {
                HttpContextBase context = value as HttpContextBase;
                if (context != null)
                    return context.ApplicationInstance.Context;
            }

            return HttpContext.Current;
        }
    }
}