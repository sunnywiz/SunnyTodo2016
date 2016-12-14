using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace SunnyTodo2016
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <2 )
            {
                Console.WriteLine("Usage:  $0 snap <filename>");
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
                var historyFileName = fileName + ".history.txt";

                if (!File.Exists(fileName))
                {
                    Console.WriteLine("File could not be read?");
                    return;
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
                        if (DateTime.TryParse(k1, null, DateTimeStyles.AssumeUniversal, out timestamp))
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
                        writer.WriteLine($"{item.Item1:s}|{item.Item2}");
                    }
                }

                foreach (var task in logic.FilledOutList)
                {
                    if (task.WasParsed && task.ParentId == null)
                    {
                        Console.WriteLine(task.ToString());
                    }
                }


            }

            Console.ReadLine();
        }
    }
}
