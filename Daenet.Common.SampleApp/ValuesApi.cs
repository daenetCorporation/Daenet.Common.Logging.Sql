using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daenet.Common.SampleApp
{
    public class ValuesApi
    {
        private ILogger<ValuesApi> m_Logger;

        public ValuesApi(ILogger<ValuesApi> logger)
        {
            m_Logger = logger;
        }

        public IEnumerable<string> Get()
        {
            using (m_Logger.BeginScope(nameof(ValuesApi)))
            {
                m_Logger.LogInformation(200, "Entered {method}", nameof(Get));

                m_Logger.LogInformation(201, "Exit {method}", nameof(Get));
                return new string[] { "value1", "value2" };
            }
        }

        public string Get(int id)
        {
            using (m_Logger.BeginScope(nameof(ValuesApi)))
            {
                m_Logger.LogInformation(202, "Entered {method}", nameof(Get));
                m_Logger.LogInformation(203, "Exit {method}", nameof(Get));
                return "value" + id; 
            }
        }

        public void Post(string value)
        {
            using (m_Logger.BeginScope(nameof(ValuesApi)))
            {
                m_Logger.LogInformation(204, "Entered {method}", nameof(Post));
                m_Logger.LogInformation(205, "Exit {method}", nameof(Post)); 
            }
        }

        public void Put(int id, string value)
        {
            using (m_Logger.BeginScope(nameof(ValuesApi)))
            {
                m_Logger.LogInformation(206, "Entered {method}", nameof(Put));
                m_Logger.LogInformation(207, "Exit {method}", nameof(Put)); 
            }
        }

        public void Delete(int id)
        {
            using (m_Logger.BeginScope(nameof(ValuesApi)))
            {
                m_Logger.LogInformation(208, "Entered {method}", nameof(Delete));
                m_Logger.LogInformation(209, "Exit {method}", nameof(Delete)); 
            }
        }
    }
}
