using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SunnyTodo2016;
using SunnyTodo2016.Data;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // TODO: check to see if we have a previously-edited id
            // try to retrieve it
            // if we can, redirect there
            // else, redirect to a new one

            var guid = Guid.NewGuid();

            return RedirectToAction("Burndown", "Home", new {id = guid.ToString("n")});
        }

        public ActionResult Burndown(Guid id)
        {
            // see if id exists
            // if not, create a new one (doesn't get saved till we save changes)
            // if it does, retrieve it and process it and return that one. 

            var myuser = new MyUser()
            {
                MyUserID = MyUser.AnonymousUserId
            };

            var vm = new HomeBurndownViewModel()
            {
                User = myuser,
                Burndown = new Burndown()
                {
                    BurndownID = id,
                    Definition = @"# Welcome to Sunny's Burndown tracker

This is a root task.  
   This is a subtask
      This is a sub-sub-task

   Leaf tasks default estimate of 1
   Tasks that are parents of other tasks have a default estimate of 0.
   You can specify a leaf with a different estimate like this  est:3

   * Mark tasks that are in progress with a '* '   
   x Mark tasks as complete by starting them with 'x '

   If you have a large task that is in process, you can mark remaining like this  est:5 rem:3

   Click save changes to re-parse your task list and create a new snapshot in time. 
     - all tasks are given Id's 
     - A burndown is automatically created for you.
     - If you want a specific ID, you can specify it like this (string):  id:007

This is another root task.",
                    History = string.Empty,
                    OwnerUser = myuser
                }
            };
            return View("Burndown",vm);
        }

        public class HomeBurndownViewModel
        {
            public MyUser User { get; set; }
            public Burndown Burndown { get; set; }
        }

        public ActionResult SaveChanges(Guid? id, HomeBurndownViewModel model)
        {
            if (String.IsNullOrEmpty(model.Burndown.Definition) ||
                id == null ||
                id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            var logic = new HierarchicalTaskEngine();
            var lines = model.Burndown.Definition.Split(new char[] {'\n'});
            logic.LoadInputList(lines);

            // would load history from DB here
            logic.LoadInputHistory(new List<Tuple<DateTime, string>>());
            logic.Process();

            // would actually save here

            var myuser = new MyUser()
            {
                MyUserID = MyUser.AnonymousUserId
            };
            var vm = new HomeBurndownViewModel()
            {
                Burndown = new Burndown()
                {
                    BurndownID = id.Value,
                    Definition = string.Join(Environment.NewLine, logic.GetOutputLines()),
                    History = null,
                    OwnerUser = myuser,
                    OwnerUserID = myuser.MyUserID
                },
                User = myuser
            };
            ModelState.Clear();
            return View("Burndown", vm);
        }
    }
}