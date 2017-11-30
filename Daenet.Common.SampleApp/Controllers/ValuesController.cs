using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Daenet.Common.SampleApp.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private ILogger<ValuesController> m_Logger;
        private ValuesApi m_Api;

        public ValuesController(ILogger<ValuesController> logger, ValuesApi api)
        {
            m_Logger = logger;
            m_Api = api;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            m_Logger.LogInformation(100, "Entered {method}", nameof(Get));

            HttpContext.Session.SetString("abc", DateTime.Now.ToString());

            //
            // Test Scopes
            using (m_Logger.BeginScope("SCOPE 1.1"))
            {
                m_Logger.LogCritical(1000, "111");
                using (m_Logger.BeginScope("SCOPE 1.2"))
                {
                    m_Logger.LogCritical(1001, "222");

                    using (m_Logger.BeginScope("SCOPE 1.3"))
                    {
                        m_Logger.LogCritical(1002, "333");
                    }
                }
            }

            var ret = m_Api.Get();

            m_Logger.LogInformation(101, "Exit {method}", nameof(Get));

            return ret;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            m_Logger.LogInformation(102, "Entered {method}", nameof(Get));

            var ret = m_Api.Get(id);

            m_Logger.LogInformation(103, "Exit {method}", nameof(Get));

            return ret;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
            m_Logger.LogInformation(104, "Entered {method}", nameof(Post));

            m_Api.Post(value);

            m_Logger.LogInformation(105, "Exit {method}", nameof(Post));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            m_Logger.LogInformation(106, "Entered {method}", nameof(Put));

            m_Api.Put(id, value);

            m_Logger.LogInformation(107, "Exit {method}", nameof(Put));
        }


        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            m_Logger.LogInformation(108, "Entered {method}", nameof(Delete));

            m_Api.Delete(id);

            m_Logger.LogInformation(109, "Exit {method}", nameof(Delete));
        }
    }
}
