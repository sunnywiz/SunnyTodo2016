﻿using System;
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
            TestTarget.LoadInputList(SplitContents(SAMPLETESTS1));
            Assert.AreEqual(4, TestTarget.InputList.Count(x => x.TodoTask != null), "Should find 4 filled out tasks");
        }

        [Test]
        public void Process_OutputItems_with_existing_ids_keep_their_ids()
        {
            TestTarget.LoadInputList(SplitContents(SAMPLETESTS1));
            TestTarget.Process();
            Assert.AreEqual("1", TestTarget.OutputList[2].Id);
        }

        [Test]
        public void Process_OutputItems_without_ids_get_new_ids()
        {
            TestTarget.LoadInputList(SplitContents(SAMPLETESTS1));

            TestTarget.Process();

            Assert.IsNotEmpty(TestTarget.OutputList[0].Id);
            Assert.IsNotEmpty(TestTarget.OutputList[1].Id);
        }

        private const string PARENTTEST = @"
A1 id:1
  B1 id:2
  B2 id:3
    C1 id:4
  B3 id:5
 B4 id:6";

        [Test]
        [TestCase("root node gets null", "1", null)]
        [TestCase("B1 gets A1", "2", "1")]
        [TestCase("B2 skips B1 and gets A1", "3", "1")]
        [TestCase("C1 gets B2", "4", "3")]
        [TestCase("B3 skips all the way back to A1", "5", "1")]
        [TestCase("B4 though indented wierd gets A1", "6", "1")]
        public void Process_FilledOutList_gets_parental_assignment(string description, string id, string parentid)
        {
            TestTarget.LoadInputList(
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
            TestTarget.LoadInputList(SplitContents(IGNOREDLINETEST));

            TestTarget.Process();

            Assert.AreEqual(3, TestTarget.OutputList.Count);
            Assert.IsTrue(TestTarget.OutputList[0].ToString().StartsWith("#"), "Comment is preserved");
            Assert.IsTrue(String.IsNullOrWhiteSpace(TestTarget.OutputList[1].ToString()),"Whitespace is preserved");
        }

        [Test]
        public void UnderlyingLibrary_understands_x()
        {
            var task = new todotxtlib.net.Task("X 2005-06-03 Test");
            Assert.IsTrue(task.Completed,"should be marked as completed");
        }

        // TODO: make it so that we can just do "x . " and today's date is filled in
        private const string ESTTEST = @"
Task one id:1 est:3 rem:1
Task two id:2
x 2005-06-03 Task three id:3 est:2.5
";


        [Test]
        [TestCase("Given estimate and remaining are preserved","1",3.0, 1.0)]
        [TestCase("If no estimate given, uses 1.0, copy to rem","2",1.0, 1.0)]
        [TestCase("Decimal estimate is ok, and completed task has no remaining", "3", 2.5, 0.0)]
        public void Process_assings_estimates(string description, string id, double est, double rem)
        {
            TestTarget.LoadInputList(SplitContents(ESTTEST));
            TestTarget.Process();

            var task = TestTarget.FilledOutList.FirstOrDefault(x => x.Id == id);
            if (task == null) Assert.Fail("Could not find line with id "+id);

            Assert.AreEqual(est, task.Estimate, description);
            Assert.AreEqual(rem, task.Remaining, description);
        }

        private const string TOTALTEST = @"
A id:1
  B id:2 rem:0.5
  x 2012-12-12 C est:2 id:3
  D id:4
    E id:5
";

        [Test]
        [TestCase("Leaf Node E only has self numbers","5",1.0, 1.0)]
        [TestCase("D has E numbers because child, but not self.", "4", 1.0, 1.0)]
        [TestCase("C only counts self but rem is modified due to x", "3", 2.0, 0.0)]
        [TestCase("B only counts self, assumes 1", "2", 1.0, 0.5)]
        [TestCase("A has all children but not self", "1", 4.0, 1.5)]

        // TODO: make it so that a parent with a direct est: feeds ratios of that to all leaves without estimates. 

        public void Process_calculates_tree_totals(string description, string id, double totalEstimate, double totalRemaining)
        {
            TestTarget.LoadInputList(SplitContents(TOTALTEST));
            TestTarget.Process();

            var task = TestTarget.FilledOutList.FirstOrDefault(x => x.Id == id);
            if (task == null) Assert.Fail("Could not find line with id " + id);

            Assert.AreEqual(totalEstimate, task.TotalEstimate, description);
            Assert.AreEqual(totalRemaining, task.TotalRemaining, description);
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
