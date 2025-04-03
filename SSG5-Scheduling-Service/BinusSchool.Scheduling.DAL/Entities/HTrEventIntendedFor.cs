using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedFor : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        ///<summary>Value for Intended For <br/>
        /// 1. All <br/>
        /// 2. Staff <br/>
        /// 3. Teacher <br/>
        /// 4. Student <br/>
        /// 5. Parent
        /// </summary>
        public string IntendedFor { get; set; }
        ///<summary>Value for Option <br/>
        /// 1. All <br/>
        ///     1.1 No Option <br/>
        /// 2. Staff <br/>
        ///     2.1 All <br/>
        ///     2.2 Department <br/>
        ///     2.3 Position <br/>
        ///     2.4 Personal Event <br/>
        /// 3. Teacher <br/>
        ///     3.1 All <br/>
        ///     3.2 Department <br/>
        ///     3.3 Position <br/>
        ///     3.4 Personal Event <br/>
        /// 4. Student <br/>
        ///     4.1 All <br/>
        ///     4.2 Level <br/>
        ///     4.3 Grade <br/>
        ///     4.4 Personal Event <br/>
        /// 5. Parent <br/>
        ///     5.1 All <br/>
        ///     5.2 Personal Event <br/>
        /// </summary>
        public string Option { get; set; }
        ///<summary>Value True If Intended For Student Otherwise False<br/>
        /// </summary>
        public bool SendNotificationToLevelHead { get; set; }
        ///<summary>Value True If Intended For Student Otherwise False<br/>
        /// </summary>
        public bool NeedParentPermission { get; set; }
        ///<summary>Value True If Intended For Student Otherwise False<br/>
        /// </summary>
        public string NoteToParent { get; set; }

        public virtual HTrEvent Event { get; set; }

        public virtual ICollection<HTrEventIntendedForDepartment> EventIntendedForDepartments { get; set; }
        public virtual ICollection<HTrEventIntendedForPosition> EventIntendedForPositions { get; set; }
        public virtual ICollection<HTrEventIntendedForPersonal> EventIntendedForPersonals { get; set; }
        public virtual ICollection<HTrEventIntendedForLevelStudent> EventIntendedForLevelStudents { get; set; }
        public virtual ICollection<HTrEventIntendedForGradeStudent> EventIntendedForGradeStudents { get; set; }
        public virtual ICollection<HTrEventIntendedForPersonalStudent> EventIntendedForPersonalStudents { get; set; }
        public virtual ICollection<HTrEventIntendedForPersonalParent> EventIntendedForPersonalParents { get; set; }
        public virtual ICollection<HTrEventIntendedForGradeParent> EventIntendedForGradeParents { get; set; }
        public virtual ICollection<HTrEventIntendedForAtdStudent> EventIntendedForAttendanceStudents { get; set; }
    }

    internal class HTrEventIntendedForConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedFor>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedFor> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedFor).Name)
                .HasMaxLength(36);

            builder.Property(x => x.IntendedFor)
                .HasMaxLength(7)
                .IsRequired();

            builder.Property(x => x.Option)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.NoteToParent)
               .HasMaxLength(450);

            builder.HasOne(x => x.Event)
               .WithMany(x => x.EventIntendedFor)
               .HasForeignKey(fk => fk.IdEvent)
               .HasConstraintName("FK_HTrEventIntendedFor_HTrEvent")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
