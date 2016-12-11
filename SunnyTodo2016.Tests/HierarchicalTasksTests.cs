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

TestA 
   TestB 
   TEST C  id:1
  Test D
";

        [Test]
        public void LoadFileFromContents_Empty_lines_are_skipped()
        {
            var htasks = HierarchicalTasks.LoadFromFileContents(SplitContents(SAMPLETESTS1));
            Assert.AreEqual(4, htasks.Count, "Should find 3 tests");
        }

        [Test]
        public void LoadFileFromContents_Items_with_existing_ids_keep_their_ids()
        {
            var htasks = HierarchicalTasks.LoadFromFileContents(SplitContents(SAMPLETESTS1));
            Assert.AreEqual(1, htasks[2].Id);
        }

        [Test]
        public void LoadFileFromContents_Items_without_ids_get_new_ids()
        {
            var htasks = HierarchicalTasks.LoadFromFileContents(SplitContents(SAMPLETESTS1));

            Assert.AreEqual(2, htasks[0].Id);
            Assert.AreEqual(3, htasks[1].Id);
        }

        private const string PARENTTEST = @"
A1 id:1
  B1 id:2
  B2 id:3
    C1 id:4
  B3 id:5
 B4 id:6";

        [Test]
        [TestCase("root node gets self",1,1)]
        [TestCase("B1 gets A1",2,1)]
        [TestCase("B2 skips B1 and gets A1",3, 1)]
        [TestCase("C1 gets B2", 4, 3)]
        [TestCase("B3 skips all the way back to A1", 5, 1)]
        [TestCase("B4 though indented wierd gets A1", 6, 1)]
        public void LoadFileFromContents_ParentalAssignment(string description, int id, int parentid)
        {
            var htasks = HierarchicalTasks.LoadFromFileContents(
                SplitContents(PARENTTEST));
            var task = htasks.FirstOrDefault(t => t.Id == id);
            if (task == null) Assert.Fail("could not find the task");
            Assert.AreEqual(parentid, task.ParentId, description);
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
