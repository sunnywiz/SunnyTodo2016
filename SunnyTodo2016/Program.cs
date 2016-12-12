using System;
using System.IO;


namespace SunnyTodo2016
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:  BurndownConsole2016 <todo.txt> [-h historyFileName] [-ot filename2] [-oh historyFileToWrite] [-r [reportfile]]");
                Console.WriteLine("");
                Console.WriteLine("Result: ");
                Console.WriteLine(" a) reads, and updates, filename.history.json for historical information on tasks.");
                Console.WriteLine(" b) writes out filename2, or if not specified, updates filename in place");
                Console.WriteLine(" b) writes out filename2, or if not specified, updates filename in place");
                Console.WriteLine("");
                Console.WriteLine("Purpose:");
                Console.WriteLine("  First Read:  https://github.com/ginatrapani/todo.txt-cli/wiki/The-Todo.txt-Format");
                Console.WriteLine("  Do that, we look for and/or add:");
                Console.WriteLine("      id:xxx -- if no id assigned, we add an id. ");
                Console.WriteLine("      pid:xxx -- tasks vs subtasks.  Uses indentation to assign parents. ");
                Console.WriteLine("      est:xxx -- Estmated amount of work. feel free to change ");
                Console.WriteLine("      rem:xxx -- Remaining amount of work.  a done task is rem:0 ");
                Console.WriteLine("");
                Console.WriteLine("  Additionally, the following tags are generated & updated, though really saved in history only.");
                Console.WriteLine("      ttlest:xxx -- total estimated");
                Console.WriteLine("      ttlrem:xxx -- total remaining");
                Console.WriteLine("");
                Console.WriteLine("Then, if -r is also specified, generates a report file with burndowns and stuff.");

                return; 
            }

            var fileName = args[0];
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File could not be read?");
            }

            var lines = File.ReadAllLines(fileName);

            var logic = new HierarchicalTaskEngine();
            logic.LoadFromFileContents(lines);
            logic.Process();

            foreach (var task in logic.OutputList)
            {
                Console.WriteLine(task.Indent + task.TodoTask.ToString());
            }
            Console.WriteLine("press return to continue");
            Console.ReadLine();
        }
    }
}
