using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessage : AuditEntity, IUserEntity
    {
        public string IdSender { get; set; }
        public UserMessageType Type { get; set; }
        /// <summary>
        /// Set value to true if sender wants to be changed to school name (school for sender id)
        /// </summary>
        public bool IsSetSenderAsSchool { get; set; }
        public string Subject { get; set; }
        /// <summary>
        /// This column will be filled when type is Annoucment Or Private
        /// </summary>
        public string IdMessageCategory { get; set; }
        /// <summary>
        /// This column will be filled when type is Feedback
        /// </summary>
        public string IdFeedbackType { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// This column will be filled when type is Announcment
        /// </summary>
        public DateTime? PublishStartDate { get; set; }
        /// <summary>
        /// This column will be filled when type is Announcment
        /// </summary>
        public DateTime? PublishEndDate { get; set; }
        /// <summary>
        /// This column will be set True when type is Private
        /// </summary>
        public bool IsAllowReply { get; set; }
        /// <summary>
        /// This column will be filled when type is Private
        /// </summary>
        public DateTime? ReplyStartDate { get; set; }
        /// <summary>
        /// This column will be filled when type is Private
        /// </summary>
        public DateTime? ReplyEndDate { get; set; }
        public StatusMessage Status { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public bool IsDraft { get; set; }
        public bool IsSendEmail { get; set; }
        public bool EndConversation { get; set; }
        public string ReasonEndConversation { get; set; }
        public string ParentMessageId { get; set; }
        public bool IsUnsend { get; set; }
        public StatusMessage StatusUnsend { get; set; }
        public bool IsEdit { get; set; }
        public virtual TrMessage MessageParent { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsFeedbackType FeedbackType { get; set; }
        public virtual MsMessageCategory MessageCategory { get; set; }
        public virtual ICollection<TrMessageAttachment> MessageAttachments { get; set; }
        public virtual ICollection<TrMessageRecepient> MessageRecepients { get; set; }
        public virtual ICollection<TrMessage> MessageReply { get; set; }
        public virtual ICollection<TrMessageApproval> MessageApprovals { get; set; }
        public virtual ICollection<TrMessageGroupMember> MessageGroupMembers { get; set; }
        public virtual ICollection<TrMessageFor> MessageFors { get; set; }
    }

    internal class TrMessageConfiguration : AuditEntityConfiguration<TrMessage>
    {
        public override void Configure(EntityTypeBuilder<TrMessage> builder)
        {
            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            builder.HasOne(x => x.User)
                 .WithMany(x => x.Messages)
                 .HasForeignKey(fk => fk.IdSender)
                 .HasConstraintName("FK_TrMessage_MsUser")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.FeedbackType)
                .WithMany(x => x.Messages)
                .HasForeignKey(fk => fk.IdFeedbackType)
                .HasConstraintName("FK_TrMessage_MsFeedbackType")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MessageCategory)
               .WithMany(x => x.Messages)
               .HasForeignKey(fk => fk.IdMessageCategory)
               .HasConstraintName("FK_TrMessage_MsMessageCategory")
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Subject).HasMaxLength(50);

            builder.Property(p => p.Content).HasColumnType("TEXT");

            builder.HasOne(x => x.MessageParent)
             .WithMany(x => x.MessageReply)
             .HasForeignKey(fk => fk.ParentMessageId)
             .HasConstraintName("FK_TrMessage_TrMessage")
             .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.StatusUnsend)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.ReasonEndConversation).HasColumnType("TEXT");

            base.Configure(builder);
        }
    }
}
