using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsSurveyRespondent : AuditEntity, IStudentEntity
    {
        public string IdSurvey { get; set; }
        public string Role { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
        public string IdParent { get; set; }
        public string IdStaff { get; set; }

        public virtual MsSurvey Survey { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsParent Parent { get; set; }
        public virtual MsStaff Staff { get; set; }
    }
    internal class MsSurveyRespondentConfiguration : AuditEntityConfiguration<MsSurveyRespondent>
    {
        public override void Configure(EntityTypeBuilder<MsSurveyRespondent> builder)
        {

            builder.Property(x => x.IdSurvey)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.Role)
              .HasMaxLength(50)
              .IsRequired();

            builder.Property(x => x.IdLevel)
             .HasMaxLength(36);

            builder.Property(x => x.IdGrade)
             .HasMaxLength(36);

            builder.Property(x => x.IdStudent)
             .HasMaxLength(36);

            builder.Property(x => x.IdParent)
             .HasMaxLength(36);

            builder.Property(x => x.IdStaff)
             .HasMaxLength(36);

            builder.HasOne(x => x.Survey)
             .WithMany(x => x.SurveyRespondents)
             .HasForeignKey(fk => fk.IdSurvey)
             .HasConstraintName("FK_MsSurveyRespondent_MsSurvey")
             .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.Level)
             .WithMany(x => x.SurveyRespondents)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_MsSurveyRespondent_MsLevel")
             .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Grade)
              .WithMany(x => x.SurveyRespondents)
              .HasForeignKey(fk => fk.IdGrade)
              .HasConstraintName("FK_MsSurveyRespondent_MsGrade")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Student)
             .WithMany(x => x.SurveyRespondents)
             .HasForeignKey(fk => fk.IdStudent)
             .HasConstraintName("FK_MsSurveyRespondent_MsStudent")
             .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Parent)
           .WithMany(x => x.SurveyRespondents)
           .HasForeignKey(fk => fk.IdParent)
           .HasConstraintName("FK_MsSurveyRespondent_MsParent")
           .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Staff)
           .WithMany(x => x.SurveyRespondents)
           .HasForeignKey(fk => fk.IdStaff)
           .HasConstraintName("FK_MsSurveyRespondent_MsStaff")
           .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
