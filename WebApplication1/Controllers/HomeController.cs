using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SunnyTodo2016;
using SunnyTodo2016.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var context = new BurndownContext())
            {
                var vm = new HomeIndexViewModel()
                {
                    Burndowns = context.Burndowns.ToList(),
                    NewBurndownUrl = Url.Action("Burndown", "Home", new { id= (Guid?) Guid.NewGuid() }, Request?.Url?.Scheme ?? "http")
                };
                return View(vm);
            }
        }

        public ActionResult Burndown(Guid? id)
        {
            if (!id.HasValue) return Index(); 

            var vm = new HomeBurndownViewModel()
            {
                BurndownId = id.Value, 
                AbsoluteUrl = Url.Action("Burndown", "Home", new {id}, Request?.Url?.Scheme??"http")
            };
            using (var context = new BurndownContext())
            {
                var dbBurndown = context.Burndowns.FirstOrDefault(b => b.BurndownId == id);
                if (dbBurndown == null)
                {
                    vm.Title = "Sample Burndown";
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
                    vm.Title = dbBurndown.Title ?? "No title given";
                    vm.Definition = dbBurndown.Definition;
                    // also want to load history and other stuff here. 
                }
            }

            return View("Burndown",vm);
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


            // would actually save here
            using (var scope = new TransactionScope())
            using (var context = new BurndownContext())
            {
                // would load history from DB here
                var dbHistory = context.History.Where(h => h.BurndownId == id.Value).ToList();
                logic.LoadInputHistory(dbHistory.Select(h=>new Tuple<DateTime, string>(h.DateTime,h.TaskLine)));

                logic.Process();

                var dbBurndown = context.Burndowns.FirstOrDefault(b => b.BurndownId == id);
                if (dbBurndown == null)
                {
                    dbBurndown = new Burndown()
                    {
                        BurndownId = id.Value,
                        OwnerUserId = MyUser.AnonymousUserId
                    };
                    context.Burndowns.Add(dbBurndown);
                }
                dbBurndown.Title = model.Title;
                dbBurndown.Definition = String.Join(Environment.NewLine, logic.GetOutputLines());

                // save new histories, if they weren't already there.
                var historyToSave = logic.GetOutputHistory().ToList();
                foreach (var oldHistory in dbHistory)
                {
                    var isInNew =
                        historyToSave.FindIndex(h => h.Item1 == oldHistory.DateTime && h.Item2 == oldHistory.TaskLine);
                    if (isInNew >= 0)
                    {
                        // this one survives as is -- no change -- don't remove it. 
                        // don't save it either. 
                        historyToSave.RemoveAt(isInNew);
                    }
                    else
                    {
                        // not in the new scheme of things -- get rid of it. 
                        context.History.Remove(oldHistory);
                    }
                }
                foreach (var newHistory in historyToSave)
                {
                    context.History.Add(new HistoryLine()
                    {
                        BurndownId = id.Value,
                        HistoryLineId = Guid.NewGuid(),
                        DateTime = newHistory.Item1,
                        TaskLine = newHistory.Item2
                    });
                }

                context.Database.Log = x => Trace.WriteLine(x);
                context.SaveChanges();
                scope.Complete();
            }

            return RedirectToAction("Burndown", "Home", new {id});
        }
    }
}