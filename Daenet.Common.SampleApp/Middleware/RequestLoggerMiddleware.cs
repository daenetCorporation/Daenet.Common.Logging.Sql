using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daenet.Common.SampleApp.Middleware
{
    /// <summary>
    /// Begin Scope for given ActivityID or creates new ActivityID
    /// </summary>
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate m_Next;
        private readonly ILogger m_Logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="loggerFactory"></param>
        public RequestLoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            m_Next = next;
            m_Logger = loggerFactory.CreateLogger<RequestLoggerMiddleware>();
        }

        /// <summary>
        /// Adds ActivityID to logging scope.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            string acID = "";
            if (context.Request.Headers.ContainsKey("ActivityID"))
                acID = context.Request.Headers["ActivityID"];
            else
                acID = Guid.NewGuid().ToString();

            using (m_Logger.BeginScope(new Dictionary<string, object>()
                {
                { "ActivityID", acID }
                    }))
            {
                await m_Next.Invoke(context);
            }
        }
    }
}
