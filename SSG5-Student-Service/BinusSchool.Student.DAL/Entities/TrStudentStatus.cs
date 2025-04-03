﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentStatus : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdTrStudentStatus { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public int IdStudentStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CurrentStatus { get; set; }
        public string Remarks { get; set; }
        public bool ActiveStatus { get; set; }
        public int? IdStudentStatusSpecial { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual LtStudentStatus StudentStatus { get; set; }
        public virtual LtStudentStatusSpecial StudentStatusSpecial { get; set; }
        public virtual ICollection<TrStudentFutureAdmissionDecision> StudentFutureAdmissionDecisions { get; set; }
        public virtual ICollection<HTrStudentStatus> HTrStudentStatuss { get; set; }
    }
    internal class TrStudentStatusConfiguration : AuditNoUniqueEntityConfiguration<TrStudentStatus>
    {
        public override void Configure(EntityTypeBuilder<TrStudentStatus> builder)
        {
            builder.HasKey(x => x.IdTrStudentStatus);

            builder.Property(x => x.IdTrStudentStatus)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdAcademicYear)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdStudent)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.EndDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.CurrentStatus)
                .HasColumnType("CHAR(1)")
                .IsRequired();

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            builder.HasOne(x => x.AcademicYear)
               .WithMany(y => y.TrStudentStatuss)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_TrStudentStatus_MsAcademicYear")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Student)
                .WithMany(y => y.TrStudentStatuss)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrStudentStatus_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.StudentStatus)
               .WithMany(y => y.TrStudentStatuss)
               .HasForeignKey(fk => fk.IdStudentStatus)
               .HasConstraintName("FK_TrStudentStatus_LTStudentStatus")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StudentStatusSpecial)
                .WithMany(y => y.TrStudentStatuses)
                .HasForeignKey(fk => fk.IdStudentStatusSpecial)
                .HasConstraintName("FK_TrStudentStatus_LtStudentStatusSpecial")
                .OnDelete(DeleteBehavior.Restrict);


            base.Configure(builder);
        }

    }
}
