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

            if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
            {
                Console.WriteLine("No connection string found matching given connection string name, returning null...");
                return null;
            }

            string vcapServices = Environment.GetEnvironmentVariable("VCAP_SERVICES");
            Console.WriteLine("Detected VCAP_SERVICES value of: " + vcapServices);

            if (String.IsNullOrEmpty(vcapServices))
            {
                Console.WriteLine("VCAP_SERVICES not set - returning connection string from app config...");
                return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            }

            JObject root = JObject.Parse(vcapServices);
            var cfServiceRoot = root[serviceType];
            if (cfServiceRoot == null)
            {
                throw new ArgumentException("The specified service type could not be found in VCAP_SERVICES.",
                    serviceType);
            }
            Console.WriteLine("Retrieved service information for " + serviceType);

            var cfServiceInstance =
                cfServiceRoot.FirstOrDefault(si => Extensions.Value<string>(si["name"]) == connectionStringName);
            if (cfServiceInstance == null)
            {
                throw new ArgumentException(
                    "The specified service name could not be found in VCAP_SERVICES. Was the service broker with the correct name created?",
                    connectionStringName);
            }
            Console.WriteLine("Retrieved service instance information for " + connectionStringName);
                
            var connectionString = cfServiceInstance["credentials"]["connection"].Value<string>();
            Console.WriteLine("Replacing connection string information for " + connectionStringName + " with VCAP_SERVICES value of " + connectionString);

            return connectionString;
        }
    }
}