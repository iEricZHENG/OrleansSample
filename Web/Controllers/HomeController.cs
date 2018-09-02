using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Demo.IGrain;
using Lib;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private IClientFactory clientFactory;
        public HomeController(IClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }
        public async Task<IActionResult> Index()
        {
            var client = clientFactory.GetClient();
            var actor = client.GetGrain<IHello>(0);
            var r = await actor.SayHello("Kiwi");
            return Content(r);
        }
    }
}
