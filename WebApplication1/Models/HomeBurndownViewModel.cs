using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class HomeBurndownViewModel
    {
        public Guid BurndownId { get; set; }
        public string Definition { get; set; }
        public string Title { get; set; }
        public string AbsoluteUrl { get; set; }
        public string HistorySummary { get; set; }
    }
}