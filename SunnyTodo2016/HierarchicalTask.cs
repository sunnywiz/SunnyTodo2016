using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SunnyTodo2016
{
    public class HierarchicalTask
    {
        // these are only stored in the detail so they can be long
        private const string KEY_PARENTID = "parentid";
        private const string KEY_TOTALESTIMATE = "totalestimate";
        private const string KEY_TOTALREMAINING = "totalremaining";

        // these are saved back to the original so they need to be short
        private const string KEY_ID = "id";
        private const string KEY_ESTIMATE = "est";
        private const string KEY_REMAINING = "rem";

        // TODO: need to track comments and blank lines as well. 

        public bool WasParsed { get; set; }

        public List<HierarchicalTask> Children { get; private set; }
        public List<HierarchicalTask> DeepChildren { get; private set; }

        public HierarchicalTask(string originalLine)
        {
            Children = new List<HierarchicalTask>();
            DeepChildren = new List<HierarchicalTask>();

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
                if (!TodoTask.Metadata.TryGetValue(KEY_ID, out id)) return null;
                int numberid;
                if (int.TryParse(id, out numberid))
                {
                    return numberid;
                }
                return null;
            }
            set
            {
                TodoTask?.Append($" {KEY_ID}:{value}");
            }
        }

        public int? ParentId
        {
            get
            {
                if (TodoTask == null) return null;
                string id;
                if (!TodoTask.Metadata.TryGetValue(KEY_PARENTID, out id)) return null;
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
                TodoTask?.Append($" {KEY_PARENTID}:{value}");
            }
        }

        public double? Estimate
        {
            get
            {
                return GetDouble(KEY_ESTIMATE);
            }
            set
            {
                SetDouble(KEY_ESTIMATE, value);
            }
        }

        public double? Remaining
        {
            get
            {
                return GetDouble(KEY_REMAINING);
            }
            set
            {
                SetDouble(KEY_REMAINING, value);
            }
        }

        public double? TotalEstimate
        {
            get
            {
                return GetDouble(KEY_TOTALESTIMATE);
            }
            set
            {
                SetDouble(KEY_TOTALESTIMATE, value);
            }
        }

        public double? TotalRemaining
        {
            get
            {
                return GetDouble(KEY_TOTALREMAINING);
            }
            set
            {
                SetDouble(KEY_TOTALREMAINING, value);
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

        private void SetDouble(string key, double? value)
        {
// bugs: case of replace, and case of remove
            if (value.HasValue)
            {
                TodoTask?.Append(" " + key + ":" + value.Value.ToString("N"));
            }
        }

        private double? GetDouble(string key)
        {
            if (TodoTask == null) return null;
            string xx;
            if (!TodoTask.Metadata.TryGetValue(key, out xx)) return null;
            double estimate;
            if (double.TryParse(xx, out estimate))
            {
                return estimate;
            }
            return 0.0;
        }
    }
}