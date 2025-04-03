using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageEndpoint
    {
        private const string _route = "message";
        private const string _tag = "Message";

        [FunctionName(nameof(MessageEndpoint.GetMessageCategories))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMessageCategoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMessageCategoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMessageCategoryResult))]
        public Task<IActionResult> GetMessageCategories(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/categories")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageCategoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.GetMessageTypes))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMessageTypeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMessageTypeResult))]
        public Task<IActionResult> GetMessageTypes(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/types")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.GetMessages))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMessageRequest.IsRead), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetMessageRequest.Type), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetMessageRequest.MessageFolder), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMessageRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMessageRequest.FeedbackType), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetMessageRequest.IsDraft), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetMessageRequest.IsApproval), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetMessageRequest.ApprovalStatus), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetMessageRequest.IdMessageCategory), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMessageResult))]
        public Task<IActionResult> GetMessages(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.AddMessageDraft))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMessageRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMessageDraft(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageAddMessageHandler>();
            return handler.Execute(req, cancellationToken, true, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.EditMessage))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMessageRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> EditMessage(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageEditMessageHandler>();
            return handler.Execute(req, cancellationToken, true, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.UpdateIsRead))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetMessageIsReadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateIsRead(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/read")] HttpRequest req,
            CancellationToken cancellationToken
        )
        {
            var handler = req.HttpContext.RequestServices.GetService<SetMessageIsReadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.UploadFileAttachment))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UploadFileAttachment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "document/file/upload")] HttpRequest req,
            ExecutionContext context,
            System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageUploadFileAttachmentHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(ExecutionContext).WithValue(context));
        }

        [FunctionName(nameof(MessageEndpoint.DeleteFileAttachment))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetFileRequest.IdMessageAttachment), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetFileRequest.FileName), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteFileAttachment(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "document/file/delete")] HttpRequest req,
            ExecutionContext context,
            System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageDeleteFileAttachmentHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(ExecutionContext).WithValue(context));
        }

        [FunctionName(nameof(MessageEndpoint.GetMessageOption))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMessageOptionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMessageOptionRequest.Code), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMessageOptionResult))]
        public Task<IActionResult> GetMessageOption(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/option")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageOptionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.RestoreMessage))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MessageRestoreRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> RestoreMessage(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/restore")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageRestoreHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.EmptyMessageTrash))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MessageRestoreRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> EmptyMessageTrash(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/empty-trash")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageEmptyTrashHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.AddMessageReply))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMessageReplyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMessageReply(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reply")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            [Queue("notification-fd-communication")] ICollector<string> collector1,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageAddMessageReplyHandler>();
            return handler.Execute(req, cancellationToken, true, nameof(collector).WithValue(collector), nameof(collector1).WithValue(collector1));
        }

        [FunctionName(nameof(MessageEndpoint.GetMessageDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMessageDetailRequest.IdMessage), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMessageDetailRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMessageDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMessageDetailResult))]
        public Task<IActionResult> GetMessageDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.DeleteMessage))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMessageRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMessage(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete")] HttpRequest req,
            ExecutionContext context,
            System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageDeleteMessageHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(ExecutionContext).WithValue(context));
        }

        [FunctionName(nameof(MessageEndpoint.GetUserBySpecificFilter))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserBySpecificFilterRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserBySpecificFilterResult))]
        public Task<IActionResult> GetUserBySpecificFilter(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user-by-specific-filter")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageGetUserBySpecificFilterHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.SetMessageApprovalStatus))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetMessageApprovalStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetMessageApprovalStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/approval")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetMessageApprovalStatusHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.GetUserByExcel))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        public Task<IActionResult> GetUserByExcel(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/user/import")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserByExcelHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.GetListSentTo))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSentToRequest.IdMessage), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListSentToRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListSentToRequest.Search), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListSentToResult))]
        public Task<IActionResult> GetListSentTo(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-sent-to")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListSentToHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.TestNotification))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListSentToResult))]
        public Task<IActionResult> TestNotification(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/test-notification")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TestNotificationHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.SetMessageEndConversation))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetMessageEndConversationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetMessageEndConversation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/end-conversation")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetMessageEndConversationHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.UnsendMessage))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UnsendMessageRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UnsendMessage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/unsend")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MessageUnsendMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #region Download File Message
        [FunctionName(nameof(MessageEndpoint.DownloadFileMessage))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadFileMessageRequest.FileName), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadFileMessageRequest.OriginFileName), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadFileMessage(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-file")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadFileMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        [FunctionName(nameof(MessageEndpoint.GetMailingLists))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Mailing List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGroupMailingListRequest.GroupName), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGroupMailingListRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGroupMailingListRequest.IsCreateMessage), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGroupMailingListResult))]
        public Task<IActionResult> GetMailingLists(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mailing-list")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MailingListHandler>();
            return handler.Execute(req, cancellationToken);
        }

        //[FunctionName(nameof(MessageEndpoint.GetMailingListDetail))]
        //[OpenApiOperation(tags: _tag, Summary = "Get MailingList Detail")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiParameter("id", Required = true)]
        //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGroupMailingListDetailsResult))]
        //public Task<IActionResult> GetMailingListDetail(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mailing-list/detail")] HttpRequest req,
        //    string id,
        //    CancellationToken cancellationToken)
        //{
        //    var handler = req.HttpContext.RequestServices.GetService<MailingListDetailHandler>();
        //    return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        //}

        [FunctionName(nameof(MessageEndpoint.GetMailingListDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get MailingList Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMailingListDetailRequest.IdMailingList), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMailingListDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGroupMailingListDetailsResult))]
        public Task<IActionResult> GetMailingListDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mailing-list/detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MailingListDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.AddMailingList))]
        [OpenApiOperation(tags: _tag, Summary = "Add MailingList")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddGroupMailingListRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMailingList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/mailing-list")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MailingListHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.UpdateMailingList))]
        [OpenApiOperation(tags: _tag, Summary = "Update MailingList")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateGroupMailingListRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMailingList(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/mailing-list")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MailingListHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.DeleteMailingList))]
        [OpenApiOperation(tags: _tag, Summary = "Delete MailingList")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMailingList(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/mailing-list")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MailingListHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MessageEndpoint.DeleteMemberMailingList))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Member Mailing List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMemberMailingListRequest))]
        //[OpenApiParameter(nameof(DeleteMemberMailingListRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        //[OpenApiParameter(nameof(DeleteMemberMailingListRequest.IdGroupMailingList), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMemberMailingList(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/mailing-list/member")] HttpRequest req,
            [Queue("notification-mss-communication")] ICollector<string> collector,
            ExecutionContext context,
            System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteMemberMailingListHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.QueueMessages))]
        [OpenApiOperation(tags: _tag, Summary = "Queue Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(QueueMessagesRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> QueueMessages(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/queue")] HttpRequest req,
            [Queue("message-queue")] ICollector<string> collector,
            System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<QueueMessagesHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MessageEndpoint.QueueNotificationMessage))]
        [OpenApiOperation(tags: _tag, Summary = "Queue Notification Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(QueueMessagesRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> QueueNotificationMessage(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/notification-queue")] HttpRequest req,
            [Queue("notification-message")] ICollector<string> collector,
            System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<QueueNotificationHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
    }
}
