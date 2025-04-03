using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class MsSubjectSelectionSubjectAlias : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string SubjectIDInitial { get; set; }
        public string SubjectIDAlias { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsSubjectSelectionSubjectAliasConfiguration : AuditEntityConfiguration<MsSubjectSelectionSubjectAlias>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSelectionSubjectAlias> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.SubjectSelectionSubjectAliases)
                .HasForeignKey(x => x.IdAcademicYear)
                .HasConstraintName("FK_MsSubjectSelectionSubjectAlias_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
