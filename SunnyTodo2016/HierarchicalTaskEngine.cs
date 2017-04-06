using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;

namespace SunnyTodo2016
{
    public class HierarchicalTaskEngine
    {
        public List<HierarchicalTask> InputList { get; }
        public List<HierarchicalTask> OutputList { get; }
        public List<HierarchicalTask> FilledOutList { get; }
        public List<HierarchicalTask> InputHistory { get; }
        public List<HierarchicalTask> OutputHistory { get; private set; }
        public List<HierarchicalTask> InterpolatedHistory { get; private set; }

        public HierarchicalTaskEngine()
        {
            InputList = new List<HierarchicalTask>();
            OutputList = new List<HierarchicalTask>();
            FilledOutList = new List<HierarchicalTask>();
            InputHistory = new List<HierarchicalTask>();
            OutputHistory = new List<HierarchicalTask>();
            InterpolatedHistory = new List<HierarchicalTask>(); 
        }

        public void LoadInputList(string[] contents)
        {
            InputList.Clear();
            foreach (var line in contents)
            {
                var hTask = new HierarchicalTask(line);
                InputList.Add(hTask);
            }
        }

        public void LoadInputHistory(IEnumerable<Tuple<DateTime, string>> contents)
        {
            InputHistory.Clear();
            foreach (var input in contents)
            {
                var hTask = new HierarchicalTask(input.Item2)
                {
                    TimeStamp = input.Item1
                };
                InputHistory.Add(hTask);
            }
        }

        public void Process()
        {
            OutputList.Clear();

            OutputList.AddRange(
                InputList.Select(x => new HierarchicalTask(x.ToString())));

            AssignOutputListIds();

            OutputScanAndDateCompletedThings();  

            FilledOutList.Clear();

            FilledOutList.AddRange(
                OutputList.Select(x => new HierarchicalTask(x.ToString())));

            AssignFilledOutListParents();

            foreach (var task in FilledOutList)
            {
                if (!task.WasParsed) continue;
                if (task.Children.Count > 0)
                    RecursiveAddToDeepChildren(task, task.Children);
            }

            AssignFilledOutEstimatesIfMissing();

            AssignFilledOutTotalEstimates();

            MergeFilledOutListAndInputHistoryToOutputHistory();

            InterpolateHistory(); 
        }

        public void InterpolateHistory()
        {
            InterpolatedHistory.Clear();

            var distinctTimes = OutputHistory.Select(x => x.TimeStamp).Distinct().OrderBy(x => x).ToList();
            var distinctIds = OutputHistory.Select(x => x.Id).Distinct().OrderBy(x => x).ToList();

            foreach (var id in distinctIds)
            {
                var recs = OutputHistory.Where(x => x.Id == id).OrderBy(x => x.TimeStamp).ToList();
                foreach (var time in distinctTimes)
                {
                    var before =
                        recs.Where(t => t.TimeStamp <= time).OrderBy(x => x.TimeStamp).LastOrDefault();

                    var after =
                        recs.Where(t => t.TimeStamp > time).OrderBy(x => x.TimeStamp).FirstOrDefault();

                    if (before == null) continue;

                    if (after == null && before.TimeStamp < time) continue;

                    var outrec = new HierarchicalTask(before.ToString())
                    {
                        TimeStamp = time
                    };
                    InterpolatedHistory.Add(outrec);
                }
            }
        }

        private void MergeFilledOutListAndInputHistoryToOutputHistory()
        {
            OutputHistory.Clear();
            OutputHistory.AddRange(InputHistory);
            DateTime timestamp = DateTime.Now;
            foreach (var task in FilledOutList.Where(t => t.WasParsed))
            {
                var existingHistory = OutputHistory
                    .Where(t => t.Id == task.Id)
                    .OrderByDescending(t => t.TimeStamp)
                    .Take(2)
                    .ToList();

                if (existingHistory.Count == 2 &&
                    timestamp > existingHistory[0].TimeStamp &&
                    existingHistory[0].TimeStamp > existingHistory[1].TimeStamp &&
                    task.TodoTask.ToString() == existingHistory[0].TodoTask.ToString() &&
                    task.TodoTask.ToString() == existingHistory[1].TodoTask.ToString())
                {
                    // this one hasn't changed!  move the middle one up
                    existingHistory[0].TimeStamp = timestamp;
                }
                else
                {
                    // something is different.  add it.
                    OutputHistory.Add(new HierarchicalTask(task.ToString())
                    {
                        TimeStamp = timestamp
                    });
                }
            }
            OutputHistory = OutputHistory.OrderBy(x => x.TimeStamp).ThenBy(x => x.Id).ToList();
        }

