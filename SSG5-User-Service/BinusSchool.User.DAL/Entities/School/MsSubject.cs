﻿using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsSubject : CodeEntity, IUserEntity
    {
        public string IdGrade { get; set; }
        public string IdDepartment { get; set; }
        public string SubjectID { get; set; }
        public int MaxSession { get; set; }
        
        public virtual MsGrade Grade { get; set; }
        public virtual MsDepartment Department { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
    }

    internal class MsSubjectConfiguration : CodeEntityConfiguration<MsSubject>
    {
        public override void Configure(EntityTypeBuilder<MsSubject> builder)
        {
            builder.Property(x => x.SubjectID)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsSubject_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdDepartment)
                .HasConstraintName("FK_MsSubject_MsDepartment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
