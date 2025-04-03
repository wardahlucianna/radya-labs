using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.File;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BinusSchool.Data.Api.User.FnCommunication
{
    public interface IMessage : IFnCommunication
    {
        [Get("/message/categories")]
        Task<ApiErrorResult<IEnumerable<GetMessageCategoryResult>>> GetMessageCategories(GetMessageCategoryRequest request);

        [Get("/message/types")]
        Task<ApiErrorResult<IEnumerable<GetMessageTypeResult>>> GetMessageTypes(GetMessageTypeRequest request);

        [Post("/message")]
        Task<ApiErrorResult> AddMessage([Body] AddMessageRequest body);

        [Put("/message")]
        Task<ApiErrorResult> EditMessage([Body] AddMessageRequest body);

        [Get("/message")]
        Task<ApiErrorResult<IEnumerable<GetMessageResult>>> GetMessage(GetMessageRequest request);

        [Put("/message/read")]
        Task<ApiErrorResult> SetMessageIsRead([Body] SetMessageIsReadRequest body);

        [Multipart]
        [Post("/message/file/upload")]
        Task<ApiErrorResult<UploadFileResult>> UploadFileAttachment(StreamPart stream, string fileName);

        [Multipart]
        [Post("/message/file/delete")]
        Task<ApiErrorResult<UploadFileResult>> DeleteFileAttachment(StreamPart stream, string fileName);

        [Get("/message/option")]
        Task<ApiErrorResult<IEnumerable<GetMessageOptionResult>>> GetMessageOption(GetMessageOptionRequest request);

        [Put("/message/restore")]
        Task<ApiErrorResult> RestoreMessage([Body] MessageRestoreRequest body);

        [Delete("/message/empty-trash")]
        Task<ApiErrorResult> EmptyTrashMessage(MessageEmptyTrashRequest request);

        [Post("/message/reply")]
        Task<ApiErrorResult> AddMessageReply([Body] AddMessageReplyRequest body);

        [Get("/message/detail")]
        Task<ApiErrorResult<GetMessageDetailResult>> GetMessageDetail(GetMessageDetailRequest request);

        [Delete("/message/delete")]
        Task<ApiErrorResult> DeleteMessage([Body] DeleteMessageRequest body);

        [Get("/user-by-specific-filter")]
        Task<ApiErrorResult<IEnumerable<GetUserBySpecificFilterResult>>> GetUserBySpecificFilter(GetUserBySpecificFilterRequest request);

        [Post("/message/approval")]
        Task<ApiErrorResult> SetMessageApprovalStatus([Body] SetMessageApprovalStatusRequest body);

        [Multipart]
        [Post("/message/user/import")]
        Task<ApiErrorResult<GetUserByExcelResult>> GetUserByExcel(StreamPart stream);

        [Get("/message/list-sent-to")]
        Task<ApiErrorResult<GetListSentToResult>> GetListSentTo(GetListSentToRequest request);

        [Post("/message/end-conversation")]
        Task<ApiErrorResult> SetMessageEndConversation([Body] SetMessageEndConversationRequest body);

        [Get("/message/download-file")]
        Task<HttpResponseMessage> DownloadFileMessage(DownloadFileMessageRequest param);

        [Post("/message/unsend")]
        Task<ApiErrorResult> UnsendMessage([Body] UnsendMessageRequest body);

        [Get("/message/mailing-list")]
        Task<ApiErrorResult<IEnumerable<GetGroupMailingListResult>>> GetMailingLists(GetGroupMailingListRequest param);

        [Get("/message/mailing-list/detail")]
        Task<ApiErrorResult<GetGroupMailingListDetailsResult>> GetMailingListDetail(GetMailingListDetailRequest param);

        [Post("/message/mailing-list")]
        Task<ApiErrorResult> AddMailingList([Body] AddGroupMailingListRequest body);

        [Put("/message/mailing-list")]
        Task<ApiErrorResult> UpdateMailingList([Body] UpdateGroupMailingListRequest body);

        [Delete("/message/mailing-list")]
        Task<ApiErrorResult> DeleteMailingList([Body] IEnumerable<string> body);

        [Delete("/message/mailing-list/member")]
        Task<ApiErrorResult> DeleteMemberMailingList([Body] DeleteMemberMailingListRequest body);

        [Get("/message/queue")]
        Task<ApiErrorResult> QueueMessages(QueueMessagesRequest request);

        [Get("/message/notification-queue")]
        Task<ApiErrorResult> QueueNotificationMessage(QueueMessagesRequest request);
    }
}
