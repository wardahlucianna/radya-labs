using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForDepartment : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEventIntendedFor { get; set; }
        public string IdDepartment { get; set; }

        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsDepartment Department { get; set; }
    }

    internal class HTrEventIntendedForDepartmentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForDepartment>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForDepartment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForDepartment).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
                .WithMany(x => x.EventIntendedForDepartments)
                .HasForeignKey(fk => fk.IdEventIntendedFor)
                .HasConstraintName("FK_HTrEventIntendedForDepartment_HTrEventIntendedFor")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            builder.HasOne(x => x.Department)
               .WithMany(x => x.HistoryEventIntendedForDepartments)
               .HasForeignKey(fk => fk.IdDepartment)
               .HasConstraintName("FK_HTrEventIntendedForDepartment_MsDepartment")
               .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
