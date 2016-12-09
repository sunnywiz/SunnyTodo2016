using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SunnyTodo2016;

namespace SunnyTodo2016.Tests
{
    [TestFixture]
    public class HierarchicalTasksTests
    {
        private const string SAMPLETESTS1 = @"

Test1
   Test1A
   Test1B

Test2
   Test2A
     Test2AB

";

        [Test]
        public void Test1()
        {
            var htasks = HierarchicalTasks.LoadFromFileContents(SplitContents(SAMPLETESTS1));
        }

        private string[] SplitContents(string multiLineString)
        {
            List<string> result = new List<string>();
            using (var reader = new StringReader(multiLineString))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    result.Add(line); 
                }
            }
            return result.ToArray();
        }
    }
}
