using System;
using System.Collections.Generic;
using System.Configuration;
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
        public int? AppPort { get; private set; }

        public CloudFoundryMongoBinder()
        {
            const string envName = "VCAP_SERVICES";
            var settings = Environment.GetEnvironmentVariable(envName);
            Console.WriteLine("VCAP_SERVICES: " + settings);

            if (String.IsNullOrEmpty(settings) || settings.Equals("{}"))
            {
                var connectionString = ConfigurationManager.ConnectionStrings["mongo"].ConnectionString;
                Url = connectionString;

                var connectionStringParts = connectionString.Split('/');
                DatabaseName = connectionStringParts[connectionStringParts.Length - 1];

                return;
            }

            var jmongo = JObject.Parse(settings);
            DatabaseName = jmongo["mongodb-2.2"][0]["credentials"]["db"].ToString();
            Url = jmongo["mongodb-2.2"][0]["credentials"]["url"].ToString();

            Console.WriteLine("Binding");
            var port = Environment.GetEnvironmentVariable("VCAP_APP_PORT");
            Console.WriteLine(String.IsNullOrEmpty(port) ? "Port is not set!" : port);
            if (string.IsNullOrEmpty(port)) AppPort = null;
            else AppPort = Convert.ToInt32(port);

            Console.WriteLine("APP_PORT is " + AppPort);

//            var client = new MongoClient(url.ToString());
//            var server = client.GetServer();
//            MongoDatabase database = server.GetDatabase(db.ToString());

//            return database;
        }
    }
}
