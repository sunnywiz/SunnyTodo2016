using System;

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

            return new RedirectToBurndownViewModel() {BurndownID = Guid.NewGuid()};

        }

        public BaseViewModel BurndownByID()
        {
            return null; 
        }

        public BaseViewModel SaveBurndownChanges()
        {
            return null;
        }

        public BaseViewModel ListOfBurndowns()
        {
            return null; 
        }       
    }
}