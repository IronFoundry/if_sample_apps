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
        public string Swap(String userString)
        {
            MongoQueue<string> workerQueue = new MongoQueue<string>("mongodb://127.0.0.1/queue_test", "queue_test", "worker_queue", 32000);
            Publisher publisher = new Publisher(workerQueue);
            publisher.Publish(userString);

            return "This is the result";
        }
    }
}