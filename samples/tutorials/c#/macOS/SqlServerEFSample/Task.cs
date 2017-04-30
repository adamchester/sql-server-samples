using System;

namespace SqlServerEFSample
{
    public class Task
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsComplete { get; set; }
        public virtual User AssignedTo { get; set; }

        public object AsLogEntry() => new { TaskId, Title, DueDate };
        public override string ToString() => AsLogEntry().ToString();
    }
}