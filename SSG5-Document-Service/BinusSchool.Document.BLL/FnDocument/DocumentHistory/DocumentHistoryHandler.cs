using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Api.Workflow.FnWorkflow;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentHistory;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentHistory
{
    public class DocumentHistoryHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMetadata _metadataService;
        private readonly IRole _roleService;
        private readonly IUser _userService;
        private readonly IApproval _approvalServices;
        private readonly IApprovalHistory _approvalHistoryServices;

        public DocumentHistoryHandler(IDocumentDbContext dbContext,
            IMetadata metadataService,
            IApproval approvalServices,
            IApprovalHistory approvalHistoryServices,
            IUser userService,
            IRole roleService)
        {
            _dbContext = dbContext;
            _metadataService = metadataService;
            _userService = userService;
            _approvalServices = approvalServices;
            _approvalHistoryServices = approvalHistoryServices;
            _roleService = roleService;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = _dbContext.Entity<HMsFormDocChange>()
                .Include(p => p.DocChangeNotes)
                .Include(p => p.FormDoc).ThenInclude(p => p.Form)
                .Include(p => p.DocChangeApprovals)
                .Where(p => p.Id == id);

            if (query.FirstOrDefault() is null)
                throw new NotFoundException(null, Localizer);

            var detail = await query
                .Select(x => new GetDocumentHistoryDetailResult
                {
                    Id = x.Id,
                    UserNameChangeBy = x.IdUserExecutor,
                    JsonDocumentValueNew = x.JsonDocumentValueNew,
                    JsonFormElementOld = x.JsonFormElementOld,
                    JsonDocumentValueOld = x.JsonDocumentValueOld,
                    JsonFormElementNew = x.JsonFormElementNew,
                    TypeChange = x.Status.AsString(Localizer),
                    AcademicYears = x.FormDoc.Form.IdAcademicYear,
                    Term = x.FormDoc.Form.IdPeriod,
                    Semester = x.FormDoc.Form.Semester,
                    Subject = x.FormDoc.Form.IdSubject,
                    Level = x.FormDoc.Form.IdLevel,
                    Grade = x.FormDoc.Form.IdGrade,
                    CreatedDate = x.DateIn,
                    CreatedBy = x.UserIn,
                    NoteFiledChange = x.DocChangeNotes.Select(p => new NoteFiledChange
                    {
                        FieldName = p.FieldName,
                        Note = p.Note,
                    }),
                })
                .FirstOrDefaultAsync(CancellationToken);
            
            var formMetadata = await _metadataService.GetMetadata(new GetMetadataRequest
            {
                Acadyears = JsonUtil.CollectIdFromJson(detail.AcademicYears),
                Levels = JsonUtil.CollectIdFromJson(detail.Level),
                Grades = JsonUtil.CollectIdFromJson(detail.Grade),
                Terms = JsonUtil.CollectIdFromJson(detail.Term),
                Subjects = JsonUtil.CollectIdFromJson(detail.Subject)
            });

            if (!formMetadata.IsSuccess)
                throw new BadRequestException(formMetadata.Message);

            detail.Semester = detail.Semester;
            detail.AcademicYears = string.Join("/", formMetadata.Payload.Acadyears.Select(x => x.Description));
            detail.Level = string.Join("/", formMetadata.Payload.Levels.Select(x => x.Description));
            detail.Grade = string.Join("/", formMetadata.Payload.Grades.Select(x => x.Description));
            detail.Term = string.Join("/", formMetadata.Payload.Terms.Select(x => x.Description));
            detail.Subject = string.Join("/", formMetadata.Payload.Subjects.Select(x => x.Description));

            var user = await _userService.GetUserDetail(detail.CreatedBy);
            detail.CreatedBy = user.Payload?.DisplayName;

            var userExecute = await _userService.GetUserDetail(detail.UserNameChangeBy);
            detail.UserNameChangeBy = userExecute.Payload?.DisplayName;

            var idWorkflow = query.FirstOrDefault().FormDoc.Form.IdApprovalType;
            if (idWorkflow != null)
            {
                var getApproval = await _approvalServices.GetListApprovalStateByWorkflowWithout(new GetListApprovalStateWithWorkflowRequest
                {
                    IdWorkflow = idWorkflow,
                    WithoutState = StateTypeConstant.End,
                });

                if (!getApproval.IsSuccess)
                    throw new BadRequestException(getApproval.Message);

                detail.HistoryApproval = getApproval.Payload
                    .OrderBy(p => p.StateNumber)
                    .GroupJoin(query.FirstOrDefault().DocChangeApprovals,
                        state => state.Id,
                        his => his.IdState,
                        (state, his) => new { state, his })
                    .SelectMany(
                        x => x.his.DefaultIfEmpty(),
                        (x, y) => new { x.state, history = y })
                    .Select(x => new HistoryApproval
                    {
                        Name = x.history?.Action == ApprovalStatus.Approved ? "Approved" :
                                   x.history?.Action == ApprovalStatus.Reject ? "Rejected" : "Review",
                        Action = x.history?.Action.AsString(Localizer),
                        DateAction = x.history?.Action == ApprovalStatus.NeedRevision ||
                                     x.history?.Action == ApprovalStatus.Approved ||
                                     x.history?.Action == ApprovalStatus.Reject ? x.history?.ActionDate : new DateTime?(),
                        RoleAction = x.state.IdRole,
                    });

                detail.Revision = query.FirstOrDefault().DocChangeApprovals
                    .OrderByDescending(p => p.DateIn)
                    .Select(p => new RevisionComment
                    {
                        Comment = p.Comment,
                        DateAction = p.ActionDate,
                        UserAction = p.IdUserActionBy
                    })
                    .FirstOrDefault();

                user = await _userService.GetUserDetail(detail.Revision.UserAction);
                detail.Revision.UserAction = user.Payload?.DisplayName;
            }

            return Request.CreateApiResult2(detail as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.GetParams<GetDocumentHistoryRequest>();

            var query = _dbContext.Entity<HMsFormDocChange>()
                .Where(p => p.IdFormDoc == param.IdDocument)
                .SearchByDynamic(param)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Status.AsString(Localizer)))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                if (string.IsNullOrEmpty(param.OrderBy))
                    query = query.OrderByDescending(x => x.DateIn).ThenByDescending(p => p.Order);

                var results = await query.SetPagination(param)
                    .Select(x => new GetDocumentHistoryResult
                    {
                        Id = x.Id,
                        TypeHistory = x.Status.AsString(Localizer),
                        //CreateBy=$"{x.TypeChange} by {x.User.Username} ({string.Join(",", x.User.SchoolUsers.SelectMany(p => p.SchoolUserRoles.Select(o => o.SchoolRole.Role.Description)))} - )",
                        CreateBy = x.Status.AsString(Localizer) + " by " + x.IdUserExecutor,
                        CreateDate = x.DateIn,
                    })
                    .ToListAsync(CancellationToken);

                if (results.Any())
                {
                    var idUsersOrRole = results.Select(x => x.CreateBy.Split(" by ").Last()).Distinct();

                    var users = await _userService.GetUsers(new GetUserRequest
                    {
                        Ids = idUsersOrRole,
                        GetAll = true
                    });

                    var role = await _roleService.GetRoles(new GetRoleRequest()
                    {
                        IdSchool = param.IdSchool,
                        GetAll = true
                    });

                    if (role.IsSuccess)
                    {
                        foreach (var item in role.Payload)
                        {
                            results.Where(x => x.CreateBy.Contains(item.Id))
                                .ToList()
                                .ForEach(x => x.CreateBy.Replace(item.Id, item.Description));
                        }
                    }


                    if (users.IsSuccess)
                    {
                        foreach (var user in users.Payload)
                        {
                            results.Where(x => x.CreateBy.Contains(user.Id))
                                .ToList()
                                .ForEach(x => x.CreateBy.Replace(user.Id, user.Description));
                        }
                    }
                }

                items = results;
            }

            var count = param.CanCountWithoutFetchDb(query.Count())
                ? query.Count()
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }


        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
