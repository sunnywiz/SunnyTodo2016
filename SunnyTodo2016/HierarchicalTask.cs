using System;
using NUnit.Framework;

namespace SunnyTodo2016
{
    public class HierarchicalTask
    {
        // TODO: need to track comments and blank lines as well. 

        public bool WasParsed { get; set; }

        public HierarchicalTask(string originalLine)
        {
            TodoTask = null; 
            _originalLine = originalLine;
            _trimmedAtStartLine = originalLine.TrimStart();
            if (string.IsNullOrWhiteSpace(originalLine))
            {
                WasParsed = false;
                return;
            }
            if (_trimmedAtStartLine.StartsWith("#"))
            {
                WasParsed = false;
                return;
            }
            TodoTask = new todotxtlib.net.Task(originalLine);
            WasParsed = true;
        }

        private readonly string _originalLine;
        private readonly string _trimmedAtStartLine; 
        public todotxtlib.net.Task TodoTask { get; private set; }

        // TODO: could modify IndentLevel to be tab-as-8-spaces aware.
        public int IndentLevel => _originalLine.Length - _trimmedAtStartLine.Length;

        public int? Id
        {
            get
            {
                if (TodoTask == null) return null;
                string id;
                if (!TodoTask.Metadata.TryGetValue("id", out id)) return null;
                int numberid;
                if (int.TryParse(id, out numberid))
                {
                    return numberid;
                }
                return null;
            }
            set
            {
                TodoTask?.Append(" id:" + value.ToString());
            }
        }

        public int? ParentId
        {
            get
            {
                if (TodoTask == null) return null;
                string id;
                if (!TodoTask.Metadata.TryGetValue("pid", out id)) return null;
                int numberid;
                if (int.TryParse(id, out numberid))
                {
                    return numberid;
                }
                return null;
            }
            set
            {
                // bugs: case of replace, and case of remove
                TodoTask?.Append(" pid:" + value.ToString());
            }
        }

        public double? Estimate
        {
            get
            {
                if (TodoTask == null) return null;
                string xx;
                if (!TodoTask.Metadata.TryGetValue("est", out xx)) return null;
                double estimate;
                if (double.TryParse(xx, out estimate))
                {
                    return estimate;
                }
                return 0.0;
            }
            set
            {
                // bugs: case of replace, and case of remove
                if (value.HasValue)
                {
                    TodoTask?.Append(" est:" + value.Value.ToString("N"));
                }
            }
        }

        public override string ToString()
        {
            if (WasParsed)
            {
                return "".PadRight(this.IndentLevel) + TodoTask.ToString();
            }
            else
            {
                return _originalLine;
            }
        }
    }
}