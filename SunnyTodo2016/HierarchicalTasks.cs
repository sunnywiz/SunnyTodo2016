using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework.Interfaces;

namespace SunnyTodo2016
{
    public class HierarchicalTasks : List<HTask>
    {
        public static HierarchicalTasks LoadFromFileContents(string[] contents)
        {
            var result = new HierarchicalTasks();
            foreach (var line in contents)
            {
                if (String.IsNullOrWhiteSpace(line)) continue;
                var hTask = new HTask(line);
                result.Add(hTask);
            }

            result.AssignIds();
            result.AssignParents();

            return result;
        }


        private void AssignIds()
        {
            int maxId = 0;
            foreach (var task in this)
            {
                var val = task.Id;
                if (val.HasValue && val.Value > maxId) maxId = val.Value;
            }
            foreach (var task in this)
            {
                var val = task.Id;
                if (!val.HasValue)
                {
                    task.Id = ++maxId;
                }
            }
        }

        private void AssignParents()
        {
            //    if no row found, then root -- parent = self. 
            //    if row found, then that's my parent. 

            // TODO: probably should verify that we don't create any loops. 

            // for all the things that don't have parents
            for (int i = 0; i < this.Count; i++)
            {
                var task = this[i];
                if (task.ParentId.HasValue) continue;

                for (var j = i - 1; j >= 0; j--)
                {
                    var ptask = this[j];
                    if (!ptask.Id.HasValue) continue;
                    if (ptask.IndentLevel >= task.IndentLevel) continue;

                    task.ParentId = ptask.Id;
                    goto outer;
                }
                // if we get here, didn't find anything that would work
                // so we'll assign ourselves
                if (!task.Id.HasValue) continue;
                task.ParentId = task.Id;

                outer:
                ;
            }
        }
    }
}