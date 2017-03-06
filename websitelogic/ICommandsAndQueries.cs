using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using websitelogic.Entities;

namespace websitelogic
{
    // Break this up when we get a few
    public interface ICommandsAndQueries
    {
        LogicalBurndown GetBurndownById(Guid burndownId);
        void SaveBurndown(LogicalBurndown burndown);
    }
}
