using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;


namespace SunnyTodo2016
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <2 )
            {
                Console.WriteLine("Usage:  $0 snap|report <filename>");
                Console.WriteLine(" for now see source.  its commented out");
                //Console.WriteLine("Usage:  BurndownConsole2016 <todo.txt> [-h historyFileName] [-ot filename2] [-oh historyFileToWrite] [-r [reportfile]]");
                //Console.WriteLine("");
                //Console.WriteLine("Result: ");
                //Console.WriteLine(" a) reads, and updates, filename.history.json for historical information on tasks.");
                //Console.WriteLine(" b) writes out filename2, or if not specified, updates filename in place");
                //Console.WriteLine(" b) writes out filename2, or if not specified, updates filename in place");
                //Console.WriteLine("");
                //Console.WriteLine("Purpose:");
                //Console.WriteLine("  First Read:  https://github.com/ginatrapani/todo.txt-cli/wiki/The-Todo.txt-Format");
                //Console.WriteLine("  Do that, we look for and/or add:");
                //Console.WriteLine("      id:xxx -- if no id assigned, we add an id. ");
                //Console.WriteLine("      pid:xxx -- tasks vs subtasks.  Uses indentation to assign parents. ");
                //Console.WriteLine("      est:xxx -- Estmated amount of work. feel free to change ");
                //Console.WriteLine("      rem:xxx -- Remaining amount of work.  a done task is rem:0 ");
                //Console.WriteLine("");
                //Console.WriteLine("  Additionally, the following tags are generated & updated, though really saved in history only.");
                //Console.WriteLine("      ttlest:xxx -- total estimated");
                //Console.WriteLine("      ttlrem:xxx -- total remaining");
                //Console.WriteLine("");
                //Console.WriteLine("Then, if -r is also specified, generates a report file with burndowns and stuff.");

                return; 
            }

            if (args[0] == "snap")
            {
                var fileName = args[1];
                DoTakeSnapshot(fileName);
                return;
            } else if (args[0] == "report")
            {
                var fileName = args[1];
                DoReport(fileName);
            }

            Console.ReadLine();
        }

        private static void DoReport(string fileName)
        {
            var historyFileName = fileName + ".history.txt";

            if (!File.Exists(historyFileName))
            {
                Console.WriteLine("History file could not be read.");
            }

            var logic = new HierarchicalTaskEngine();
            List<Tuple<DateTime, string>> parsed = new List<Tuple<DateTime, string>>();
            var historyLines = File.ReadAllLines(historyFileName);
            foreach (var hl in historyLines)
            {
                var index = hl.IndexOf('|');
                if (index < 0) continue;
                DateTime timestamp;
                var k1 = hl.Substring(0, index);
                var k2 = hl.Substring(index + 1);
                if (DateTime.TryParse(k1, null, DateTimeStyles.RoundtripKind, out timestamp))
                {
                    parsed.Add(new Tuple<DateTime, string>(timestamp, k2));
                }
            }
            logic.LoadInputHistory(parsed);
            logic.OutputHistory.Clear(); 
            logic.OutputHistory.AddRange(logic.InputHistory);
            logic.InterpolateHistory();

            var distinctTimes = logic.InterpolatedHistory.Select(x => x.TimeStamp).Distinct().OrderBy(x => x).ToList();
            var distinctIds = logic.InterpolatedHistory.Select(x => x.Id).Distinct().OrderBy(x => x).ToList();

            Console.Write("Timestamp");
            foreach (var id in distinctIds)
            {
                Console.Write(",C"+id);
            }
            Console.WriteLine();
            foreach (var time in distinctTimes)
            {
                Console.Write($"{time:g}");
                foreach (var id in distinctIds)
                {
                    var rec = logic.InterpolatedHistory.FirstOrDefault(x => x.TimeStamp == time && x.Id == id);
                    if (rec == null)
                    {
                        Console.Write(",");
                        continue; 
                    }

                    if (rec.Remaining.HasValue && rec.Estimate == rec.TotalEstimate && rec.Remaining == rec.TotalRemaining)
                    {
                        // this is a child thing. 
                        Console.Write($",{rec.Remaining.Value:F}");
                        continue; 
                    }

                    // ths is not a child thing
                    Console.Write(",");
                }
                Console.WriteLine(); 
            }

        }

        private static void DoTakeSnapshot(string fileName)
        {
            var historyFileName = fileName + ".history.txt";

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File could not be read?");
            }

            var lines = File.ReadAllLines(fileName);

            var logic = new HierarchicalTaskEngine();
            logic.LoadInputList(lines);

            if (File.Exists(historyFileName))
            {
                List<Tuple<DateTime, string>> parsed = new List<Tuple<DateTime, string>>();
                var historyLines = File.ReadAllLines(historyFileName);
                foreach (var hl in historyLines)
                {
                    var index = hl.IndexOf('|');
                    if (index < 0) continue;
                    DateTime timestamp;
                    var k1 = hl.Substring(0, index);
                    var k2 = hl.Substring(index + 1);
                    if (DateTime.TryParse(k1, null, DateTimeStyles.RoundtripKind, out timestamp))
                    {
                        parsed.Add(new Tuple<DateTime, string>(timestamp, k2));
                    }
                }
                logic.LoadInputHistory(parsed);
            }


            logic.Process();

            using (var writer = File.CreateText(fileName))
            {
                foreach (string line in logic.GetOutputLines())
                {
                    writer.WriteLine(line);
                }
            }

            using (var writer = File.CreateText(historyFileName))
            {
                foreach (var item in logic.GetOutputHistory())
                {
                    writer.WriteLine($"{item.Item1:o}|{item.Item2}");
                }
            }

            foreach (var task in logic.FilledOutList)
            {
                if (task.WasParsed && task.ParentId == null)
                {
                    Console.WriteLine(task.ToString());

                    Console.WriteLine();
                    Console.WriteLine("Timestamp,Estimated,Left");
                    foreach (var ht in logic.OutputHistory.Where(t => t.Id == task.Id).OrderBy(t => t.TimeStamp))
                    {
                        Console.WriteLine($"{ht.TimeStamp:g},{ht.TotalEstimate},{ht.TotalRemaining}");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
