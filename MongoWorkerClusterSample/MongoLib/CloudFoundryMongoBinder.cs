using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace MongoLib
{
    public class CloudFoundryMongoBinder
    {
        public String DatabaseName { get; private set; }
        public String Url { get; private set; }
        public int AppPort { get; private set; }

        public CloudFoundryMongoBinder()
        {
            var settings = Environment.GetEnvironmentVariable("VCAP_SERVICES");
            Console.WriteLine("VCAP_SERVICES: " + settings);

            if (RunningLocally(settings))
            {
                Console.WriteLine("Running locally - retrieving from web.config");

                var connectionString = ConfigurationManager.ConnectionStrings["mongo"].ConnectionString;
                Url = connectionString;

                var connectionStringParts = connectionString.Split('/');
                DatabaseName = connectionStringParts[connectionStringParts.Length - 1];

                AppPort = Process.GetCurrentProcess().Id;
            }
            else
            {
                Console.WriteLine("Running on Cloud Foundry - retrieving from VCAP_SERVICES");

                var jmongo = JObject.Parse(settings);
                DatabaseName = jmongo["mongodb-2.2"][0]["credentials"]["db"].ToString();
                Url = jmongo["mongodb-2.2"][0]["credentials"]["url"].ToString();

                var port = Environment.GetEnvironmentVariable("VCAP_APP_PORT");
                AppPort = Convert.ToInt32(port);
            }

            Console.WriteLine("MONGO URL is " + Url);
            Console.WriteLine("DATABASE_NAME is " + DatabaseName);
            Console.WriteLine("APP_PORT is " + AppPort);
        }

        private static bool RunningLocally(string settings)
        {
            return String.IsNullOrEmpty(settings) || settings.Equals("{}");
        }
    }
}
