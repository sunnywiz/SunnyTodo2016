using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework.Interfaces;

namespace SunnyTodo2016
{
    public class HierarchicalTaskEngine 
    {
        public List<HierarchicalTask> InputList { get; private set; }
        public List<HierarchicalTask> OutputList { get; private set; }
        public List<HierarchicalTask> FilledOutList { get; private set; }
        // later: InputHistory and OutputHistory

        public HierarchicalTaskEngine()
        {
            InputList = new List<HierarchicalTask>();
            OutputList = new List<HierarchicalTask>();
            FilledOutList = new List<HierarchicalTask>();
        }

        public void LoadFromFileContents(string[] contents)
        {
            InputList.Clear();
            foreach (var line in contents)
            {
                var hTask = new HierarchicalTask(line);
                InputList.Add(hTask);
            }

        }

        public void Process()
        {
            OutputList.Clear();

            OutputList.AddRange(
                InputList.Select(x=>new HierarchicalTask(x.ToString())));

            AssignOutputListIds();

            FilledOutList.Clear(); 

            FilledOutList.AddRange(
                OutputList.Select(x=>new HierarchicalTask(x.ToString())));

            AssignFilledOutListParents();

            AssignFilledOutEstimatesIfMissing(); 
        }

        private void AssignFilledOutEstimatesIfMissing()
        {
            foreach (var task in FilledOutList)
            {
                if (task.TodoTask != null)
                {
                    if (task.Estimate == null)
                    {
                        task.Estimate = 1.0; 
                    }
                }
            }
        }

        private void AssignOutputListIds()
        {
            int maxId = 0;
            foreach (var task in OutputList)
            {
                var val = task.Id;
                if (val.HasValue && val.Value > maxId) maxId = val.Value;
            }
            foreach (var task in OutputList)
            {
                var val = task.Id;
                if (!val.HasValue)
                {
                    task.Id = ++maxId;
                }
            }
        }

        public void AssignFilledOutListParents()
        {
            //    if no row found, then root -- parent = self. 
            //    if row found, then that's my parent. 

            // TODO: probably should verify that we don't create any loops. 

            // for all the things that don't have parents
            for (int i = 0; i < FilledOutList.Count; i++)
            {
                var task = FilledOutList[i];
                if (task.ParentId.HasValue) continue;

                for (var j = i - 1; j >= 0; j--)
                {
                    var ptask = FilledOutList[j];
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