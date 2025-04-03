using System;
using System.Collections.Generic;
using System.Linq;
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
using BinusSchool.Data.Model.Document.FnDocument.Template;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Document.FnDocument.Template
{
    public class TemplateHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMetadata _metadataService;
        private readonly IRole _roleService;
        private readonly IUser _userService;
        private readonly IWorkflow _workFlowService;

        public TemplateHandler(IDocumentDbContext dbContext, IMetadata metadataService, IRole roleService, IUser userService,
            IWorkflow workFlowService)
        {
            _dbContext = dbContext;
            _metadataService = metadataService;
            _roleService = roleService;
            _userService = userService;
            _workFlowService = workFlowService;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var template = await _dbContext.Entity<MsForm>()
                .Include(x => x.FormAssignmentRole).ThenInclude(y => y.FormAssignmentUsers)
                .Select(x => new GetTemplateDetailResult
                {
                    Id = x.Id,
                    Grade = x.IdGrade,
                    Acadyear = x.IdAcademicYear,
                    IsApprovalForm = x.IsApprovalForm,
                    Approval = x.IdApprovalType != null
                        ? new ApprovalVm { IdApprovalType = x.IdApprovalType }
                        : null,
                    IdSchool = x.IdSchool,
                    SchoolDocumentCategory = new CodeWithIdVm(x.IdDocCategory, x.DocCategory.Code, x.DocCategory.Description),
                    JsonFormElement = x.JsonFormElement,
                    JsonSchema = x.JsonSchema,
                    Level = x.IdLevel,
                    IsMultipleForm = x.IsMultipleForm,
                    Subject = x.IdSubject,
                    Term = x.IdPeriod,
                    FormBuilderAssignmentRole = x.FormAssignmentRole != null
                        ? new AssignmentRoleResult
                        {
                            Id = x.FormAssignmentRole.IdRole,
                            FormBuilderAssignmentUsers = x.FormAssignmentRole.FormAssignmentUsers.Select(y => new ItemValueVm(y.IdUser))
                        }
                        : null,
                    Semester = x.Semester,
                })
               .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            if (template is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Template"], "Id", id));

            var formMetadata = await _metadataService.GetMetadata(new GetMetadataRequest
            {
                Acadyears = JsonUtil.CollectIdFromJson(template.Acadyear),
                Levels = JsonUtil.CollectIdFromJson(template.Level),
                Grades = JsonUtil.CollectIdFromJson(template.Grade),
                Subjects = JsonUtil.CollectIdFromJson(template.Subject),
                Terms = JsonUtil.CollectIdFromJson(template.Term)
            });

            if (!formMetadata.IsSuccess)
                throw new BadRequestException(formMetadata.Message);

            template.Acadyear = formMetadata.Payload.Acadyears.Any()
                ? JsonConvert.SerializeObject(formMetadata.Payload.Acadyears.Select(x => new { x.Id, text = x.Description }))
                : "[]";
            template.Term = formMetadata.Payload.Terms.Any()
                ? JsonConvert.SerializeObject(formMetadata.Payload.Terms.Select(x => new { x.Id, text = x.Description }))
                : "[]";
            template.Level = formMetadata.Payload.Levels.Any()
                ? JsonConvert.SerializeObject(formMetadata.Payload.Levels.Select(x => new { x.Id, text = x.Description }))
                : "[]";
            template.Grade = formMetadata.Payload.Grades.Any()
                ? JsonConvert.SerializeObject(formMetadata.Payload.Grades.Select(x => new { x.Id, text = x.Description }))
                : "[]";
            template.Subject = formMetadata.Payload.Subjects.Any()
                ? JsonConvert.SerializeObject(formMetadata.Payload.Subjects.Select(x => new { x.Id, text = x.Description }))
                : "[]";
            template.Term = formMetadata.Payload.Terms.Any()
                ? JsonConvert.SerializeObject(formMetadata.Payload.Terms.Select(x => new { x.Id, text = x.Description }))
                : "[]";

            if (template.FormBuilderAssignmentRole != null)
            {
                var role = await _roleService.GetRoleGroupById(new IdCollection
                {
                    Ids = new[] { template.FormBuilderAssignmentRole.Id }
                });
                template.FormBuilderAssignmentRole.Description = role.Payload.FirstOrDefault()?.Description;

                foreach (var item in template.FormBuilderAssignmentRole.FormBuilderAssignmentUsers)
                {
                    var user = await _userService.GetUserDetail(item.Id);
                    item.Description = user.Payload?.DisplayName;
                }
            }

            if (template.Approval != null)
            {
                // TODO: Invoke workflow service here
                var workFlow = await _workFlowService.GetDetailWorkflow(template.Approval.IdApprovalType);
                template.Approval.ApprovalName = workFlow.Payload?.Description;
            }

            return Request.CreateApiResult2(template as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetTemplateRequest>(nameof(GetTemplateRequest.IdSchoolDocumentCategory));
            var predicate = PredicateBuilder.Create<MsForm>(x => x.IdDocCategory == param.IdSchoolDocumentCategory);
            if (!string.IsNullOrEmpty(param.Acadyear))
                predicate = predicate.And(x => x.IdAcademicYear.Contains(param.Acadyear));
            if (!string.IsNullOrEmpty(param.Level))
                predicate = predicate.And(x => x.IdLevel.Contains(param.Level));

            var query = _dbContext.Entity<MsForm>()
                .Include(x => x.DocCategory).ThenInclude(x => x.DocType)
                .Where(predicate)
                .SearchByDynamic(param)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;
            KeyValuePair<string, object> category = default;
            KeyValuePair<string, object> type = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.IdAcademicYear))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                var results = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                var categoryVm = default(CodeWithIdVm);
                var typeVm = default(CodeWithIdVm);

                if (results.Count == 0)
                {
                    var docCategory = await _dbContext.Entity<MsDocCategory>()
                        .Include(x => x.DocType)
                        .FirstOrDefaultAsync(x => x.Id == param.IdSchoolDocumentCategory);
                    if (docCategory is null)
                        throw new BadRequestException(string.Join(Localizer["ExNotExist"], Localizer["DocumentCategory"], "Id", param.IdSchoolDocumentCategory));

                    categoryVm = new CodeWithIdVm(docCategory.Id, docCategory.Code, docCategory.Description);
                    typeVm = new CodeWithIdVm(docCategory.IdDocType, docCategory.DocType.Code, docCategory.DocType.Description);
                }
                else
                {
                    var result = results.First();
                    categoryVm = new CodeWithIdVm
                    {
                        Id = result.IdDocCategory,
                        Code = result.DocCategory.Code,
                        Description = result.DocCategory.Description
                    };
                    typeVm = new CodeWithIdVm
                    {
                        Id = result.DocCategory.IdDocType,
                        Code = result.DocCategory.DocType.Code,
                        Description = result.DocCategory.DocType.Description
                    };
                }

                category = KeyValuePair.Create(nameof(category), categoryVm as object);
                type = KeyValuePair.Create(nameof(type), typeVm as object);

                var formMetadata = await _metadataService.GetMetadata(new GetMetadataRequest
                {
                    Acadyears = JsonUtil.CollectIdFromJson(results.Select(x => x.IdAcademicYear).ToArray()),
                    Levels = JsonUtil.CollectIdFromJson(results.Select(x => x.IdLevel).ToArray()),
                    Grades = JsonUtil.CollectIdFromJson(results.Select(x => x.IdGrade).ToArray()),
                    Subjects = JsonUtil.CollectIdFromJson(results.Select(x => x.IdSubject).ToArray())
                });

                if (formMetadata.IsSuccess)
                {
                    items = results
                        .Select(x => new GetTemplateResult
                        {
                            Id = x.Id,
                            Acadyear = string.Join("/", formMetadata.Payload.Acadyears.Where(y => JsonUtil.CollectIdFromJson(x.IdAcademicYear).Contains(y.Id)).Select(x => x.Description)),
                            Level = string.Join("/", formMetadata.Payload.Levels.Where(y => JsonUtil.CollectIdFromJson(x.IdLevel).Contains(y.Id)).Select(x => x.Description)),
                            Grade = string.Join("/", formMetadata.Payload.Grades.Where(y => JsonUtil.CollectIdFromJson(x.IdGrade).Contains(y.Id)).Select(x => x.Description)),
                            Subject = string.Join("/", formMetadata.Payload.Subjects.Where(y => JsonUtil.CollectIdFromJson(x.IdSubject).Contains(y.Id)).Select(x => x.Description))
                        })
                        .ToList();
                }
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddProperty(category, type));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddTemplateRequest, AddTemplateValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (body.IsMultipleForm)
            {
                foreach (var acadYear in body.AcademicYear)
                {
                    foreach (var term in body.Term)
                    {
                        foreach (var level in body.Level)
                        {
                            foreach (var grade in body.Grade)
                            {
                                foreach (var subject in body.Subject)
                                {
                                    var template = new MsForm
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdDocCategory = body.IdSchoolDocumentCategory,
                                        IdApprovalType = !string.IsNullOrWhiteSpace(body.IdApprovalType) ? body.IdApprovalType : string.Empty,                                      
                                        IdSchool = body.IdSchool,
                                        JsonFormElement = body.JsonFormElement,
                                        JsonSchema = body.JsonSchema,
                                        IdAcademicYear = acadYear,
                                        IdPeriod = term,
                                        Semester = body.Semester,
                                        IdLevel = level,
                                        IdGrade = grade,
                                        IdSubject = subject,
                                        IsApprovalForm = body.IsApprovalForm,
                                        IsMultipleForm = body.IsMultipleForm,
                                        UserIn = AuthInfo.UserId
                                    };

                                    _dbContext.Entity<MsForm>().Add(template);

                                    CreateAssignmentAndDocument(template.Id, body.UserAndRole.IdRole, body.UserAndRole.IdUser, body.JsonFormElement);
                                }
                            }
                        }
                    }
                }

                await AddOrUpdateSelectManual(body.JsonFormElement);
            }
            else
            {
                var template = new MsForm
                {
                    Id = Guid.NewGuid().ToString(),
                    IdDocCategory = body.IdSchoolDocumentCategory,
                    IdApprovalType = !string.IsNullOrWhiteSpace(body.IdApprovalType) ? body.IdApprovalType : string.Empty,
                    IdSchool = body.IdSchool,
                    JsonFormElement = body.JsonFormElement,
                    JsonSchema = body.JsonSchema,
                    IdAcademicYear = JsonConvert.SerializeObject(body.AcademicYear),
                    IdPeriod = JsonConvert.SerializeObject(body.Term),
                    Semester = body.Semester,
                    IdLevel = JsonConvert.SerializeObject(body.Level),
                    IdGrade = JsonConvert.SerializeObject(body.Grade),
                    IdSubject = JsonConvert.SerializeObject(body.Subject),
                    IsApprovalForm = body.IsApprovalForm,
                    IsMultipleForm = body.IsMultipleForm,
                    UserIn = AuthInfo.UserId
                };

                _dbContext.Entity<MsForm>().Add(template);
                await AddOrUpdateSelectManual(body.JsonFormElement);
                CreateAssignmentAndDocument(template.Id, body.UserAndRole.IdRole, body.UserAndRole.IdUser, body.JsonFormElement);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateTemplateRequest, UpdateTemplateValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existTemplate = await _dbContext.Entity<MsForm>()
                .Include(p => p.FormDocs)
                .Include(p => p.FormAssignmentRole).ThenInclude(p => p.FormAssignmentUsers)
                .FirstOrDefaultAsync(x => x.Id == body.Id);

            if (existTemplate == null)
                throw new BadRequestException(string.Join(Localizer["ExNotExist"], Localizer["Template"], "Id", body.Id));

            existTemplate.IdDocCategory = body.IdSchoolDocumentCategory;
            existTemplate.IdApprovalType = !string.IsNullOrWhiteSpace(body.IdApprovalType) ? body.IdApprovalType : null;
            existTemplate.IdSchool = body.IdSchool;
            existTemplate.JsonFormElement = body.JsonFormElement;
            existTemplate.JsonSchema = body.JsonSchema;
            existTemplate.IdAcademicYear = JsonConvert.SerializeObject(body.AcademicYear);
            existTemplate.IdPeriod = JsonConvert.SerializeObject(body.Term);
            existTemplate.Semester = body.Semester;
            existTemplate.IdLevel = JsonConvert.SerializeObject(body.Level);
            existTemplate.IdGrade = JsonConvert.SerializeObject(body.Grade);
            existTemplate.IdSubject = JsonConvert.SerializeObject(body.Subject);
            existTemplate.IsApprovalForm = body.IsApprovalForm;
            existTemplate.IsMultipleForm = body.IsMultipleForm;
            existTemplate.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsForm>().Update(existTemplate);
            await AddOrUpdateSelectManual(body.JsonFormElement);

            if (existTemplate.FormAssignmentRole != null)
            {
                if (existTemplate.FormAssignmentRole.FormAssignmentUsers.Count > 0)
                {
                    _dbContext.Entity<MsFormAssignmentUser>().RemoveRange(existTemplate.FormAssignmentRole.FormAssignmentUsers);
                }

                _dbContext.Entity<MsFormAssignmentRole>().Remove(existTemplate.FormAssignmentRole);
            }

            CreateAssignment(existTemplate.Id, body.UserAndRole.IdRole, body.UserAndRole.IdUser);

            foreach (var document in existTemplate.FormDocs)
            {
                document.JsonFormElement = body.JsonFormElement;
                document.UserUp = AuthInfo.UserId;
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        #region Private Method

        private void CreateAssignment(string templateId, string idRole, IReadOnlyList<string> idUsers)
        {
            var assignmentRole = new MsFormAssignmentRole
            {
                Id = templateId,
                IdRole = idRole,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsFormAssignmentRole>().Add(assignmentRole);

            if (idUsers.Count > 0)
            {
                foreach (var userItem in idUsers)
                {
                    var assigUser = new MsFormAssignmentUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUser = userItem,
                        IdFormAssignmentRole = assignmentRole.Id,
                        UserIn = AuthInfo.UserId
                    };

                    _dbContext.Entity<MsFormAssignmentUser>().Add(assigUser);
                }
            }
        }

        private void CreateAssignmentAndDocument(string templateId, string idRole, IReadOnlyList<string> idUsers, string jsonFormElement)
        {
            CreateAssignment(templateId, idRole, idUsers);

            var document = new MsFormDoc
            {
                Id = Guid.NewGuid().ToString(),
                IdForm = templateId,
                Status = ApprovalStatus.Draft,
                JsonFormElement = jsonFormElement,
                JsonDocumentValue = string.Empty
            };

            _dbContext.Entity<MsFormDoc>().Add(document);
        }

        private async Task AddOrUpdateSelectManual(string jsonElement)
        {
            var sections = JArray.Parse(jsonElement);
            if (sections.Count != 0)
            {
                var elements = sections.SelectMany(x => x["elements"].ToArray());
                if (elements.Any())
                {
                    var manuals = elements.Where(x => x["type"].Value<string>() == "select-manual");

                    if (manuals.Any())
                    {
                        var keys = manuals.Select(x => x["attribute"]["elementKey"].Value<string>()).Distinct();

                        // throw when any duplicate keys
                        // var anyDuplicateKeys = keys.GroupBy(x => x).Any(x => x.Count() > 1);
                        // if (anyDuplicateKeys)
                        //     throw new BadRequestException(string.Format(_localizer["ExNotUnique"], "elementKey"));

                        var existManuals = await _dbContext.Entity<MsParamGlobal>()
                            .Where(x => keys.Any(y => y == x.Key))
                            .ToListAsync();

                        // update existing select-manual
                        if (existManuals.Count != 0)
                        {
                            // only can add options here
                            // foreach (var man in existManuals)
                            // {
                            //     var currentManual = manuals.First(x => x["attribute"]["elementKey"].Value<string>() == man.Key);

                            //     var newOptKeys = currentManual.SelectMany(x => x["Option"]["options"].ToArray().Select(y => y["id"]));
                            //     var existOptKeys = JArray.Parse(man.Value).Select(x => x["id"]);
                            //     newOptKeys = newOptKeys.Except(existOptKeys);

                            //     var newOptions = currentManual.Where(x => newOptKeys.Contains(x["Option"]["options"].ToArray().Select(x => x["id"])));
                            // }

                            foreach (var man in existManuals)
                            {
                                man.Value = manuals.First(x => x["attribute"]["elementKey"].Value<string>() == man.Key)["Option"]["Options"].ToString();
                                man.UserUp = AuthInfo.UserId;
                            }

                            _dbContext.Entity<MsParamGlobal>().UpdateRange(existManuals);

                            var newManuals = keys.Except(existManuals.Select(x => x.Key));
                            manuals = manuals.Where(x => newManuals.Any(y => y == x["attribute"]["elementKey"].Value<string>()));
                        }

                        // add new select-manual if any
                        var paramGlobals = manuals.Select(x => new MsParamGlobal
                        {
                            Id = Guid.NewGuid().ToString(),
                            Key = x["attribute"]["elementKey"].Value<string>(),
                            Value = x["Option"]["Options"].ToString()
                        });

                        if (paramGlobals.Any())
                            _dbContext.Entity<MsParamGlobal>().AddRange(paramGlobals);
                    }
                }
            }
        }

        #endregion
    }
}
