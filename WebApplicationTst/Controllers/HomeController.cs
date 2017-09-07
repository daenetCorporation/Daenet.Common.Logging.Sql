using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplicationTst.Controllers
{
    public class HomeController : Controller
    {
        ILogger m_Logger;

        public HomeController(ILogger<HomeController> logger)
        {
            m_Logger = logger;
        }

        public IActionResult Index()
        {
            using (m_Logger.BeginScope("SCOPE 1.1"))
            {
                m_Logger.LogCritical("111");
                using (m_Logger.BeginScope("SCOPE 1.2"))
                {
                    m_Logger.LogCritical("222");

                    using (m_Logger.BeginScope("SCOPE 1.3"))
                    {
                        m_Logger.LogCritical("333");
                    }

                }

            }

            return View();
        }

        public IActionResult About()
        {
            m_Logger.LogCritical("222");

            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
