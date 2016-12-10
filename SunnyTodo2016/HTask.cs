namespace SunnyTodo2016
{
    public class HTask
    {
        public string Line { get; set; }
        public todotxtlib.net.Task TodoTask { get; set; }

        public int? Id
        {
            get
            {
                if (TodoTask == null) return null;
                string id;
                if (TodoTask.Metadata.TryGetValue("id", out id))
                {
                    int numberid;
                    if (int.TryParse(id, out numberid))
                    {
                        return numberid;
                    }
                }
                return null;
            }
            set
            {
                if (TodoTask == null) return;
                TodoTask.Append(" id:"+value.ToString());
            }
        }

        public int? ParentId { get; set; }
    }
}