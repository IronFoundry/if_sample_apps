using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoLib;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Publish(String userString)
        {
            Console.WriteLine("Publish starting...");
            CloudFoundryMongoBinder binder = new CloudFoundryMongoBinder();

            MongoQueue<string> workerQueue = new MongoQueue<string>(binder.Url, binder.DatabaseName, "worker_queue", 32000);
            Publisher publisher = new Publisher(workerQueue);
            publisher.Publish(userString);

            Console.WriteLine("... Publishing finished");

            return new EmptyResult();
        }
    }
}