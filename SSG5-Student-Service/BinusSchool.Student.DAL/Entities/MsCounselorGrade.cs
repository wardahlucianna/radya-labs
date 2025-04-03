using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsCounselorGrade : AuditEntity, IStudentEntity
    {
        public string IdCounselor { get; set; }
        public string IdGrade { get; set; }

        public virtual MsCounselor Counselor { get; set; }    
        public virtual MsGrade Grade { get; set; }    
    }

    internal class MsCounselorGradeConfiguration : AuditEntityConfiguration<MsCounselorGrade>
    {
        public override void Configure(EntityTypeBuilder<MsCounselorGrade> builder)
        {

            builder.HasOne(x => x.Counselor)
             .WithMany(x => x.CounselorGrade)
             .HasForeignKey(fk => fk.IdCounselor)
             .HasConstraintName("FK_MsCounselorGrade_MsCounselor")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Grade)
             .WithMany(x => x.CounselorGrade)
             .HasForeignKey(fk => fk.IdGrade)
             .HasConstraintName("FK_MsCounselorGrade_MsGrade")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
