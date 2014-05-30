using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftballStatsViewer.Models;

namespace UnitTestProject1
{
    [TestClass]
    public class CloudFoundryConnectionStringBinderFixture
    {
        private string multipleServices =
    "{\"ms-sql\":[{\"name\":\"RealName\",\"label\":\"ms-sql\",\"tags\":[],\"plan\":\"free\",\"credentials\":{\"connection\":\"Data Source=10.91.166.29,1433;Initial Catalog=cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb;User ID=cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0;Password=]Q#wUN[{J%z+O7o6Q7zhmWWc\",\"user\":\"cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0\",\"password\":\"]Q#wUN[{J%z+O7o6Q7zhmWWc\",\"server\":\"10.91.166.29\",\"port\":\"1433\",\"database\":\"cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb\"}}],\"mongodb-2.2\":[{\"name\":\"free-mongo\",\"label\":\"mongodb-2.2\",\"tags\":[\"nosql\",\"document\"],\"plan\":\"free\",\"credentials\":{\"hostname\":\"10.91.166.21\",\"host\":\"10.91.166.21\",\"port\":10006,\"username\":\"6d751127-dae5-4ff8-8ae5-18356d86894a\",\"password\":\"86807b15-5c20-4c43-b01a-ca1092cfd664\",\"name\":\"4cb21486-7ac2-4ba9-9d1a-c34fdb22c835\",\"db\":\"db\",\"url\":\"mongodb://6d751127-dae5-4ff8-8ae5-18356d86894a:86807b15-5c20-4c43-b01a-ca1092cfd664@10.91.166.21:10006/db\"}}]}";

        public CloudFoundryConnectionStringBinderFixture()
        {
            Environment.SetEnvironmentVariable("VCAP_SERVICES", null);
        }

        [TestMethod]
        public void OriginalConnectionStringReturnedWhenVCapServicesIsNull()
        {
            string originalConnectionStringName = CloudFoundryConnectionStringBinder.Bind("ms-sql", "RealName");

            Assert.AreEqual("foo", originalConnectionStringName);
        }

        [TestMethod]
        public void ReturnsNullWhenNameNotFoundInConfigFile()
        {
            string returnedNull = CloudFoundryConnectionStringBinder.Bind("ms-sql", "FakeName");

            Assert.IsNull(returnedNull);
        }

        [TestMethod]
        public void ExceptionThrownWhenNoMatchingServiceTypeFound()
        {
            Environment.SetEnvironmentVariable("VCAP_SERVICES", multipleServices);
            try
            {
                CloudFoundryConnectionStringBinder.Bind("not-found-service-type", "RealName");
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("not-found-service-type"));
            }
        }

        [TestMethod]
        public void ExceptionThrownWhenConnectionStringNameNotFoundInCorrectServiceTypeSection()
        {
            Environment.SetEnvironmentVariable("VCAP_SERVICES", multipleServices);
            try
            {
                CloudFoundryConnectionStringBinder.Bind("ms-sql", "DifferentConnectionString");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("DifferentConnectionString"));
            }
        }

        [TestMethod]
        public void MatchingConnectionStringReturnedFromVcapServicesWhenConnectionStringNamesMatch()
        {
            Environment.SetEnvironmentVariable("VCAP_SERVICES", multipleServices);

            string cfConnectionString = CloudFoundryConnectionStringBinder.Bind("ms-sql", "RealName");
            Assert.AreEqual("Data Source=10.91.166.29,1433;Initial Catalog=cf-db-153449c9-e184-4952-aca1-ad3ab7b4d2cb;User ID=cf-user-d29728c1-ce6e-412f-b6eb-d901b08cb2e0;Password=]Q#wUN[{J%z+O7o6Q7zhmWWc", cfConnectionString);
        }
    }
}
