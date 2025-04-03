using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
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
    public class TrStudentSubjectSelectionCountry : AuditEntity, IStudentEntity
    {
        public string IdMappingCountryGrade { get; set; }
        public string IdStudent { get; set; }
        public string? IdSubjectSelectionPeriod { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdCurrentGrade { get; set; }
        public string? OtherSelectionCountry { get; set; }
        public string IdUserAction { get; set; }
        public virtual MsMappingCountryGrade MappingCountryGrade { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsSubjectSelectionPeriod SubjectSelectionPeriod { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade CurrentGrade { get; set; }
    }

    internal class TrStudentSubjectSelectionCountryConfiguration : AuditEntityConfiguration<TrStudentSubjectSelectionCountry>
    {
        public override void Configure(EntityTypeBuilder<TrStudentSubjectSelectionCountry> builder)
        {
            builder.HasOne(x => x.MappingCountryGrade)
                .WithMany(x => x.StudentSubjectSelectionCountries)
                .HasForeignKey(x => x.IdMappingCountryGrade)
                .HasConstraintName("FK_TrStudentSubjectSelectionCountry_MsMappingCountryGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentSubjectSelectionCountries)
                .HasForeignKey(x => x.IdStudent)
                .HasConstraintName("FK_TrStudentSubjectSelectionCountry_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectSelectionPeriod)
                .WithMany(x => x.StudentSubjectSelectionCountries)
                .HasForeignKey(x => x.IdSubjectSelectionPeriod)
                .HasConstraintName("FK_TrStudentSubjectSelectionCountry_MsSubjectSelectionPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.CurrentGrade)
                .WithMany(x => x.StudentSubjectSelectionCountries)
                .HasForeignKey(x => x.IdCurrentGrade)
                .HasConstraintName("FK_TrStudentSubjectSelectionCountry_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.StudentSubjectSelectionCountries)
                .HasForeignKey(x => x.IdAcademicYear)
                .HasConstraintName("FK_TrStudentSubjectSelectionCountry_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}