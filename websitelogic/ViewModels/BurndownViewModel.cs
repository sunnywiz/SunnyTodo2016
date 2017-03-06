using System;
using System.Collections.Generic;

namespace websitelogic.ViewModels
{
    public class BurndownViewModel : BaseViewModel
    {
        public Guid BurndownId { get; set; }
        public List<string> Definition { get; set; }

    }
}