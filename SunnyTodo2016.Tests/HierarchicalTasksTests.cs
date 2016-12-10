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
        public void LoadFileFromContents_ParentalAssignment_roots_get_self()
        {
            var htasks = HierarchicalTasks.LoadFromFileContents(
                SplitContents(PARENTTEST));
            Assert.AreEqual(1, htasks[0].ParentId);
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
