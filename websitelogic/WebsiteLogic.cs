using System;
using System.Collections.Generic;
using System.Linq;
using SunnyTodo2016;
using websitelogic.ViewModels;

namespace websitelogic
{
    /// <summary>
    /// This is the web logic, distilled AWAY from MVC, so that it could be expressed via Lambda or something.
    /// </summary>
    public class WebsiteLogic
    {

        public WebsiteLogic()
        {
        }

        public BaseViewModel HomePage()
        {
            // when visiting home page: 
            //   should see list of burndowns + new button 
            //   if not logged in, this should be any publicly visible burndowns that you have visited (via cookie)
            //   if logged in, should include any burndowns that you own
            //   later might include burndowns you are invited to
            // if no burndowns, go immediately to a new burndown

            return new RedirectToBurndownViewModel() { BurndownId = Guid.NewGuid() };

        }

        public BaseViewModel BurndownById(Guid burndownId)
        {
            // see if id exists
            // if not, create a new one (doesn't get saved till we save changes)
            // if it does, retrieve it and process it and return that one. 

            var vm = new BurndownViewModel()
            {
                BurndownId = burndownId,
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

This is another root task."
.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
.ToList(),
            };
            return vm;
        }

        public BaseViewModel SaveBurndownChanges(BurndownViewModel model)
        {
            // dont forget to check if this user can access this burndown or not

            var logic = new HierarchicalTaskEngine();

            // this splitting and joining needs to move back out to the MVC layer, I think. 


            logic.LoadInputList(model.Definition.ToArray());

            // would load history from DB here
            logic.LoadInputHistory(new List<Tuple<DateTime, string>>());
            logic.Process();

            // would actually save here

            // and this would become a RedirectToBurndownViewModel instead.
            var vm = new BurndownViewModel()
            {
                BurndownId = model.BurndownId,
                Definition = logic.GetOutputLines().ToList(),
            };
            return vm;
        }

        public BaseViewModel ListOfBurndowns()
        {
            return null;
        }
    }
}