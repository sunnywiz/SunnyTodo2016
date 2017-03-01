using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SunnyTodo2016.Data;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var guid = Guid.NewGuid();

            var myuser = new MyUser()
            {
                MyUserID = MyUser.AnonymousUserId
            };

            var vm = new HomeIndexViewModel()
            {
                User = myuser,
                Burndown = new Burndown()
                {
                    BurndownID = Guid.NewGuid(),
                    Definition = @"# Welcome to Sunny's Burndown tracker",
                    History = string.Empty,
                    OwnerUser = myuser
                }
            };
            return View(vm);
        }

        public class HomeIndexViewModel 
        {
            public MyUser User { get; set; }
            public Burndown Burndown { get; set; }
        }
    }
}