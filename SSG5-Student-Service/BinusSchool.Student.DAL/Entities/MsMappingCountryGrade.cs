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
    public class MsMappingCountryGrade : AuditEntity, IStudentEntity
    {
        public string IdSubjectSelectionCountry { get; set; }
        public string IdGrade { get; set; }
        public virtual LtSubjectSelectionCountry SubjectSelectionCountry { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionCountry> StudentSubjectSelectionCountries { get; set; }

    }

    internal class MsMappingCountryGradeConfiguration : AuditEntityConfiguration<MsMappingCountryGrade>
    {
        public override void Configure(EntityTypeBuilder<MsMappingCountryGrade> builder)
        {
            builder.HasOne(x => x.SubjectSelectionCountry)
                .WithMany(x => x.MappingCountryGrades)
                .HasForeignKey(x => x.IdSubjectSelectionCountry)
                .HasConstraintName("FK_MsMappingCountryGrade_LtSubjectSelectionCountry")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.MappingCountryGrades)
                .HasForeignKey(x => x.IdGrade)
                .HasConstraintName("FK_MsMappingCountryGrade_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}