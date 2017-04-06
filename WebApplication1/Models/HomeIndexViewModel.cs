using System.Collections.Generic;
using SunnyTodo2016.Data;

namespace WebApplication1.Models
{
    public class HomeIndexViewModel
    {
        public List<Burndown> Burndowns { get; set; }
        public string NewBurndownUrl { get; set; }
    }
}