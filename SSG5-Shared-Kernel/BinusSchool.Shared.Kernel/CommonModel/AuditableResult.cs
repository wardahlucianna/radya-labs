using System;

namespace BinusSchool.Common.Model
{
    public class AuditableResult
    {
        public AuditableUser UserIn { get; set; }
        public DateTime? DateIn { get; set; }
        public AuditableUser UserUp { get; set; }
        public DateTime? DateUp { get; set; }
    }

    public class AuditableUser
    {
        public AuditableUser() { }

        public AuditableUser(string id) : 
            this(id, null) { }

        public AuditableUser(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}