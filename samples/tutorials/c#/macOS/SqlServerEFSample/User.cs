using System;
using System.Collections.Generic;

namespace SqlServerEFSample
{
    public class User
    {
        public int UserId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public virtual IList<Task> Tasks { get; set; }

        public String GetFullName()
        {
            return this.FirstName + " " + this.LastName;
        }

        public object AsLogEntry() => new { UserId, Name=GetFullName() };
        public override string ToString() => AsLogEntry().ToString();
    }
}