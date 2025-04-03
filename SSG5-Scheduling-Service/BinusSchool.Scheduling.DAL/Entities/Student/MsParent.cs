using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsParent : UserKindStudentParentEntity, ISchedulingEntity
    {
        public string POB { get; set; }
        //public DateTime? DOB { get; set; }
        //public Int16 AliveStatus { get; set; }
        //public Int16 BinusianStatus { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string WorkEmailAddress { get; set; }
        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual ICollection<TrEventIntendedForPersonalParent> EventIntendedForPersonalParents { get; set; }
        public virtual ICollection<HTrEventIntendedForPersonalParent> HistoryEventIntendedForPersonalParents { get; set; }
    }

    internal class MsParentConfiguration : UserKindStudentParentEntityConfiguration<MsParent>
    {
        public override void Configure(EntityTypeBuilder<MsParent> builder)
        {
            builder.Property(x => x.POB)
                   .HasMaxLength(100);

            //builder.Property(x => x.DOB)
            //    .HasColumnType(typeName: "datetime2")
            //    .IsRequired();

            builder.Property(x => x.ResidencePhoneNumber)
                .HasColumnType("VARCHAR(25)")
                .HasMaxLength(25);

            builder.Property(x => x.MobilePhoneNumber1)
                .HasColumnType("VARCHAR(25)")
                .HasMaxLength(25);

            builder.Property(x => x.MobilePhoneNumber2)
                .HasColumnType("VARCHAR(25)")
                .HasMaxLength(25);

            builder.Property(x => x.MobilePhoneNumber3)
                .HasColumnType("VARCHAR(25)")
                .HasMaxLength(25);

            builder.Property(x => x.PersonalEmailAddress)
                .HasColumnType("VARCHAR(50)")
                .HasMaxLength(50);

            builder.Property(x => x.WorkEmailAddress)
                .HasColumnType("VARCHAR(50)")
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
