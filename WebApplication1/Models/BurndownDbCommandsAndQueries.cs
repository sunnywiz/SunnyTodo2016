using System;
using System.Linq;
using websitelogic;
using websitelogic.Entities;

namespace WebApplication1.Models
{
    public class BurndownDbCommandsAndQueries : ICommandsAndQueries
    {
        public LogicalBurndown GetBurndownById(Guid burndownId)
        {
            using (var context = new BurndownDbContext())
            {
                var bd = context.Burndowns.FirstOrDefault(x => x.Id == burndownId);
                if (bd == null) return null;
                return new LogicalBurndown()
                {
                    BurndownID = bd.Id,
                    Definition = bd.Definition.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList(),
                    History = null,
                    OwnerUserId = Guid.Empty
                };
            }
        }

        public void SaveBurndown(LogicalBurndown burndown)
        {
            using (var context = new BurndownDbContext())
            {
                var bd = context.Burndowns.FirstOrDefault(x => x.Id == burndown.BurndownID);
                if (bd == null)
                {
                    bd = new BurndownDbContext.DbBurndown() { Id = burndown.BurndownID };
                    context.Burndowns.Add(bd);
                }
                bd.Definition = String.Join("\r\n", burndown.Definition);
                context.SaveChanges();
            }
        }
    }
}