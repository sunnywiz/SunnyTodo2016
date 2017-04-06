using System;
using System.Collections.Generic;
using SunnyTodo2016.Data;

namespace WebApplication1.Models
{
    public class HomeBurndownViewModel
    {
        public Guid BurndownId { get; set; }
        public string Definition { get; set; }
        public string Title { get; set; }
        public string AbsoluteUrl { get; set; }
    }

    public class HomeIndexViewModel
    {
        public List<Burndown> Burndowns { get; set; }
        public string NewBurndownUrl { get; set; }
    }
}