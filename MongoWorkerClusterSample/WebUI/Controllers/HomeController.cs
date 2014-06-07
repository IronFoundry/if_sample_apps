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
        public ActionResult Swap(String userString)
        {
            Console.WriteLine("Swap starting...");
            CloudFoundryMongoBinder binder = new CloudFoundryMongoBinder();
            Console.WriteLine("binding to " + binder.Url + ":" + binder.DatabaseName);

            MongoQueue<string> workerQueue = new MongoQueue<string>(binder.Url, binder.DatabaseName, "worker_queue", 32000);
            Publisher publisher = new Publisher(workerQueue);
            publisher.Publish(userString);

            Console.WriteLine("Waiting for next call...");

            return new EmptyResult();
        }

        public ActionResult CollectResults()
        {
            return new EmptyResult();
        }
    }
}