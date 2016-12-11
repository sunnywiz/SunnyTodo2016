using System;

namespace SunnyTodo2016
{
    public class HTask
    {
        public HTask (string originalLine)
        {
            if (string.IsNullOrWhiteSpace(originalLine)) throw new ArgumentNullException("hey man need a line");
            OriginalLine = originalLine;
            _trimmedAtStartLine = originalLine.TrimStart();
            TodoTask = new todotxtlib.net.Task(originalLine);

        }

        public string OriginalLine { get; private set; }

        private readonly string _trimmedAtStartLine; 

        public todotxtlib.net.Task TodoTask { get; private set; }

        // TODO: could modify IndentLevel to be tab-as-8-spaces aware.
        public int IndentLevel => OriginalLine.Length - _trimmedAtStartLine.Length;

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
                TodoTask?.Append(" pid:" + value.ToString());
            }
        }
    }
}