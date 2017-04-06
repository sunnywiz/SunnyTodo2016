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
            var vm = new HomeBurndownViewModel() {BurndownId = id};
            using (var context = new BurndownContext())
            {
                var dbBurndown = context.Burndowns.FirstOrDefault(b => b.BurndownID == id);
                if (dbBurndown == null)
                {
                    vm.Definition = @"# Welcome to Sunny's Burndown tracker

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

This is another root task.";
                }
                else
                {
                    vm.Definition = dbBurndown.Definition;
                    // also want to load history and other stuff here. 
                }
            }

            return View("Burndown",vm);
        }

        public class HomeBurndownViewModel
        {
            public Guid BurndownId { get; set; }
            public string Definition { get; set; }
        }

        public ActionResult SaveChanges(Guid? id, HomeBurndownViewModel model)
        {
            // dont forget to check if this user can access this burndown or not

            if (String.IsNullOrEmpty(model.Definition) ||
                id == null ||
                id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            var logic = new HierarchicalTaskEngine();
            //  http://stackoverflow.com/questions/14217101/what-character-represents-a-new-line-in-a-text-area  says its \r\n
            var lines = model.Definition.Split(new string[] {"\r\n"}, StringSplitOptions.None);

            logic.LoadInputList(lines);

            // would load history from DB here
            logic.LoadInputHistory(new List<Tuple<DateTime, string>>());
            logic.Process();

            // would actually save here
            using (var context = new BurndownContext())
            {
                var dbBurndown = context.Burndowns.FirstOrDefault(b => b.BurndownID == id);
                if (dbBurndown == null)
                {
                    dbBurndown = new Burndown()
                    {
                        BurndownID = id.Value,
                        OwnerUserID = MyUser.AnonymousUserId
                    };
                    context.Burndowns.Add(dbBurndown);
                }
                dbBurndown.Definition = String.Join(Environment.NewLine, logic.GetOutputLines());
                context.SaveChanges();
            }

            return RedirectToAction("Burndown", "Home", new {id});
        }
    }
}