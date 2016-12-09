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
                var task = new todotxtlib.net.Task(line);
                result.Add(new HTask() { Line = line, TodoTask= task}); 
            }
            return result; 
        }
    }
}