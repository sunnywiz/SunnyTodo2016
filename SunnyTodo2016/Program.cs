using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurndownConsole2016
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:  BurndownConsole2016 <todo.txt> [-o filename2]");
                Console.WriteLine("");
                Console.WriteLine("Result: ");
                Console.WriteLine(" a) reads, and updates, filename.history.json for historical information on tasks.");
                Console.WriteLine(" b) writes out filename2, or if not specified, updates filename in place");
                Console.WriteLine("");
                Console.WriteLine("Purpose:");
                Console.WriteLine("  First Read:  https://github.com/ginatrapani/todo.txt-cli/wiki/The-Todo.txt-Format");
                Console.WriteLine("  Do that, we look for and/or add:");
                Console.WriteLine("      id:xxx -- if no id assigned, we add an id. ");
                Console.WriteLine("      parent:xxx -- tasks vs subtasks.  Uses indentation to assign parents. ");
                Console.WriteLine("      est:xxx -- Estmated amount of work. feel free to change ");
                Console.WriteLine("      rem:xxx -- Remaining amount of work.  a done task is rem:0 ");
                Console.WriteLine("");
                Console.WriteLine("  Additionally, the following tags are generated & updated:");
                Console.WriteLine("      ttlest:xxx -- total estimated");
                Console.WriteLine("      ttlrem:xxx -- total remaining");

                return; 
            }

            var fileName = args[0];
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File could not be read?");
            }

            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                try
                {
                    // @"(?:(?<done>[xX] (?:(?<completeddate>[0-9]{4}-[0-9]{2}-[0-9]{2}) )))?(?:\((?<priority>[A-Z])\) )?(?:(?<createddate>[0-9]{4}-[0-9]{2}-[0-9]{2}) )?(?<todo>.+)$");

                    var task = new todotxtlib.net.Task(line);
                    Console.WriteLine(task.Body);
                    Console.WriteLine($"  Priority: {task.Priority}");
                    Console.WriteLine($"  Created: {task.CreatedDate}");
                    Console.WriteLine($"  Completed: {task.CompletedDate}");
                    Console.WriteLine($"  Projects: {String.Join("|",task.Projects)}");
                    Console.WriteLine($"  Contexts: {String.Join("|",task.Contexts)}");
                    foreach (var meta in task.Metadata)
                    {
                        Console.WriteLine($"  Meta: {meta.Key}:{meta.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not parse line: "+line);
                }
            }
            Console.ReadLine(); 
        }
    }
}
