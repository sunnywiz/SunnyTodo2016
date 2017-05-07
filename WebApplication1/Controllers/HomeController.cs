using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using SunnyTodo2016;
using SunnyTodo2016.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private const string SampleBurndownDefinition = @"# Welcome to Sunny's Burndown tracker

This is a root task.  <
    This is a subtask  >
       This is a sub-sub-task ""

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

        private Guid UserId
        {
            get
            {
                var userIdAsString = User.Identity.GetUserId();
                Guid g;
                if (string.IsNullOrEmpty(userIdAsString))
                {
                    return MyUser.AnonymousUserId;
                }
                else
                {
                    if (!Guid.TryParse(userIdAsString, out g))
                    {
                        return MyUser.AnonymousUserId;
                    }
                    return g;
                }

            }
        }

        public ActionResult Index()
        {
            using (var context = new BurndownContext())
            {
                List<Burndown> burndowns;
                var queryable = context.Burndowns
                    .Where(b => b.OwnerUserId == UserId);
                if (UserId == MyUser.AnonymousUserId)
                {
                    // Future -- save this as a persistent cookie, don't ask db.
                    queryable = queryable.OrderByDescending(b => b.LastModifiedDate).Take(10);
                }
                burndowns =
                        queryable
                        .OrderBy(b => b.LastModifiedDate)
                        .ToList();
                var vm = new HomeIndexViewModel()
                {
                    Burndowns = burndowns,
                    NewBurndownUrl = Url.Action("Burndown", "Home", new { id = (Guid?)Guid.NewGuid() }, Request?.Url?.Scheme ?? "http")
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
                AbsoluteUrl = Url.Action("Burndown", "Home", new { id }, Request?.Url?.Scheme ?? "http")
            };
            using (var context = new BurndownContext())
            {
                var dbBurndown = context.Burndowns.FirstOrDefault(b => b.BurndownId == id);
                if (dbBurndown == null)
                {
                    vm.Title = "Sample Burndown";
                    vm.Definition = SampleBurndownDefinition;
                    vm.CanUserEditBurndown = true;
                    vm.CanUserEditBurndownAccess = (UserId != MyUser.AnonymousUserId);
                    vm.IsPublicEditable = (UserId == MyUser.AnonymousUserId);
                    vm.IsPublicViewable = (UserId == MyUser.AnonymousUserId);
                }
                else
                {
                    var accessCheck = new AccessChecks(dbBurndown, UserId);
                    if (!accessCheck.CanView)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var logic = new HierarchicalTaskEngine();

                    vm.Title = dbBurndown.Title ?? "No title given";
                    vm.Definition = dbBurndown.Definition;
                    vm.IsPublicEditable = dbBurndown.IsPublicEditable;
                    vm.IsPublicViewable = dbBurndown.IsPublicViewable;

                    var accessChecks = new AccessChecks(dbBurndown, UserId);

                    vm.CanUserEditBurndown = accessChecks.CanEdit;
                    vm.CanUserEditBurndownAccess = accessChecks.CanEditAccessibility;

                    // also want to load history and other stuff here. 

                    var dbHistory = context.History.Where(h => h.BurndownId == id.Value).ToList();
                    logic.LoadInputHistory(dbHistory.Select(h => new Tuple<DateTime, string>(h.DateTime, h.TaskLine)));

                    var parentIds = logic.InputHistory.Where(x => x.ParentId == null).Select(x => x.Id).Distinct().OrderBy(x => x).ToList();

                    StringBuilder report = new StringBuilder();
                    foreach (var parentId in parentIds)
                    {
                        report.AppendLine();
                        foreach (var ht in logic.InputHistory.Where(x => x.Id == parentId).OrderBy(x => x.TimeStamp))
                        {
                            report.AppendFormat("{0}: {1} {2}", ht.TimeStamp.ToShortDateString(), ht.TotalEstimate, ht.TotalRemaining);
                            report.AppendLine();
                        }
                    }
                    vm.HistorySummary = report.ToString();

                }
            }

            return View("Burndown", vm);
        }

        public ActionResult SaveChanges(HomeBurndownViewModel model)
        {
            // dont forget to check if this user can access this burndown or not

            if (model == null ||
                model.BurndownId == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            var logic = new HierarchicalTaskEngine();
            //  http://stackoverflow.com/questions/14217101/what-character-represents-a-new-line-in-a-text-area  says its \r\n
            var lines = model.Definition.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            logic.LoadInputList(lines);


            // would actually save here
            using (var scope = new TransactionScope())
            using (var context = new BurndownContext())
            {
                // would load history from DB here
                var dbHistory = context.History.Where(h => h.BurndownId == model.BurndownId).ToList();
                logic.LoadInputHistory(dbHistory.Select(h => new Tuple<DateTime, string>(h.DateTime, h.TaskLine)));

                logic.Process();

                var dbBurndown = context.Burndowns.FirstOrDefault(b => b.BurndownId == model.BurndownId);
                if (dbBurndown == null)
                {
                    dbBurndown = new Burndown()
                    {
                        BurndownId = model.BurndownId,
                        OwnerUserId = UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsPublicEditable = false,
                        IsPublicViewable = false
                    };
                    context.Burndowns.Add(dbBurndown);
                }

                var accessCheck = new AccessChecks(dbBurndown, UserId);
                if (!accessCheck.CanEdit)
                {
                    return RedirectToAction("Index", "Home");
                }

                dbBurndown.Title = model.Title;
                dbBurndown.Definition = String.Join(Environment.NewLine, logic.GetOutputLines());
                dbBurndown.LastModifiedDate = DateTime.UtcNow;

                if (accessCheck.IfAnonymouslyOwnedMustBePubliclyEditable)
                {
                    dbBurndown.IsPublicEditable = true;
                    dbBurndown.IsPublicViewable = true;
                }
                else if (accessCheck.CanEditAccessibility)
                {
                    // clean this up later
                    if (model.IsPublicEditable.HasValue)
                        dbBurndown.IsPublicEditable = model.IsPublicEditable.Value;

                    if (model.IsPublicViewable.HasValue)
                        dbBurndown.IsPublicViewable = model.IsPublicViewable.Value;
                }

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
                        BurndownId = model.BurndownId,
                        HistoryLineId = Guid.NewGuid(),
                        DateTime = newHistory.Item1,
                        TaskLine = newHistory.Item2
                    });
                }

                context.Database.Log = x => Trace.WriteLine(x);
                context.SaveChanges();
                scope.Complete();
            }

            return RedirectToAction("Burndown", "Home", new { id = model.BurndownId });
        }

        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            List<string> historyLines = null;
            List<string> todoLines = null;
            var fileName = "uploaded file";
            foreach (var file in files)
            {
                List<string> lines = new List<string>();
                using (var sr = new StreamReader(file.InputStream))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
                if (file.FileName.EndsWith(".history.txt"))
                {
                    historyLines = lines;
                }
                else
                {
                    todoLines = lines;
                    fileName = file.FileName;
                }
            }
            var burndownId = Guid.NewGuid();

            var historyTuples =
                historyLines.Select(HistoryFileHelper.PipeSeperatedLineToHistoryTuple).Where(t => t != null).ToList();

            var dbHistoryLines = historyTuples.Select(t => new HistoryLine()
            {
                DateTime = t.Item1,
                BurndownId = burndownId,
                HistoryLineId = Guid.NewGuid(),
                TaskLine = t.Item2
            }).ToList();

            using (var context = new BurndownContext())
            {
                var dbBurndown = new Burndown()
                {
                    BurndownId = burndownId,
                    CreatedDate = DateTime.UtcNow,
                    Definition = String.Join(Environment.NewLine, todoLines),
                    History = dbHistoryLines,
                    LastModifiedDate = DateTime.UtcNow,
                    OwnerUserId = UserId,
                    Title = fileName,
                };
                context.Burndowns.Add(dbBurndown);

                var accessCheck = new AccessChecks(dbBurndown, UserId);
                if (accessCheck.IfAnonymouslyOwnedMustBePubliclyEditable)
                {
                    dbBurndown.IsPublicEditable = true;
                    dbBurndown.IsPublicViewable = true;
                }
                else
                {
                    dbBurndown.IsPublicEditable = false;
                    dbBurndown.IsPublicViewable = false;
                }

                context.SaveChanges();
            }
            return RedirectToAction("Burndown", "Home", new { id = burndownId });
        }
    }
}