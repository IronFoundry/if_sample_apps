using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class Class1
    {
        private string singleService =
            "{\"ms-sql\":[{\"name\":\"free-sql\",\"label\":\"ms-sql\",\"tags\":[],\"plan\":\"free\",\"credentials\":{\"connection\":\"Data Source=10.91.166.29,1433;Initial Catalog=cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb;User ID=cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0;Password=]Q#wUN[{J%z+O7o6Q7zhmWWc\",\"user\":\"cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0\",\"password\":\"]Q#wUN[{J%z+O7o6Q7zhmWWc\",\"server\":\"10.91.166.29\",\"port\":\"1433\",\"database\":\"cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb\"}}]}";

        private string multipleServices =
            "{\"ms-sql\":[{\"name\":\"free-sql\",\"label\":\"ms-sql\",\"tags\":[],\"plan\":\"free\",\"credentials\":{\"connection\":\"Data Source=10.91.166.29,1433;Initial Catalog=cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb;User ID=cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0;Password=]Q#wUN[{J%z+O7o6Q7zhmWWc\",\"user\":\"cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0\",\"password\":\"]Q#wUN[{J%z+O7o6Q7zhmWWc\",\"server\":\"10.91.166.29\",\"port\":\"1433\",\"database\":\"cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb\"}}],\"mongodb-2.2\":[{\"name\":\"free-mongo\",\"label\":\"mongodb-2.2\",\"tags\":[\"nosql\",\"document\"],\"plan\":\"free\",\"credentials\":{\"hostname\":\"10.91.166.21\",\"host\":\"10.91.166.21\",\"port\":10006,\"username\":\"6d751127-dae5-4ff8-8ae5-18356d86894a\",\"password\":\"86807b15-5c20-4c43-b01a-ca1092cfd664\",\"name\":\"4cb21486-7ac2-4ba9-9d1a-c34fdb22c835\",\"db\":\"db\",\"url\":\"mongodb://6d751127-dae5-4ff8-8ae5-18356d86894a:86807b15-5c20-4c43-b01a-ca1092cfd664@10.91.166.21:10006/db\"}}]}";
       
        [TestMethod]
        public void FindsConnectionStringProperly()
        {
            JObject root = JObject.Parse(multipleServices);
//            var connectionString =
//                root["ms-sql"].Select(serviceInstance => serviceInstance["credentials"]["connection"].Value<string>());
            var serviceInstance =
                root["ms-sql"].First(si => si["name"].Value<string>() == "free-sql");
            var connectionString = serviceInstance["credentials"]["connection"].Value<string>();
            Console.WriteLine(connectionString);
        }

        [TestMethod]
        public void ReturnsNullWhenServiceTypeNotFound()
        {
            JObject root = JObject.Parse(multipleServices);
            var serviceType = root["DoesNotExist"];

            Assert.IsNull(serviceType);
        }
    }
}
