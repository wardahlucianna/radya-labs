﻿using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsDepartment : CodeEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public DepartmentType Type { get; set; }
        
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
        public virtual ICollection<TrEventIntendedForDepartment> TrEventIntendedForDepartments { get; set; }
        public virtual ICollection<HTrEventIntendedForDepartment> HistoryEventIntendedForDepartments { get; set; }
        public virtual ICollection<MsDepartmentLevel> DepartmentLevels { get; set; }
    }

    internal class MsDepartmentConfiguration : CodeEntityConfiguration<MsDepartment>
    {
        public override void Configure(EntityTypeBuilder<MsDepartment> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Departments)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsDepartment_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
