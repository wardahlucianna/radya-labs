using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventIntendedFor : AuditEntity, IAttendanceEntity
    {
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

        public virtual TrEvent Event { get; set; }
        public virtual ICollection<TrEventIntendedForLevelStudent> EventIntendedForLevelStudents { get; set; }
        public virtual ICollection<TrEventIntendedForGradeStudent> EventIntendedForGradeStudents { get; set; }
        public virtual ICollection<TrEventIntendedForPersonalStudent> EventIntendedForPersonalStudents { get; set; }
        public virtual ICollection<TrEventIntendedForAttendanceStudent> EventIntendedForAttendanceStudents { get; set; }
    }

    internal class TrEventIntendedForConfiguration : AuditEntityConfiguration<TrEventIntendedFor>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedFor> builder)
        {
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
               .HasConstraintName("FK_TrEventIntendedFor_TrEvent")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