        private void AssignFilledOutTotalEstimates()
        {
            foreach (var task in FilledOutList)
            {
                double totalEst = task.Estimate ?? 0.0;
                double totalRem = task.Remaining ?? 0.0;

                foreach (var deepchild in task.DeepChildren)
                {
                    totalEst += deepchild.Estimate ?? 0.0;
                    totalRem += deepchild.Remaining ?? 0.0;
                }

                task.TotalEstimate = totalEst;
                task.TotalRemaining = totalRem;
            }
        }

        private void RecursiveAddToDeepChildren(HierarchicalTask task, List<HierarchicalTask> taskChildren)
        {
            task.DeepChildren.AddRange(taskChildren);
            foreach (var childTask in taskChildren)
            {
                if (childTask.Children.Count > 0)
                    RecursiveAddToDeepChildren(task, childTask.Children);
            }
        }

        private void AssignFilledOutEstimatesIfMissing()
        {
            foreach (var task in FilledOutList)
            {
                if (task.TodoTask == null) continue;

                if (task.Estimate == null && !task.Children.Any())
                        task.Estimate = 1.0; 


                if (task.Estimate == null)
                    // parent, don't need to think about remaining. 
                    continue;

                if (task.Remaining != null)
                    // remaining already specified, DO NOT override. 
                    continue;

                task.Remaining = task.TodoTask.Completed ? 0.0 : task.Estimate;
            }
        }
        

        private void AssignOutputListIds()
        {
            foreach (var task in OutputList)
            {
                if (!task.WasParsed) continue; 
                var val = task.Id;
                if (!String.IsNullOrWhiteSpace(val)) continue;

                // parent Id's not assigned yet so this doesn't work. 

                for (int i = 1;; i++)
                {
                    var proposed = i.ToString();
                    if (OutputList.Any(t => t.WasParsed && t.Id == proposed)) continue;
                    task.Id = proposed;
                    break; 
                }
            }
        }

        public void OutputScanAndDateCompletedThings()
        {
            foreach (var task in OutputList)
            {
                if (!task.WasParsed) continue;
                if (task.TodoTask.Completed) continue;

                var task1 = task.TodoTask.ToString();
                if (task1.StartsWith("x "))
                {
                    // so, by a miracle -- TodoTask should have parsed x YYYY-MM-DD to a completed task. 
                    // if it starts with "x " but doesn't register as completd, it must be missing the yyyy-mm-dd
                    // so add it

                    var sb = new StringBuilder(task1);
                    if (sb.Length > 2)
                    {
                        if (!String.IsNullOrWhiteSpace(task1.Substring(2, 1)))
                        {
                            sb.Insert(2, ' ');
                        }
                    }
                    sb.Insert(2, DateTime.Now.ToString("yyyy-MM-dd"));
                    task.TodoTask.Replace(sb.ToString());
                }
            }
        }

        public void AssignFilledOutListParents()
        {
            //    if no row found, then root -- parent = self. 
            //    if row found, then that's my parent. 

            // TODO: probably should verify that we don't create any loops. 
            // this is probably already done as we only look backwards for a parent

            // for all the things that don't have parents
            for (int i = 0; i < FilledOutList.Count; i++)
            {
                var task = FilledOutList[i];
                if (task.ParentId != null) continue;

                for (var j = i - 1; j >= 0; j--)
                {
                    var ptask = FilledOutList[j];
                    if (ptask.Id==null) continue;
                    if (ptask.IndentLevel >= task.IndentLevel) continue;

                    task.ParentId = ptask.Id;
                    ptask.Children.Add(task);
                    goto outer;
                }
                // if we get here, didn't find anything that would work
                // no big deal, leave as null. 

                outer:
                ;
            }
        }

        public IEnumerable<string> GetOutputLines()
        {
            foreach (var task in OutputList)
            {
                yield return task.ToString();
            }
        }

        public IEnumerable<Tuple<DateTime, string>> GetOutputHistory()
        {
            foreach (var task in OutputHistory.Where(t => t.WasParsed))
            {
                yield return new Tuple<DateTime, string>(task.TimeStamp, task.TodoTask.ToString());
            }
        }
    }
}