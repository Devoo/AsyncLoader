using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AsyncLoaderDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Demo1 = AsyncLoader.SaveForAsync("Plan Data Saved");

            ViewBag.Demo2 = AsyncLoader.SaveForAsync(() =>
            {
                Thread.Sleep(2000);
                return "After 2 seconds you get this...";
            });

            ViewBag.Demo3 = AsyncLoader.SaveForAsync(args =>
            {
                Thread.Sleep(1000);
                return "After 1 seconds with args: " + args;
            });
            

            return View();
        }
    }
}