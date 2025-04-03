using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities.School
{
    public class MsSubjectMappingSubjectLevel : AuditEntity, IStudentEntity
    {
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public bool IsDefault { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsSubjectLevel SubjectLevel { get; set; }
    }

    internal class MsSubjectMappingSubjectLevelConfiguration : AuditEntityConfiguration<MsSubjectMappingSubjectLevel>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectMappingSubjectLevel> builder)
        {
            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectMappingSubjectLevels)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsSubjectMappingSubjectLevel_MsSubject")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectLevel)
               .WithMany(x => x.SubjectMappingSubjectLevels)
               .HasForeignKey(fk => fk.IdSubjectLevel)
               .HasConstraintName("FK_MsSubjectMappingSubjectLevel_MsSubjectLevel")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}