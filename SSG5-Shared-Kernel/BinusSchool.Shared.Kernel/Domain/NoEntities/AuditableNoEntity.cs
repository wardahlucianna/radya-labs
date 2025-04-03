using System;

namespace BinusSchool.Domain.NoEntities
{
    public abstract class AuditableNoEntity
    {
        public string UserIn { get; set; }
        public DateTime DateIn { get; set; }
        public string UserUp { get; set; }
        public DateTime DateUp { get; set; }
    }
}