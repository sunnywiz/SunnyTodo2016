using System;
using System.Web.Mvc;
using websitelogic;
using websitelogic.ViewModels;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private WebsiteLogic _logic; 
        public HomeController()
        {
            _logic = new WebsiteLogic();
            // load it up with stuff... 
        }

        #region Actions

        public ActionResult Index()
        {
            return ActOnResult(_logic.HomePage());
        }

        public ActionResult Burndown(Guid id)
        {
            return ActOnResult(_logic.BurndownById(id));
        }


        public ActionResult SaveChanges(Guid? id, BurndownViewModel model)
        {
            if (id==null) return Index();
            model.BurndownId = id.Value; 
            return ActOnResult(_logic.SaveBurndownChanges(model));
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
                return View("Burndown", m2);
            }

            throw new NotSupportedException("Don't know how to interpret type " + result.GetType().FullName);
        }

        #endregion
    }
}