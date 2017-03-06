using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    /// <summary>
    /// This only exists because \r\n was a detail that i wasn't 
    /// sure belonged in the logical layer. 
    /// </summary>
    public class HtmlSpecificBurndownViewModel
    {
        public string Definition { get; set; }
        public Guid BurndownId { get; set; }
    }
}