using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace MongoLib
{
    public class CloudFoundryMongoBinder
    {
        public MongoDatabase Bind()
        {
            const string envName = "VCAP_SERVICES";
            var settings = Environment.GetEnvironmentVariable(envName);
            var jmongo = JObject.Parse(settings);
            var db = jmongo["mongodb-2.2"][0]["credentials"]["db"];
            var url = jmongo["mongodb-2.2"][0]["credentials"]["url"];
            var client = new MongoClient(url.ToString());
            var server = client.GetServer();
            MongoDatabase database = server.GetDatabase(db.ToString());

            return database;
        }
    }
}
