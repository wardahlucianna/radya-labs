using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using Newtonsoft.Json;
using BinusSchool.Common.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.PublishSurvey.Validator;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.School.FnSchool.SurveySummary;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class PublishSurveyHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IPublishSurvey _servicePublishService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRolePosition _serviceRolePosition;

        public PublishSurveyHandler(ISchoolDbContext dbContext, IPublishSurvey servicePublishService, IMachineDateTime datetime, IServiceProvider serviceProvider, IRolePosition serviceRolePosition)
        {
            _dbContext = dbContext;
            _servicePublishService = servicePublishService;
            _datetime = datetime;
            _serviceProvider = serviceProvider;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var getTemplateSurvey = await _dbContext.Entity<MsSurveyTemplate>()
              .Where(x => ids.Contains(x.Id))
              .ToListAsync(CancellationToken);

            getTemplateSurvey.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsSurveyTemplate>().UpdateRange(getTemplateSurvey);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var item = await _dbContext.Entity<TrPublishSurvey>()
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyDepartments).ThenInclude(e => e.Department)
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyPositions).ThenInclude(e => e.TeacherPosition)
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyUsers).ThenInclude(e => e.User)
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyUsers).ThenInclude(e => e.TeacherPosition)
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Level)
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Grade)
               .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Homeroom)
                    .ThenInclude(e => e.Grade)
                .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Homeroom)
                    .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
               .Include(e => e.PublishSurveyMappings)
               .Include(e => e.SurveyTemplate)
               .Include(e => e.PublishSurveyLink)
               .Where(e => e.Id == id)
               .Select(x => new DetailPublishSurveyResult
               {
                   Id = x.Id,
                   IdSurveyTemplate = x.IdSurveyTemplate,
                   IdSurveyTemplateChild = x.IdSurveyTemplateChild,
                   IdSurveyTemplateLink = x.IdSurveyTemplateLink,
                   IdSurveyTemplateChildLink = x.IdSurveyTemplateChildLink,
                   AcademicYear = new ItemValueVm
                   {
                       Id = x.AcademicYear.Id,
                       Description = x.AcademicYear.Description,
                   },
                   Semester = x.Semester,
                   SurveyName = x.Title,
                   SurveyTemplateTypeEnum = x.SurveyTemplate.Type,
                   SurveyTemplateType = x.SurveyTemplate.Type.GetDescription(),
                   SurveyTypeEnum = x.SurveyType,
                   SurveyType = x.SurveyType.GetDescription(),
                   Description = x.Description,
                   SurveyTemplate = new ItemValueVm
                   {
                       Id = x.SurveyTemplate.Id,
                       Description = x.SurveyTemplate.Title,
                   },
                   PublishSurveyLink = new ItemValueVm
                   {
                       Id = x.PublishSurveyLink.Id,
                       Description = x.PublishSurveyLink.Title,
                   },
                   StartDate = x.StartDate,
                   EndDate = x.EndDate,
                   IsGrapicExtender = x.IsGrapicExtender,
                   IsMandatory = x.IsMandatory,
                   IsEntryOneTime = x.IsEntryOneTime,
                   SubmissionOptionEnum = x.SubmissionOption,
                   SubmissionOption = x.SubmissionOption.ToString() == "SubmitReviewPerChild"
                                            ? "Submit Review Per Child"
                                                : x.SubmissionOption.ToString() == "SubmitReviewPerFamily"
                                                    ? "Submit Review Per Family"
                                                        : x.SubmissionOption.ToString() == "Submit1ReviewPerChildOr1ReviewPerFamily"
                                                            ? "Submit 1 Review Per Child or 1 Review Per Family" : "",
                   AboveSubmitText = x.AboveSubmitText,
                   ThankYouMessage = x.ThankYouMessage,
                   AfterSurveyCloseText = x.AfterSurveyCloseText,
                   Respondent = x.PublishSurveyRespondents.Select(e => new DetailPublishSurveyRespondent
                   {
                       RoleEnum = e.Role,
                       Role = e.Role.GetDescription(),
                       OptionEnum = e.Option,
                       Option = e.Option.ToString() == "SpecificUser" ? "Specific User" : e.Option.ToString(),
                       Position = e.PublishSurveyPositions.Select(f => new ItemValueVm
                       {
                           Id = f.IdTeacherPosition,
                           Description = f.TeacherPosition.Description,
                       }).ToList(),
                       User = e.PublishSurveyUsers.Select(f => new DetailPublishSurveyRespondentUser
                       {
                           Id = f.IdUser,
                           Description = f.User.DisplayName,
                           TeacherPosition = new ItemValueVm
                           {
                               Id = f.TeacherPosition.Id,
                               Description = f.TeacherPosition.Description
                           }
                       }).ToList(),
                       Department = e.PublishSurveyDepartments.Select(f => new ItemValueVm
                       {
                           Id = f.IdDepartement,
                           Description = f.Department.Description,
                       }).ToList(),
                       Grade = e.PublishSurveyGrades.Select(f => new DetailPublishSurveyRespondentGrade
                       {
                           Level = new ItemValueVm
                           {
                               Id = f.Level.Id,
                               Description = f.Level.Description
                           },
                           Grade = new ItemValueVm
                           {
                               Id = f.Grade.Id,
                               Description = f.Grade.Description
                           },
                           Semester = f.Homeroom.Semester,
                           Homeroom = new ItemValueVm
                           {
                               Id = f.Homeroom.Id,
                               Description = f.Homeroom.Grade.Code + f.Homeroom.GradePathwayClassroom.Classroom.Code
                           }
                       }).ToList(),
                   }).ToList()
               }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetPublishSurveyRequest>();
            var predicate = PredicateBuilder.Create<TrPublishSurvey>(x => x.IsActive);
            string[] _columns = { "AcademicYear", "Semeter", "SurveyType", "SurveyName", "StartDate", "EndDate", "Status" };

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.SurveyType.ToString()))
                predicate = predicate.And(x => x.SurveyType == param.SurveyType);

            if (!string.IsNullOrEmpty(param.StartDate.ToString()) && !string.IsNullOrEmpty(param.EndDate.ToString()))
                predicate = predicate.And(x => x.StartDate.Date >= Convert.ToDateTime(param.StartDate).Date && x.EndDate.Date <= Convert.ToDateTime(param.EndDate).Date);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Title.Contains(param.Search));

            var queryPublishSurvey = _dbContext.Entity<TrPublishSurvey>()
                .Include(e => e.AcademicYear)
                .Include(e => e.SurveyTemplate)
                .Where(predicate)
                .Select(x => new
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear.Description,
                    Semeter = x.Semester,
                    SurveyType = x.SurveyType.GetDescription(),
                    SurveyName = x.Title,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IdTemplateChild = x.SurveyTemplate.IdTemplateChild,
                    IdTemplate = x.SurveyTemplate.Id,
                    SurveyTemplateType = x.SurveyTemplate.Type,
                    Status = GetStatusSurvey(_datetime.ServerTime.Date, x.Status, x.StartDate.Date, x.EndDate.Date),
                    IdAcademicYear = x.AcademicYear.Id
                });

            var listPublishSurvey = await queryPublishSurvey.ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(param.Status))
                listPublishSurvey = listPublishSurvey.Where(x => x.Status == param.Status.ToString()).ToList();

            var query = listPublishSurvey.Distinct();
            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;

                case "Semeter":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semeter)
                        : query.OrderBy(x => x.Semeter);
                    break;
                case "SurveyType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SurveyType)
                        : query.OrderBy(x => x.SurveyType);
                    break;
                case "SurveyName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SurveyName)
                        : query.OrderBy(x => x.SurveyName);
                    break;
                case "StartDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.StartDate);
                    break;
                case "EndDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndDate)
                        : query.OrderBy(x => x.EndDate);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;

            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetPublishSurveyResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Semeter = x.Semeter,
                    SurveyType = x.SurveyType,
                    SurveyName = x.SurveyName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    IdSurveyTemplate = x.IdTemplate,
                    IdSurveyTemplateChild = x.IdTemplateChild,
                    SurveyTemplateType = x.SurveyTemplateType,
                    IdAcademicYear = x.IdAcademicYear
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetPublishSurveyResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Semeter = x.Semeter,
                    SurveyType = x.SurveyType,
                    SurveyName = x.SurveyName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    IdSurveyTemplate = x.IdTemplate,
                    IdSurveyTemplateChild = x.IdTemplateChild,
                    SurveyTemplateType = x.SurveyTemplateType,
                    IdAcademicYear = x.IdAcademicYear
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddPublishSurveyRequest, AddPublishSurveyValidator>();

            var exsisTitle = await _dbContext.Entity<TrPublishSurvey>()
                .Where(e => e.IdAcademicYear == body.IdAcademicYear && e.Title.ToLower() == body.SurveyName.ToLower())
                .AnyAsync(CancellationToken);

            if (exsisTitle)
                throw new BadRequestException("title publish survey is exsis");

            var surveyTemplateType = await _dbContext.Entity<MsSurveyTemplate>()
                                      .Where(e => e.Id == body.IdSurveyTemplate)
                                      .Select(e => e.Type)
                                      .FirstOrDefaultAsync(CancellationToken);

            var NewSurveyTemplatePublish = new TrPublishSurvey
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester,
                Title = body.SurveyName,
                SurveyType = body.SurveyType,
                Description = body.Description,
                IdSurveyTemplate = body.IdSurveyTemplate,
                IdSurveyTemplateChild = body.IdSurveyTemplateChild,
                IdPublishSurveyLink = body.IdPublishSurveyLink,
                IdSurveyTemplateLink = body.IdSurveyTemplateLink,
                IdSurveyTemplateChildLink = body.IdSurveyTemplateChildLink,
                StartDate = body.StartDate,
                EndDate = body.EndDate,
                IsGrapicExtender = body.IsGrapicExtender,
                IsMandatory = body.IsMandatory,
                IsEntryOneTime = body.IsEntryOneTime,
                SubmissionOption = body.SubmissionOption,
                AboveSubmitText = body.AboveSubmitText,
                ThankYouMessage = body.ThankYouMessage,
                AfterSurveyCloseText = body.AfterSurveyCloseText,
                Status = surveyTemplateType == SurveyTemplateType.Survey ? PublishSurveyStatus.Publish : PublishSurveyStatus.Process
            };
            _dbContext.Entity<TrPublishSurvey>().Add(NewSurveyTemplatePublish);


            var respondent = body.Respondent.SelectMany(e => e.Grade).ToList();
            foreach (var itemRespondent in body.Respondent)
            {
                var NewSurveyTemplateRespondent = new TrPublishSurveyRespondent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdPublishSurvey = NewSurveyTemplatePublish.Id,
                    Role = itemRespondent.Role,
                    Option = itemRespondent.Option
                };
                _dbContext.Entity<TrPublishSurveyRespondent>().Add(NewSurveyTemplateRespondent);

                if (itemRespondent.Option == PublishSurveyOption.Position)
                {
                    if (itemRespondent.Role != PublishSurveyRole.Staff && itemRespondent.Role != PublishSurveyRole.Teacher)
                        throw new BadRequestException("Invalid role for 'Position' option");

                    var getUserRolePositionService = await _serviceRolePosition.GetUserRolePosition(new GetUserRolePositionRequest
                    {
                        IdAcademicYear = body.IdAcademicYear,
                        IdSchool = body.IdSchool,
                        UserRolePositions = new List<GetUserRolePosition>()
                            {
                                new GetUserRolePosition
                                {
                                    Role = itemRespondent.Role == PublishSurveyRole.Staff ? UserRolePersonalOptionRole.STAFF : UserRolePersonalOptionRole.TEACHER,
                                    Option = UserRolePersonalOptionType.Position,
                                    TeacherPositions = itemRespondent.IdPosition
                                }
                            }
                    });

                    var getUserRolePosition = getUserRolePositionService.IsSuccess ? getUserRolePositionService.Payload : null;

                    if (!getUserRolePosition.Any())
                        throw new BadRequestException("User doesn't have role position");

                    foreach (var idPosition in itemRespondent.IdPosition)
                    {
                        var NewSurveyTemplatePosition = new TrPublishSurveyPosition
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdTeacherPosition = idPosition,
                        };
                        _dbContext.Entity<TrPublishSurveyPosition>().Add(NewSurveyTemplatePosition);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.SpecificUser)
                {
                    foreach (var itemUser in itemRespondent.User)
                    {
                        var NewSurveyTemplateUser = new TrPublishSurveyUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdUser = itemUser.IdUser,
                            //IdTeacherPosition = itemUser.IdTeacherPosition
                        };

                        if (!string.IsNullOrEmpty(itemUser.IdTeacherPosition))
                            NewSurveyTemplateUser.IdTeacherPosition = itemUser.IdTeacherPosition;
                        _dbContext.Entity<TrPublishSurveyUser>().Add(NewSurveyTemplateUser);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.Department)
                {
                    foreach (var idDepartement in itemRespondent.IdDepartment)
                    {
                        var NewSurveyTemplateDepartement = new TrPublishSurveyDepartment
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdDepartement = idDepartement,
                        };
                        _dbContext.Entity<TrPublishSurveyDepartment>().Add(NewSurveyTemplateDepartement);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.Level || itemRespondent.Option == PublishSurveyOption.Grade)
                {
                    foreach (var itemGrade in itemRespondent.Grade)
                    {
                        var NewSurveyTemplateGrade = new TrPublishSurveyGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdLevel = itemGrade.IdLevel,
                            IdGrade = itemGrade.IdGrade,
                            Semester = itemGrade.Semester,
                            IdHomeroom = itemGrade.IdHomeroom,
                        };
                        _dbContext.Entity<TrPublishSurveyGrade>().Add(NewSurveyTemplateGrade);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.All)
                {
                    if (itemRespondent.Role == PublishSurveyRole.Student)
                    {
                        var getUserRolePositionService = await _serviceRolePosition.GetUserRolePosition(new GetUserRolePositionRequest
                        {
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            UserRolePositions = new List<GetUserRolePosition>()
                            {
                                new GetUserRolePosition
                                {
                                    Role = UserRolePersonalOptionRole.STUDENT,
                                    Option = UserRolePersonalOptionType.All
                                }
                            }
                        });

                        var getUserRolePosition = getUserRolePositionService.IsSuccess ? getUserRolePositionService.Payload : null;

                        if (!getUserRolePosition.Any())
                            throw new BadRequestException("User doesn't have role position");

                        var listRespondent = getUserRolePosition
                                    .GroupBy(e => new
                                    {
                                        IdGrade = e.Grade.Id,
                                        IdLevel = e.Level.Id,
                                        IdHomeroom = e.Homeroom.Id,
                                        Semester = e.Homeroom.Semester
                                    })
                                    .Select(e => new PublishSurveyRespondentGrade
                                    {
                                        IdGrade = e.Key.IdGrade,
                                        IdLevel = e.Key.IdLevel,
                                        IdHomeroom = e.Key.IdHomeroom,
                                        Semester = e.Key.Semester
                                    })
                                    .ToList();

                        respondent.AddRange(listRespondent);

                        var listNewSurveyTemplateGrade = listRespondent
                                                            .Select(e => new TrPublishSurveyGrade
                                                            {
                                                                Id = Guid.NewGuid().ToString(),
                                                                IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                                                                IdLevel = e.IdLevel,
                                                                IdGrade = e.IdGrade,
                                                                Semester = e.Semester,
                                                                IdHomeroom = e.IdHomeroom,
                                                            })
                                                            .ToList();

                        _dbContext.Entity<TrPublishSurveyGrade>().AddRange(listNewSurveyTemplateGrade);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Mapping Student


            var getPublishSurvey = await _dbContext.Entity<TrPublishSurvey>()
                        .Where(e => e.Id == NewSurveyTemplatePublish.Id)
                        .FirstOrDefaultAsync(CancellationToken);

            GetAddMappingStudentRequest paramAddMappingStudent = new GetAddMappingStudentRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                GetPublishSurvey = getPublishSurvey,
                IdSurveyTemplate = body.IdSurveyTemplate,
                Respondent = respondent,
                IdUser = AuthInfo.UserId,
                SurveyTemplateType = surveyTemplateType,
                Type = PublishSurveyLogType.Create
            };

            RunAsync(paramAddMappingStudent);
            #endregion

            AddPublishSurveyResult item = new AddPublishSurveyResult
            {
                IdPublishSurvey = NewSurveyTemplatePublish.Id
            };

            return Request.CreateApiResult2(item as object);
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdatePublishSurveyRequest, UpdatePublishSurveyValidator>();

            var exsisTitle = await _dbContext.Entity<TrPublishSurvey>()
                .Where(e => e.IdAcademicYear == body.IdAcademicYear && e.Title.ToLower() == body.SurveyName.ToLower() && e.Id != body.Id)
                .AnyAsync(CancellationToken);

            if (exsisTitle)
                throw new BadRequestException("title publish survey is exsis");

            var surveyTemplateType = await _dbContext.Entity<MsSurveyTemplate>()
                                      .Where(e => e.Id == body.IdSurveyTemplate)
                                      .Select(e => e.Type)
                                      .FirstOrDefaultAsync(CancellationToken);

            var getSurveyTemplatePublish = await _dbContext.Entity<TrPublishSurvey>()
                                            .Include(e => e.AcademicYear)
                                            .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyDepartments)
                                            .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades)
                                            .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyPositions)
                                            .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyUsers)
                                            .Where(e => e.Id == body.Id)
                                            .FirstOrDefaultAsync(CancellationToken);

            #region Remove child
            var RemoveDepartment = getSurveyTemplatePublish.PublishSurveyRespondents.SelectMany(e => e.PublishSurveyDepartments).ToList();
            RemoveDepartment.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrPublishSurveyDepartment>().UpdateRange(RemoveDepartment);

            var RemoveGrade = getSurveyTemplatePublish.PublishSurveyRespondents.SelectMany(e => e.PublishSurveyGrades).ToList();
            RemoveGrade.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrPublishSurveyGrade>().UpdateRange(RemoveGrade);

            var RemovePosition = getSurveyTemplatePublish.PublishSurveyRespondents.SelectMany(e => e.PublishSurveyPositions).ToList();
            RemovePosition.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrPublishSurveyPosition>().UpdateRange(RemovePosition);

            var RemoveUser = getSurveyTemplatePublish.PublishSurveyRespondents.SelectMany(e => e.PublishSurveyUsers).ToList();
            RemoveUser.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrPublishSurveyUser>().UpdateRange(RemoveUser);

            var RemoveRespondent = getSurveyTemplatePublish.PublishSurveyRespondents.ToList();
            RemoveRespondent.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrPublishSurveyRespondent>().UpdateRange(RemoveRespondent);
            #endregion

            #region Update
            getSurveyTemplatePublish.IdAcademicYear = body.IdAcademicYear;
            getSurveyTemplatePublish.Title = body.SurveyName;
            getSurveyTemplatePublish.SurveyType = body.SurveyType;
            getSurveyTemplatePublish.Description = body.Description;
            getSurveyTemplatePublish.IdSurveyTemplate = body.IdSurveyTemplate;
            getSurveyTemplatePublish.IdSurveyTemplateChild = body.IdSurveyTemplateChild;
            getSurveyTemplatePublish.IdPublishSurveyLink = body.IdPublishSurveyLink;
            getSurveyTemplatePublish.IdSurveyTemplateLink = body.IdSurveyTemplateLink;
            getSurveyTemplatePublish.IdSurveyTemplateChildLink = body.IdSurveyTemplateChildLink;
            getSurveyTemplatePublish.StartDate = body.StartDate;
            getSurveyTemplatePublish.EndDate = body.EndDate;
            getSurveyTemplatePublish.IsGrapicExtender = body.IsGrapicExtender;
            getSurveyTemplatePublish.IsMandatory = body.IsMandatory;
            getSurveyTemplatePublish.IsEntryOneTime = body.IsEntryOneTime;
            getSurveyTemplatePublish.SubmissionOption = body.SubmissionOption;
            getSurveyTemplatePublish.AboveSubmitText = body.AboveSubmitText;
            getSurveyTemplatePublish.ThankYouMessage = body.ThankYouMessage;
            getSurveyTemplatePublish.AfterSurveyCloseText = body.AfterSurveyCloseText;
            getSurveyTemplatePublish.Status = surveyTemplateType == SurveyTemplateType.Survey ? PublishSurveyStatus.Publish : PublishSurveyStatus.Process;
            _dbContext.Entity<TrPublishSurvey>().UpdateRange(getSurveyTemplatePublish);


            var respondent = body.Respondent.SelectMany(e => e.Grade).ToList();
            foreach (var itemRespondent in body.Respondent)
            {
                var NewSurveyTemplateRespondent = new TrPublishSurveyRespondent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdPublishSurvey = getSurveyTemplatePublish.Id,
                    Role = itemRespondent.Role,
                    Option = itemRespondent.Option
                };
                _dbContext.Entity<TrPublishSurveyRespondent>().Add(NewSurveyTemplateRespondent);

                if (itemRespondent.Option == PublishSurveyOption.Position)
                {
                    foreach (var idPosition in itemRespondent.IdPosition)
                    {
                        var NewSurveyTemplatePosition = new TrPublishSurveyPosition
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdTeacherPosition = idPosition,
                        };
                        _dbContext.Entity<TrPublishSurveyPosition>().Add(NewSurveyTemplatePosition);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.SpecificUser)
                {
                    foreach (var itemUser in itemRespondent.User)
                    {
                        var NewSurveyTemplateUser = new TrPublishSurveyUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdUser = itemUser.IdUser,
                            //IdTeacherPosition = itemUser.IdTeacherPosition
                        };

                        if (!string.IsNullOrEmpty(itemUser.IdTeacherPosition))
                            NewSurveyTemplateUser.IdTeacherPosition = itemUser.IdTeacherPosition;

                        _dbContext.Entity<TrPublishSurveyUser>().Add(NewSurveyTemplateUser);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.Department)
                {
                    foreach (var idDepartement in itemRespondent.IdDepartment)
                    {
                        var NewSurveyTemplateDepartement = new TrPublishSurveyDepartment
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdDepartement = idDepartement,
                        };
                        _dbContext.Entity<TrPublishSurveyDepartment>().Add(NewSurveyTemplateDepartement);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.Level || itemRespondent.Option == PublishSurveyOption.Grade)
                {
                    foreach (var itemGrade in itemRespondent.Grade)
                    {
                        var NewSurveyTemplateGrade = new TrPublishSurveyGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                            IdLevel = itemGrade.IdLevel,
                            IdGrade = itemGrade.IdGrade,
                            Semester = itemGrade.Semester,
                            IdHomeroom = itemGrade.IdHomeroom,
                        };
                        _dbContext.Entity<TrPublishSurveyGrade>().Add(NewSurveyTemplateGrade);
                    }
                }

                if (itemRespondent.Option == PublishSurveyOption.All)
                {
                    if (itemRespondent.Role == PublishSurveyRole.Student)
                    {
                        var getUserRolePositionService = await _serviceRolePosition.GetUserRolePosition(new GetUserRolePositionRequest
                        {
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = getSurveyTemplatePublish.AcademicYear.IdSchool,
                            UserRolePositions = new List<GetUserRolePosition>()
                            {
                                new GetUserRolePosition
                                {
                                    Role = UserRolePersonalOptionRole.STUDENT,
                                    Option = UserRolePersonalOptionType.All
                                }
                            }
                        });

                        var getUserRolePosition = getUserRolePositionService.IsSuccess ? getUserRolePositionService.Payload : null;

                        var listRespondent = getUserRolePosition
                                    .GroupBy(e => new
                                    {
                                        IdGrade = e.Grade.Id,
                                        IdLevel = e.Level.Id,
                                        IdHomeroom = e.Homeroom.Id,
                                        Semester = e.Homeroom.Semester
                                    })
                                    .Select(e => new PublishSurveyRespondentGrade
                                    {
                                        IdGrade = e.Key.IdGrade,
                                        IdLevel = e.Key.IdLevel,
                                        IdHomeroom = e.Key.IdHomeroom,
                                        Semester = e.Key.Semester
                                    })
                                    .ToList();

                        respondent.AddRange(listRespondent);

                        var listNewSurveyTemplateGrade = listRespondent
                                                            .Select(e => new TrPublishSurveyGrade
                                                            {
                                                                Id = Guid.NewGuid().ToString(),
                                                                IdPublishSurveyRespondent = NewSurveyTemplateRespondent.Id,
                                                                IdLevel = e.IdLevel,
                                                                IdGrade = e.IdGrade,
                                                                Semester = e.Semester,
                                                                IdHomeroom = e.IdHomeroom,
                                                            })
                                                            .ToList();

                        _dbContext.Entity<TrPublishSurveyGrade>().AddRange(listNewSurveyTemplateGrade);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Mapping
            var getPublishSurvey = await _dbContext.Entity<TrPublishSurvey>()
                        .Where(e => e.Id == body.Id)
                        .FirstOrDefaultAsync(CancellationToken);

            GetAddMappingStudentRequest paramAddMappingStudent = new GetAddMappingStudentRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                GetPublishSurvey = getPublishSurvey,
                IdSurveyTemplate = body.IdSurveyTemplate,
                Respondent = respondent,
                IdUser = AuthInfo.UserId,
                SurveyTemplateType = surveyTemplateType,
                Type = PublishSurveyLogType.Update
            };

            RunAsync(paramAddMappingStudent);
            #endregion

            #endregion

            return Request.CreateApiResult2();
        }

        public async Task RunAsync(GetAddMappingStudentRequest data)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ISchoolDbContext>();
                var IdPublishSurveyLog = Guid.NewGuid().ToString();
                if (data.SurveyTemplateType == SurveyTemplateType.StudentLearningSurvey)
                {
                    try
                    {
                        #region create publish survey log
                        await AddAndUpdatePublishSurveyLog(new AddAndUpdatePublishSurveyLogRequet
                        {
                            IdUser = data.IdUser,
                            IsDone = false,
                            IsError = false,
                            IsProses = true,
                            IdPublishSurvey = data.GetPublishSurvey.Id,
                            Type = data.Type,
                            DbContext = dbContext,
                            IdPublishSurveyLog = IdPublishSurveyLog
                        });
                        #endregion

                        #region logic mapping
                        GetAddMappingStudentRequest paramAddMappingStudent = new GetAddMappingStudentRequest
                        {
                            DbContext = dbContext,
                            IdUser = data.IdUser,
                            IdAcademicYear = data.IdAcademicYear,
                            GetPublishSurvey = data.GetPublishSurvey,
                            IdSurveyTemplate = data.IdSurveyTemplate,
                            Respondent = data.Respondent,
                        };

                        if (data.Type == PublishSurveyLogType.Create)
                        {
                            var listAddMappingStudent = await GetAddMappingStudent(paramAddMappingStudent);
                            dbContext.Entity<TrPublishSurveyMapping>().AddRange(listAddMappingStudent);
                            await dbContext.SaveChangesAsync();
                        }

                        if (data.Type == PublishSurveyLogType.Update)
                        {
                            var listAddMappingStudent = await GetAddMappingStudent(paramAddMappingStudent);

                            var listIdLesson = listAddMappingStudent.Select(e => e.IdLesson).Distinct().ToList();
                            var listIdHomeroomStudent = listAddMappingStudent.Select(e => e.IdHomeroomStudent).Distinct().ToList();
                            var listIdBinusian = listAddMappingStudent.Select(e => e.IdBinusian).Distinct().ToList();


                            var countPublishSurvey = await dbContext.Entity<TrPublishSurveyMapping>()
                                                        .Where(e => e.IdPublishSurvey == data.GetPublishSurvey.Id
                                                                    && e.PublishSurvey.Semester == data.GetPublishSurvey.Semester)
                                                        .IgnoreQueryFilters()
                                                        .CountAsync(CancellationToken);

                            List<TrPublishSurveyMapping> listPublishSurvey = new List<TrPublishSurveyMapping>();
                            var index = 0;
                            var lenght = 3000;
                            do
                            {
                                var listPublishSurveyPerIndex = await dbContext.Entity<TrPublishSurveyMapping>()
                                                                    .Include(e => e.HomeroomStudent)
                                                                    .Include(e=>e.Lesson)
                                                                    .Where(e => e.IdPublishSurvey == data.GetPublishSurvey.Id
                                                                    && e.PublishSurvey.Semester == data.GetPublishSurvey.Semester)
                                                                    .IgnoreQueryFilters()
                                                                    .Skip(index).Take(lenght)
                                                                    .ToListAsync(CancellationToken);

                                listPublishSurvey.AddRange(listPublishSurveyPerIndex);
                                index += lenght;
                            } while (index <= countPublishSurvey);

                            foreach (var item in listPublishSurvey)
                            {
                                var RemoveMappingStudent = listAddMappingStudent.Where(e => e.IdLesson == item.IdLesson
                                                                    && e.IdHomeroomStudent == item.IdHomeroomStudent
                                                                    && e.IdBinusian == item.IdBinusian).Any();

                                if(item.Lesson.Semester != data.GetPublishSurvey.Semester && item.IsActive)
                                {
                                    item.IsActive = false;
                                    dbContext.Entity<TrPublishSurveyMapping>().Update(item);
                                    continue;
                                }

                                if (!RemoveMappingStudent)
                                {
                                    //remove
                                    if (item.IsActive)
                                    {
                                        item.IsActive = false;
                                        dbContext.Entity<TrPublishSurveyMapping>().Update(item);
                                    }
                                }
                                else
                                {
                                    //update
                                    if (!item.IsActive)
                                    {
                                        item.IsActive = true;
                                        dbContext.Entity<TrPublishSurveyMapping>().Update(item);
                                    }
                                }

                            }

                            foreach (var item in listAddMappingStudent)
                            {
                                var exsisPublishSurvey = listPublishSurvey.Where(e => e.IdLesson == item.IdLesson
                                                                    && e.IdHomeroomStudent == item.IdHomeroomStudent
                                                                    && e.IdBinusian == item.IdBinusian).Any();

                                //create
                                if (!exsisPublishSurvey)
                                    dbContext.Entity<TrPublishSurveyMapping>().Add(item);
                            }
                            await dbContext.SaveChangesAsync();
                        }
                        #endregion

                        #region publish survey log finish
                        data.GetPublishSurvey.Status = PublishSurveyStatus.Publish;
                        dbContext.Entity<TrPublishSurvey>().Update(data.GetPublishSurvey);
                        await dbContext.SaveChangesAsync();

                        await AddAndUpdatePublishSurveyLog(new AddAndUpdatePublishSurveyLogRequet
                        {
                            IdUser = data.IdUser,
                            IsDone = true,
                            IsError = false,
                            IsProses = false,
                            IdPublishSurvey = data.GetPublishSurvey.Id,
                            Type = data.Type,
                            DbContext = dbContext,
                            IdPublishSurveyLog = IdPublishSurveyLog
                        });
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        #region publish survey log gagal
                        data.GetPublishSurvey.Status = PublishSurveyStatus.Failed;
                        dbContext.Entity<TrPublishSurvey>().Update(data.GetPublishSurvey);
                        await dbContext.SaveChangesAsync();

                        await AddAndUpdatePublishSurveyLog(new AddAndUpdatePublishSurveyLogRequet
                        {
                            IdUser = data.IdUser,
                            IsDone = false,
                            IsError = true,
                            IsProses = false,
                            IdPublishSurvey = data.GetPublishSurvey.Id,
                            Message = ex.Message,
                            Type = data.Type,
                            DbContext = dbContext,
                            IdPublishSurveyLog = IdPublishSurveyLog
                        });
                        #endregion
                    }
                }
            }
        }

        public async Task<string> AddAndUpdatePublishSurveyLog(AddAndUpdatePublishSurveyLogRequet data)
        {
            var SurveySummaryLog = data.DbContext.Entity<TrPublishSurveyLog>()
                     .Where(e => e.Id == data.IdPublishSurveyLog)
                     .FirstOrDefault();

            if (SurveySummaryLog == null)
            {
                TrPublishSurveyLog newSurveySummaryLog = new TrPublishSurveyLog
                {
                    Id = data.IdPublishSurveyLog,
                    StartDate = _datetime.ServerTime,
                    UserIn = data.IdUser,
                    IsProcess = true,
                    IdPublishSurvey = data.IdPublishSurvey,
                    Type = data.Type
                };

                if (data.IsDone)
                {
                    newSurveySummaryLog.EndDate = _datetime.ServerTime;
                    newSurveySummaryLog.IsDone = data.IsDone;
                    newSurveySummaryLog.IsProcess = false;
                }

                if (data.IsError)
                {
                    newSurveySummaryLog.EndDate = _datetime.ServerTime;
                    newSurveySummaryLog.IsError = data.IsError;
                    newSurveySummaryLog.ErrorMessage = data.Message;
                    newSurveySummaryLog.IsProcess = false;
                }

                data.DbContext.Entity<TrPublishSurveyLog>().Add(newSurveySummaryLog);
                await data.DbContext.SaveChangesAsync(CancellationToken);
            }
            else
            {
                SurveySummaryLog.EndDate = _datetime.ServerTime;
                SurveySummaryLog.IsProcess = true;

                if (data.IsDone)
                {
                    SurveySummaryLog.IsDone = data.IsDone;
                    SurveySummaryLog.IsProcess = false;
                }

                if (data.IsError)
                {
                    SurveySummaryLog.IsError = data.IsError;
                    SurveySummaryLog.ErrorMessage = data.Message;
                    SurveySummaryLog.IsProcess = false;
                }

                data.DbContext.Entity<TrPublishSurveyLog>().Update(SurveySummaryLog);
                await data.DbContext.SaveChangesAsync(CancellationToken);
            }
            return data.IdPublishSurveyLog;
        }

        public static string GetStatusSurvey(DateTime dateTime, PublishSurveyStatus publishSurveyStatus, DateTime startDate, DateTime endDate)
        {
            string status = default;

            if (publishSurveyStatus == PublishSurveyStatus.Publish)
            {
                if (dateTime < startDate)
                    status = PublishSurveyStatus.Upcoming.GetDescription();

                if (dateTime >= startDate && dateTime <= endDate)
                    status = PublishSurveyStatus.Publish.GetDescription();

                if (dateTime > endDate)
                    status = PublishSurveyStatus.Unpublished.GetDescription();
            }
            else
            {
                status = publishSurveyStatus.GetDescription();
            }

            return status;
        }

        public async Task<List<TrPublishSurveyMapping>> GetAddMappingStudent(GetAddMappingStudentRequest data)
        {
            List<TrPublishSurveyMapping> listPublishSurveyMapping = new List<TrPublishSurveyMapping>();

            var semester = data.Respondent.Select(e => e.Semester).Distinct().FirstOrDefault();

            var listIdLevelRespondent = data.Respondent.Select(e => e.IdLevel).Distinct().ToList();
            var listIdHomeroomRespondent = data.Respondent
                                .Select(e => e.IdHomeroom)
                                .Where(e => !string.IsNullOrEmpty(e))
                                .Distinct()
                                .ToList();

            var queryHomeroomStudent = data.DbContext.Entity<MsHomeroomStudent>()
                                    .Where(e => listIdLevelRespondent.Contains(e.Homeroom.Grade.IdLevel));

            if (listIdHomeroomRespondent.Any())
                queryHomeroomStudent = queryHomeroomStudent
                                        .Where(e => listIdLevelRespondent.Contains(e.Homeroom.Grade.IdLevel));

            if (listIdHomeroomRespondent.Any())
                queryHomeroomStudent = queryHomeroomStudent.Where(e => listIdHomeroomRespondent.Contains(e.IdHomeroom));


            var listHomeroomStudent = await queryHomeroomStudent
                                        .Select(e => new
                                        {
                                            IdLevel = e.Homeroom.Grade.IdLevel,
                                            IdGrade = e.Homeroom.IdGrade,
                                            IdHomeroom = e.Homeroom.Id,
                                            Semester = e.Homeroom.Semester
                                        })
                                        .Distinct()
                                        .ToListAsync(CancellationToken);

            foreach (var itemHomeroom in listHomeroomStudent)
            {
                var getMappingStudentService = await _servicePublishService.GetResetMappingStudentLearningSurvey(new GetResetMappingStudentLearningSurveyRequest
                {
                    IdAcademicYear = data.IdAcademicYear,
                    IdLevel = itemHomeroom.IdLevel,
                    IdGrade = itemHomeroom.IdGrade,
                    Semester = itemHomeroom.Semester,
                    IdHomeroom = itemHomeroom.IdHomeroom,
                });

                var listStudentMappingService = getMappingStudentService.IsSuccess ? getMappingStudentService.Payload : null;

                if (listStudentMappingService != null)
                {
                    var MappingStudentLearningSurvey = listStudentMappingService.MappingStudentLearningSurvey;

                    foreach (var itemMappingStudent in MappingStudentLearningSurvey)
                    {
                        var listKey = itemMappingStudent
                                            .Where(e => e.Key.Contains("-"))
                                            .Select(e => e.Key).ToList();

                        var idStudent = itemMappingStudent
                                            .Where(e => e.Key == "IdStudent")
                                            .Select(e => e.Value).FirstOrDefault();


                        var idHomeroomStudent = itemMappingStudent
                                            .Where(e => e.Key == "IdHomeroomStudent")
                                            .Select(e => e.Value).FirstOrDefault();
                        foreach (var itemKey in listKey)
                        {
                            var objectValue = itemMappingStudent
                                            .Where(e => e.Key == itemKey)
                                            .Select(e => e.Value)
                            .FirstOrDefault();

                            var jsonString = JsonConvert.SerializeObject(objectValue);
                            var value = JsonConvert.DeserializeObject<MappingStudentLearningValueResult>(jsonString);

                            TrPublishSurveyMapping newPublishSurveyMapping = new TrPublishSurveyMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdPublishSurvey = data.GetPublishSurvey.Id,
                                IdBinusian = value.IdUserTeacher,
                                IdHomeroomStudent = idHomeroomStudent.ToString(),
                                IdLesson = value.IdLesson,
                                IsMapping = value.IsChecked
                            };

                            listPublishSurveyMapping.Add(newPublishSurveyMapping);
                        }
                    }
                }
            }

            return listPublishSurveyMapping;
        }
    }

    public class GetAddMappingStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdSurveyTemplate { get; set; }
        public string IdUser { get; set; }
        public TrPublishSurvey GetPublishSurvey { get; set; }
        public SurveyTemplateType SurveyTemplateType { get; set; }
        public List<PublishSurveyRespondentGrade> Respondent { get; set; }
        public PublishSurveyLogType Type { get; set; }
        public ISchoolDbContext DbContext { get; set; }
    }

    public class AddAndUpdatePublishSurveyLogRequet
    {
        public string IdPublishSurvey { get; set; }
        public string IdPublishSurveyLog { get; set; }
        public string IdUser { get; set; }
        public string Message { get; set; }
        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public bool IsProses { get; set; }
        public ISchoolDbContext DbContext { get; set; }
        public PublishSurveyLogType Type { get; set; }
    }

}
