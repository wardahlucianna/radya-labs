using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsFieldStudentData : AuditEntity, IStudentEntity
    {
            public string StudentData { get; set; }
            public string AliasName { get; set; }
            public string FlowTable { get; set; }
    }
    internal class MsFieldStudentDataConfiguration : AuditEntityConfiguration<MsFieldStudentData>
    {
        public override void Configure(EntityTypeBuilder<MsFieldStudentData> builder)
        {            
            
            builder.Property(x => x.StudentData)           
                .HasMaxLength(150);

            builder.Property(x => x.AliasName)           
                .HasMaxLength(150);

            builder.Property(x => x.FlowTable)
                .HasMaxLength(150);

            base.Configure(builder);
            
        }
    }
    
}
