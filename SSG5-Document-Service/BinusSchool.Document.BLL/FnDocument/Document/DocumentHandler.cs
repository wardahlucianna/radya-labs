using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
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
using BinusSchool.Data.Model.Document.FnDocument.Document;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Document.FnDocument.Document.Validator;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace BinusSchool.Document.FnDocument.Document
{
    public class DocumentHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMetadata _metadataService;
        private readonly IRole _roleService;
        private readonly IUser _userService;
        private readonly IApproval _approvalServices;
        private readonly IApprovalHistory _approvalHistoryServices;

        public DocumentHandler(IDocumentDbContext dbContext,
            IMetadata metadataService,
            IRole roleService,
            IUser userService,
            IApprovalHistory approvalHistoryServices,
            IApproval approvalServices)
        {
            _dbContext = dbContext;
            _metadataService = metadataService;
            _roleService = roleService;
            _userService = userService;
            _approvalHistoryServices = approvalHistoryServices;
            _approvalServices = approvalServices;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = _dbContext.Entity<MsFormDoc>()
                .Include(p => p.Form).ThenInclude(p => p.DocCategory)
                .Include(p => p.FormDocChanges)
                .Include(p => p.FormDocApprovals)
                .Include(p => p.FormDocChanges).ThenInclude(p => p.DocChangeApprovals)
                .Where(p => p.Id == id);

            var document = await query
                .Select(p => new GetDocumentDetailResult
                {
                    Id = p.Id,
                    IdFormBuilderTemplate = p.Form.Id,
                    JsonDocumentValue = p.JsonDocumentValue,
                    JsonFormElement = p.JsonFormElement,
                    AcademicYears = p.Form.IdAcademicYear,
                    Subject = p.Form.IdSubject,
                    Level = p.Form.IdLevel,
                    Grade = p.Form.IdGrade,
                    Semester = p.Form.Semester,
                    Term = p.Form.IdPeriod,
                    Status = p.Status,
                    CreateBy = p.UserIn,
                    CreateDate = p.DateIn,
                    ForNextRole = p.Status == ApprovalStatus.NeedApproval && p.FormDocChanges.Count > 0 && p.FormDocChanges.First().DocChangeApprovals.Count > 0
                            ? p.FormDocChanges.OrderByDescending(y => y.DateIn).First().DocChangeApprovals.OrderByDescending(y => y.ActionDate).First().IdForRoleActionNext
                            : p.Status == ApprovalStatus.Approved && p.FormDocChanges.Count > 0 && p.FormDocChanges.First().DocChangeApprovals.Count > 0
                            ? p.FormDocChanges.OrderByDescending(y => y.DateIn).First().DocChangeApprovals.OrderByDescending(y => y.ActionDate).First().IdRoleActionBy : null
                })
                .FirstOrDefaultAsync();

            if (document is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Document"], "Id", id));

            var formMetadata = await _metadataService.GetMetadata(new GetMetadataRequest
            {
                Acadyears = JsonUtil.CollectIdFromJson(document.AcademicYears),
                Levels = JsonUtil.CollectIdFromJson(document.Level),
                Grades = JsonUtil.CollectIdFromJson(document.Grade),
                Terms = JsonUtil.CollectIdFromJson(document.Term),
                Subjects = JsonUtil.CollectIdFromJson(document.Subject)
            });

            if (!formMetadata.IsSuccess)
                throw new BadRequestException(formMetadata.Message);

            var user = await _userService.GetUserDetail(document.CreateBy);

            if (document.ForNextRole != null)
            {
                var role = await _roleService.GetRoleGroupById(new IdCollection
                {
                    Ids = new[] { document.ForNextRole }
                });
                document.Status += $" {role.Payload.FirstOrDefault()?.Description}";
            }

            document.CreateBy = user.Payload?.DisplayName;
            document.AcademicYears = string.Join("/", formMetadata.Payload.Acadyears.Select(x => x.Description));
            document.Level = string.Join("/", formMetadata.Payload.Levels.Select(x => x.Description));
            document.Grade = string.Join("/", formMetadata.Payload.Grades.Select(x => x.Description));
            document.Term = string.Join("/", formMetadata.Payload.Terms.Select(x => x.Description));
            document.Subject = string.Join("/", formMetadata.Payload.Subjects.Select(x => x.Description));

            //get task unutk user trigger approve atau reject
            var getApprovalHistory = await _approvalHistoryServices.GetApprovalHistoryByUser(new GetApprovalHistoryByUserRequest
            {
                IdDocument = document.Id,
                IdUserAction = AuthInfo.UserId,
                Action = ApprovalStatus.Draft,
            });

            // if (!getApprovalHistory.IsSuccess)
            //     throw new BadRequestException(getApprovalHistory.Message);

            document.ApprovalTask = new ApprovalTask
            {
                IdForUser = getApprovalHistory.Payload?.IdForUser,
                IdFromState = getApprovalHistory.Payload?.IdFromState,
                IdApprovalTask = getApprovalHistory.Payload?.IdApprovalTask,
                IdDocument = getApprovalHistory.Payload?.IdDocument,
                Action = getApprovalHistory.Payload?.Action,
            };

            var idWorkflow = query.FirstOrDefault().Form.IdApprovalType;
            if (idWorkflow != null)
            {
                var getDocumentChange = query.FirstOrDefault().FormDocChanges.OrderByDescending(p => p.DateIn).FirstOrDefault();
                if (getDocumentChange != null)
                {
                    var getApproval = await _approvalServices.GetListApprovalStateByWorkflowWithout(new GetListApprovalStateWithWorkflowRequest
                    {
                        IdWorkflow = idWorkflow,
                        WithoutState = StateTypeConstant.End,
                    });

                    if (!getApproval.IsSuccess)
                        throw new BadRequestException(getApproval.Message);

                    document.HistoryApprovals = getApproval.Payload
                        .OrderBy(p => p.StateNumber)
                        .GroupJoin(getDocumentChange.DocChangeApprovals,
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
                            Action = x.state.IdRole == x.history?.IdRoleActionBy ? x.history?.Action.ToString() : "",
                            DateAction = x.state.IdRole == x.history?.IdRoleActionBy ? x.history?.ActionDate : default(DateTime?),
                            RoleAction = x.state.IdRole,
                        });

                    var revision = getDocumentChange.DocChangeApprovals.Where(p => p.Action != ApprovalStatus.Submit)
                        .OrderByDescending(x => x.DateIn)
                        .FirstOrDefault();

                    if (revision != null)
                    {
                        user = await _userService.GetUserDetail(revision.IdUserActionBy);
                        document.Revision = new RevisionComment
                        {
                            Comment = revision.Comment,
                            DateAction = revision.ActionDate,
                            UserAction = user.Payload?.DisplayName
                        };
                    }
                }
            }

            return Request.CreateApiResult2(document as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.GetParams<GetDocumentRequest>();
            var columns = new[] { "acadyear", "term", "semester", "level", "grade", "subject", "status" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Form.Acadyear" },
                { columns[1], "Form.Term" },
                { columns[2], "Form.Semester" },
                { columns[3], "Form.Level" },
                { columns[4], "Form.Grade" },
                { columns[5], "Form.Subject" },
            };

            var predicate = PredicateBuilder.Create<MsFormDoc>(x => x.Form.IdDocCategory.Contains(param.IdCategory));

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.Form.IdAcademicYear.Contains(param.IdAcadyear));
            if (!string.IsNullOrEmpty(param.IdTerm))
                predicate = predicate.And(x => x.Form.IdPeriod.Contains(param.IdTerm));
            if (!string.IsNullOrEmpty(param.IdSemester))
                predicate = predicate.And(x => x.Form.Semester.Contains(param.IdSemester));
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Form.IdLevel.Contains(param.IdLevel));
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Form.IdGrade.Contains(param.IdGrade));
            if (!string.IsNullOrEmpty(param.IdSubject))
                predicate = predicate.And(x => x.Form.IdSubject.Contains(param.IdSubject));
            if (!string.IsNullOrEmpty(param.IdApproval))
                predicate = predicate.And(x => x.Form.IdApprovalType.Contains(param.IdApproval));

            var isAdmin = AuthInfo.Roles.Any(x => x.Id == "ADM" || x.Id == "SAM");
            var isApprover = AuthInfo.Roles.Any(x => x.Id == "HOD" || x.Id == "SH");
            if (!isAdmin)
            {
                var roles = AuthInfo.Roles.Select(x => x.Id).ToList();
                if (!isApprover)
                {
                    predicate = predicate.And(x => roles.Contains(x.Form.FormAssignmentRole.IdRole));
                    predicate = predicate.And(x => x.Form.FormAssignmentRole.FormAssignmentUsers.Any() ? x.Form.FormAssignmentRole.FormAssignmentUsers.Any(y => y.IdUser == AuthInfo.UserId) : true);
                }
                else
                {
                    // for HOD and SH get only need approval for him && approved
                    predicate = predicate.And(x => x.Status == ApprovalStatus.Approved || x.Status == ApprovalStatus.Reject
                                                   || (x.Status == ApprovalStatus.NeedApproval && x.FormDocChanges.Any() && x.FormDocChanges.First().DocChangeApprovals.Any() ?
                                                   roles.Contains(x.FormDocChanges.OrderByDescending(y => y.DateIn).First().DocChangeApprovals.OrderByDescending(y => y.ActionDate).First().IdForRoleActionNext) :
                                                   true));
                }
            }

            var query = _dbContext.Entity<MsFormDoc>()
                .Include(p => p.Form)
                .Include(p => p.Form).ThenInclude(p => p.FormAssignmentRole).ThenInclude(p => p.FormAssignmentUsers)
                .Include(p => p.FormDocChanges).ThenInclude(x => x.DocChangeApprovals)
                .Where(predicate)
                .SearchByDynamic(param);

            query = param.OrderBy == "status"
                ? param.OrderType == OrderType.Asc ?
                        //query.OrderBy(x => GetStatusOrdering(x.Status,isAdmin,isApprover)) :
                        //query.OrderByDescending(x => GetStatusOrdering(x.Status, isAdmin, isApprover))
                        query.OrderBy(x => x.Status) :
                        query.OrderByDescending(x => x.Status)
                : query.OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .Select(x => new GetDocumentResult
                    {
                        Id = x.Id,
                        Acadyear = x.Form.IdAcademicYear,
                        Level = x.Form.IdLevel,
                        Grade = x.Form.IdGrade,
                        Subject = x.Form.IdSubject
                    })
                    .ToListAsync(CancellationToken);
                var formMetadata = await _metadataService.GetMetadata(new GetMetadataRequest
                {
                    Acadyears = JsonUtil.CollectIdFromJson(result.Select(x => x.Acadyear).ToArray()),
                    Levels = JsonUtil.CollectIdFromJson(result.Select(x => x.Level).ToArray()),
                    Grades = JsonUtil.CollectIdFromJson(result.Select(x => x.Grade).ToArray()),
                    Subjects = JsonUtil.CollectIdFromJson(result.Select(x => x.Subject).ToArray())
                });
                if (!formMetadata.IsSuccess)
                    throw new BadRequestException(formMetadata.Message);

                result.ForEach(x =>
                {
                    x.Acadyear = string.Join("/", formMetadata.Payload.Acadyears.Where(y => JsonUtil.CollectIdFromJson(x.Acadyear).Contains(y.Id)).Select(x => x.Description));
                    x.Level = string.Join("/", formMetadata.Payload.Levels.Where(y => JsonUtil.CollectIdFromJson(x.Level).Contains(y.Id)).Select(x => x.Description));
                    x.Grade = string.Join("/", formMetadata.Payload.Grades.Where(y => JsonUtil.CollectIdFromJson(x.Grade).Contains(y.Id)).Select(x => x.Description));
                    x.Subject = string.Join("/", formMetadata.Payload.Subjects.Where(y => JsonUtil.CollectIdFromJson(x.Subject).Contains(y.Id)).Select(x => x.Description));
                });

                items = result.Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = string.Format("{0} - {1} ({2} - {3})", x.Acadyear, x.Level, x.Grade, x.Subject)
                }).ToList();
            }
            else
            {
                var results = await query
                    .SetPagination(param)
                    .Select(x => new GetDocumentResult
                    {
                        Id = x.Id,
                        Semester = x.Form.Semester,
                        Grade = x.Form.IdGrade,
                        Subject = x.Form.IdSubject,
                        Term = x.Form.IdPeriod,
                        Status = x.Status.AsString(Localizer),
                        ForNextRole = x.Status == ApprovalStatus.NeedApproval && x.FormDocChanges.Count > 0 && x.FormDocChanges.First().DocChangeApprovals.Count > 0
                            ? x.FormDocChanges.OrderByDescending(y => y.DateIn).First().DocChangeApprovals.OrderByDescending(y => y.ActionDate).First().IdForRoleActionNext
                            : x.Status == ApprovalStatus.Approved && x.FormDocChanges.Count > 0 && x.FormDocChanges.First().DocChangeApprovals.Count > 0
                            ? x.FormDocChanges.OrderByDescending(y => y.DateIn).First().DocChangeApprovals.OrderByDescending(y => y.ActionDate).First().IdRoleActionBy : null,
                        Acadyear = x.Form.IdAcademicYear,
                        Level = x.Form.IdLevel,
                    })
                    .ToListAsync(CancellationToken);



                var formMetadata = await _metadataService.GetMetadata(new GetMetadataRequest
                {
                    Acadyears = JsonUtil.CollectIdFromJson(results.Select(x => x.Acadyear).ToArray()),
                    Levels = JsonUtil.CollectIdFromJson(results.Select(x => x.Level).ToArray()),
                    Grades = JsonUtil.CollectIdFromJson(results.Select(x => x.Grade).ToArray()),
                    Subjects = JsonUtil.CollectIdFromJson(results.Select(x => x.Subject).ToArray()),
                    Terms = JsonUtil.CollectIdFromJson(results.Select(x => x.Term).ToArray())
                });

                if (!formMetadata.IsSuccess)
                    throw new BadRequestException(formMetadata.Message);

                results.ForEach(x =>
                {
                    x.Acadyear = string.Join("/", formMetadata.Payload.Acadyears.Where(y => JsonUtil.CollectIdFromJson(x.Acadyear).Contains(y.Id)).Select(x => x.Description));
                    x.Level = string.Join("/", formMetadata.Payload.Levels.Where(y => JsonUtil.CollectIdFromJson(x.Level).Contains(y.Id)).Select(x => x.Description));
                    x.Grade = string.Join("/", formMetadata.Payload.Grades.Where(y => JsonUtil.CollectIdFromJson(x.Grade).Contains(y.Id)).Select(x => x.Description));
                    x.Subject = string.Join("/", formMetadata.Payload.Subjects.Where(y => JsonUtil.CollectIdFromJson(x.Subject).Contains(y.Id)).Select(x => x.Description));
                    x.Term = string.Join("/", formMetadata.Payload.Terms.Where(y => JsonUtil.CollectIdFromJson(x.Term).Contains(y.Id)).Select(x => x.Description));
                    if (x.ForNextRole != null)
                    {
                        x.Status = string.Format("{0} ({1})", x.Status, x.ForNextRole);
                    }
                });

                items = results;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateDocumentRequest, UpdateDocumentValidator>();
            var document = await _dbContext.Entity<MsFormDoc>()
                .Include(x => x.Form).ThenInclude(p => p.FormAssignmentRole).ThenInclude(p => p.FormAssignmentUsers)
                .FirstOrDefaultAsync(x => body.Id == x.Id, CancellationToken);

            if (document != null)
            {
                if (document.Form.FormAssignmentRole.FormAssignmentUsers.Count > 0)
                {
                    if (!document.Form.FormAssignmentRole.FormAssignmentUsers.Any(p => p.IdUser == AuthInfo.UserId))
                    {
                        throw new BadRequestException("This document not Assign for you");
                    }
                }
                else
                {
                    if (!AuthInfo.Roles.Any(p => p.Id == document.Form.FormAssignmentRole.IdRole))
                    {
                        throw new BadRequestException("This document not Assign for you");
                    }
                }

                if (!string.IsNullOrEmpty(document.Form.JsonSchema))
                {
                    var changeHistoryStatus = ApprovalStatus.Submit;
                    var schemaJson = document.Form.JsonSchema;
                    var schema = JSchema.Parse(schemaJson);
                    var jsonValue = JObject.Parse(body.JsonDocumentValue);
                    var valid = jsonValue.IsValid(schema, out IList<ValidationError> Errors);

                    if (valid)
                    {
                        Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                        //add to change history
                        if (!string.IsNullOrWhiteSpace(document.UserIn))
                        {
                            document.UserIn = AuthInfo.UserId;
                            changeHistoryStatus = ApprovalStatus.Submit;
                        }
                        else
                        {
                            document.UserUp = AuthInfo.UserId;
                            changeHistoryStatus = ApprovalStatus.NeedRevision;
                        }


                        if (document.Status == ApprovalStatus.Draft || document.Status == ApprovalStatus.Reject || document.Status == ApprovalStatus.NeedRevision)
                        {
                            if (!body.IsSaveAsDraft)
                            {
                                var addToChangeHistory = Guid.NewGuid().ToString();


                                if (document.Form.IsApprovalForm && !string.IsNullOrWhiteSpace(document.Form.IdApprovalType))
                                {
                                    //masuk mekanisme approval send notifikasi dan submit ke approval task
                                    //1.get approval dari approval transition
                                    var getApproval = await _approvalServices.GetApprovalStateByWorkflow(new GetApprovalStateByWorkflowRequest
                                    {
                                        IdApprovalWorkflow = document.Form.IdApprovalType,
                                        StateType = StateTypeConstant.Start
                                    });

                                    if (!getApproval.IsSuccess)
                                        throw new BadRequestException(getApproval.Message);

                                    if (getApproval.Payload != null)
                                    {
                                        //2.get user berdasarkan role id dari approval managment
                                        //* belum di filter berdasarjkan school id
                                        var getUser = await _userService.GetUserByRoleAndSchoolWithoutValidateStaff(new GetUserBySchoolAndRoleRequest
                                        {
                                            IdRole = getApproval.Payload.IdRole,
                                            IdSchool = document.Form.IdSchool,
                                        });

                                        if (!getUser.IsSuccess)
                                            throw new BadRequestException(getUser.Message);

                                        if (getUser.Payload.Count() == 0)
                                            throw new BadRequestException("User for Approval Not found");

                                        //3.add Task unutk user trigger user bisa approve atau rejct berdasarkan history ini
                                        if (getUser.Payload.Any())
                                        {
                                            foreach (var item in getUser.Payload)
                                            {
                                                var addHistoryApproval = await _approvalHistoryServices.AddApprovalHistory(new AddApprovalHistoryRequest
                                                {
                                                    IdUserAction = item.Id,
                                                    IdFormState = getApproval.Payload.Id,
                                                    IdDocument = document.Id,
                                                    Action = ApprovalStatus.Draft,
                                                });

                                                if (!addHistoryApproval.IsSuccess)
                                                    throw new BadRequestException(addHistoryApproval.Message);
                                            }

                                            //4.add to send notification email and signal r
                                            // TODO: send notification via email and/or signalR
                                        }

                                        //ubah status berdasarkan flllow approval
                                        document.Status = ApprovalStatus.NeedApproval;

                                        var hisApprovals = new List<HMsFormDocChangeApproval>();
                                        //add to history approval submit
                                        hisApprovals.Add(new HMsFormDocChangeApproval
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdState = getApproval.Payload.Id,
                                            IdUserActionBy = AuthInfo.UserId,
                                            // TODO: get id default yg di pakai user ketika login 
                                            IdRoleActionBy = document.Form.FormAssignmentRole.IdRole,
                                            IdFormDocument = document.Id,
                                            Action = changeHistoryStatus,
                                            Comment = "",
                                            ActionDate = DateTimeUtil.ServerTime,
                                            IdForRoleActionNext = getApproval?.Payload.IdRole,
                                            UserIn = AuthInfo.UserId,
                                            IdsFormDocChange = addToChangeHistory
                                        });


                                        //add history chnage to status ned approval 
                                        var addToChangeNeedApprovalHistory = Guid.NewGuid().ToString();
                                        AddChnageHistory(body, document, document.Status, addToChangeNeedApprovalHistory, true, getApproval?.Payload.IdRole, 1);

                                        //add to history approval nedd approval
                                        hisApprovals.Add(new HMsFormDocChangeApproval
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdState = getApproval.Payload.Id,
                                            IdUserActionBy = AuthInfo.UserId,
                                            // TODO: get id default yg di pakai user ketika login 
                                            IdRoleActionBy = document.Form.FormAssignmentRole.IdRole,
                                            IdFormDocument = document.Id,
                                            Action = document.Status,
                                            Comment = "",
                                            ActionDate = DateTimeUtil.ServerTime,
                                            IdForRoleActionNext = getApproval?.Payload.IdRole,
                                            UserIn = AuthInfo.UserId,
                                            IdsFormDocChange = addToChangeNeedApprovalHistory
                                        });

                                        _dbContext.Entity<HMsFormDocChangeApproval>().AddRange(hisApprovals);
                                    }
                                }

                                //save to change history submit atau revisi
                                AddChnageHistory(body, document, changeHistoryStatus, addToChangeHistory, false, null, 0);

                            }
                        }
                        document.JsonDocumentValue = body.JsonDocumentValue;
                        document.JsonFormElement = body.JsonFormElement;
                        document.IdForm = body.IdFormBuilderTemplate;


                        _dbContext.Entity<MsFormDoc>().Update(document);
                        await _dbContext.SaveChangesAsync(CancellationToken);

                        await Transaction.CommitAsync(CancellationToken);

                        return Request.CreateApiResult2();
                    }
                    else
                    {
                        var listError = Errors.Select(x => x.Message).ToList();
                        throw new Exception(string.Join(",", listError.ToArray()));
                    }
                }
                else
                {
                    throw new Exception("JSON schema Is Not Found");
                }
            }
            else
            {
                throw new Exception("Document Not Found");
            }
        }

        /// <summary>
        /// to add history document 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="document"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private void AddChnageHistory(UpdateDocumentRequest model, MsFormDoc document, ApprovalStatus status, string Id, bool isAddRole, string toNextRole, int order)
        {
            var addToChangeHistory = new HMsFormDocChange
            {
                Id = Id,
                IdFormDoc = document.Id,
                JsonDocumentValueOld = document.JsonDocumentValue,
                JsonFormElementOld = document.JsonFormElement,
                JsonDocumentValueNew = model.JsonDocumentValue,
                JsonFormElementNew = model.JsonFormElement,
                Status = status,
                Order = order,
                UserIn = AuthInfo.UserId,
                IdUserExecutor = isAddRole ? toNextRole : AuthInfo.UserId,
            };

            //change note history submit atau revisi
            if (model.HistoryChangeFieldNote.Any())
            {
                foreach (var item in model.HistoryChangeFieldNote)
                {
                    var historyNote = new HMsFormDocChangeNote
                    {
                        Id = Guid.NewGuid().ToString(),
                        FieldName = item.FieldName,
                        Note = item.Note,
                        IdsFormDocChange = addToChangeHistory.Id,
                        UserIn = AuthInfo.UserId
                    };
                    _dbContext.Entity<HMsFormDocChangeNote>().Add(historyNote);
                }
            }

            _dbContext.Entity<HMsFormDocChange>().Add(addToChangeHistory);

        }

        private int GetStatusOrdering(ApprovalStatus status, bool isAdmin, bool isApprover)
        {
            if (!isAdmin)
            {
                if (!isApprover)
                {
                    //for teacher
                    switch (status)
                    {
                        case ApprovalStatus.Draft:
                            return 1;
                        case ApprovalStatus.Submit:
                            return 2;
                        case ApprovalStatus.NeedRevision:
                            return 3;
                        case ApprovalStatus.NeedApproval:
                            return 4;
                        case ApprovalStatus.Approved:
                            return 5;
                        case ApprovalStatus.Reject:
                            return 6;
                    }
                }
                else
                {
                    // for HOD and SH
                    switch (status)
                    {

                        case ApprovalStatus.NeedApproval:
                            return 1;
                        case ApprovalStatus.NeedRevision:
                            return 2;
                        case ApprovalStatus.Approved:
                            return 3;
                        case ApprovalStatus.Reject:
                            return 4;
                        case ApprovalStatus.Draft:
                            return 5;
                        case ApprovalStatus.Submit:
                            return 6;
                    }
                }
            }

            return (int)status;
        }
    }
}
