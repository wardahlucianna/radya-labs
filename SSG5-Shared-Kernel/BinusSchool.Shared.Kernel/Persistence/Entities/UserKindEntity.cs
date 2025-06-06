﻿using BinusSchool.Common.Model.Enums;
using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    /// <summary>
    /// Entity base for user profile
    /// </summary>
    public abstract class UserKindEntity : AuditEntity
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
    }

    public class UserKindEntityConfiguration<T> : AuditEntityConfiguration<T> where T : UserKindEntity, IEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.FirstName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.MiddleName)
                .HasMaxLength(250);

            builder.Property(x => x.LastName)
                .HasMaxLength(250);


            builder.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(6)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
