using System;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SoftballStatsViewer.Models
{
    public static class CloudFoundryConnectionStringBinder
    {
        public static string Bind(string serviceType, string connectionStringName)
        {
            Console.WriteLine("Beginning bind for: " + connectionStringName);

            string vcapServices = Environment.GetEnvironmentVariable("VCAP_SERVICES");

            Console.WriteLine("Detected VCAP_SERVICES value of: " + vcapServices);

            if (String.IsNullOrEmpty(vcapServices))
            {
                Console.WriteLine("VCAP_SERVICES not set - returning original connection string name");
                return connectionStringName;
            }

            if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
            {
                Console.WriteLine("No match found between service instance name and connection string name");
                return connectionStringName;
            }

            JObject root = JObject.Parse(vcapServices);
            var cfServiceRoot = root[serviceType];
            Console.WriteLine("Retrieved service information for " + serviceType);

            var cfServiceInstance =
                cfServiceRoot.FirstOrDefault(si => Extensions.Value<string>(si["name"]) == connectionStringName);
            Console.WriteLine("Retrieved service instance information for " + connectionStringName);

            var connectionString = cfServiceInstance["credentials"]["connection"].Value<string>();
            Console.WriteLine("Detected connection string value of " + connectionString);
            return connectionString;
        }
    }
}