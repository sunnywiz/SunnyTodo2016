using System;
using System.Collections.Generic;

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
                var task = new todotxtlib.net.Task(line);
                result.Add(new HTask() { Line = line, TodoTask= task}); 
            }

            result.AssignIds();

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
            // for all the things that don't have parents
            // find a previous row that has an indent less than this one
            //    if no row found, then root -- parent = self. 
            //    if row found, then that's my parent. 
        }
    }
}