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
    public class MsSubjectSelectionPeriod : AuditEntity, IStudentEntity
    {
        public string IdGrade { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrStudentSubjectSelection> StudentSubjectSelections { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionMajor> StudentSubjectSelectionMajors { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionCountry> StudentSubjectSelectionCountries { get; set; }

    }

    internal class MsSubjectSelectionPeriodConfiguration : AuditEntityConfiguration<MsSubjectSelectionPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSelectionPeriod> builder)
        {
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.SubjectSelectionPeriods)
                .HasForeignKey(x => x.IdGrade)
                .HasConstraintName("FK_MsSubjectSelectionPeriod_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}