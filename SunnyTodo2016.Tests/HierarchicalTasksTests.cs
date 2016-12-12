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
        private const string SAMPLETESTS1 = @"TestA 
   TestB 
   TEST C  id:1
  Test D



";

        HierarchicalTaskEngine TestTarget = new HierarchicalTaskEngine();

        [SetUp]
        public void SetUp()
        {
            TestTarget = new HierarchicalTaskEngine();
        }


        [Test]
        public void LoadFileFromContents_loads_some_lines()
        {
            TestTarget.LoadFromFileContents(SplitContents(SAMPLETESTS1));
            Assert.AreEqual(4, TestTarget.InputList.Count(x => x.TodoTask != null), "Should find 4 filled out tasks");
        }

        [Test]
        public void Process_OutputItems_with_existing_ids_keep_their_ids()
        {
            TestTarget.LoadFromFileContents(SplitContents(SAMPLETESTS1));
            TestTarget.Process();
            Assert.AreEqual(1, TestTarget.OutputList[2].Id);
        }

        [Test]
        public void Process_OutputItems_without_ids_get_new_ids()
        {
            TestTarget.LoadFromFileContents(SplitContents(SAMPLETESTS1));

            TestTarget.Process();

            Assert.AreEqual(2, TestTarget.OutputList[0].Id);
            Assert.AreEqual(3, TestTarget.OutputList[1].Id);
        }

        private const string PARENTTEST = @"
A1 id:1
  B1 id:2
  B2 id:3
    C1 id:4
  B3 id:5
 B4 id:6";

        [Test]
        [TestCase("root node gets self", 1, 1)]
        [TestCase("B1 gets A1", 2, 1)]
        [TestCase("B2 skips B1 and gets A1", 3, 1)]
        [TestCase("C1 gets B2", 4, 3)]
        [TestCase("B3 skips all the way back to A1", 5, 1)]
        [TestCase("B4 though indented wierd gets A1", 6, 1)]
        public void Process_FilledOutList_gets_parental_assignment(string description, int id, int parentid)
        {
            TestTarget.LoadFromFileContents(
                SplitContents(PARENTTEST));

            TestTarget.Process();

            var task = TestTarget.FilledOutList.FirstOrDefault(t => t.Id == id);
            if (task == null) Assert.Fail("could not find the task");
            Assert.AreEqual(parentid, task.ParentId, description);
        }


        private const string IGNOREDLINETEST =
            @"# This is a comment

# there was a blank line above this comment";

        [Test]
        public void LoadFromFileContents_does_keep_track_of_blank_lines_and_comment_lines()
        {
            TestTarget.LoadFromFileContents(SplitContents(IGNOREDLINETEST));

            TestTarget.Process();

            Assert.AreEqual(3, TestTarget.OutputList.Count);
            Assert.IsTrue(TestTarget.OutputList[0].ToString().StartsWith("#"), "Comment is preserved");
            Assert.IsTrue(String.IsNullOrWhiteSpace(TestTarget.OutputList[1].ToString()),"Whitespace is preserved");
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
