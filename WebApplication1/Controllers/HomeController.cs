using System;
using System.Linq;
using System.Web.Mvc;
using websitelogic;
using websitelogic.ViewModels;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private WebsiteLogic _logic; 
        public HomeController()
        {
            _logic = new WebsiteLogic(new BurndownDbCommandsAndQueries());
            // load it up with stuff... 
        }

        #region Actions

        public ActionResult Index()
        {
            return ActOnResult(_logic.HomePage());
        }

        public ActionResult Burndown(Guid? id)
        {
            if (id == null) return RedirectToAction("Index","Home");

            return ActOnResult(_logic.BurndownById(id.Value));
        }


        public ActionResult SaveChanges(Guid? id, HtmlSpecificBurndownViewModel model)
        {
            //  http://stackoverflow.com/questions/14217101/what-character-represents-a-new-line-in-a-text-area  says its \r\n
            if (id==null) return Index();

            model.BurndownId = id.Value; 

            var lines = model.Definition.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            var lvm = new BurndownViewModel()
            {
                BurndownId = id.Value,
                Definition = model.Definition.Split(new string[] {"\r\n"}, StringSplitOptions.None).ToList()
            };

            return ActOnResult(_logic.SaveBurndownChanges(lvm));
        }

        #endregion

        #region Results

        private ActionResult ActOnResult(BaseViewModel result)
        {
            var m1 = result as RedirectToBurndownViewModel;
            if (m1 != null)
                return RedirectToAction("Burndown", "Home", new {id = m1.BurndownId});

            var m2 = result as BurndownViewModel;
            if (m2 != null)
            {
                ModelState.Clear();
                return View("Burndown", new HtmlSpecificBurndownViewModel()
                {
                    BurndownId = m2.BurndownId,
                    Definition = String.Join("\r\n",m2.Definition)
                });
            }

            throw new NotSupportedException("Don't know how to interpret type " + result.GetType().FullName);
        }

        #endregion
    }
}