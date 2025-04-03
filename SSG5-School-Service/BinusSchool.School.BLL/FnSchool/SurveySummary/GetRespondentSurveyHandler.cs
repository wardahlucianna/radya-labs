using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentStatus;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.OData.UriParser;
using Newtonsoft.Json;
using NPOI.Util;

namespace BinusSchool.School.FnSchool.SurveySummary
{
    public class GetRespondentSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IParent _serviceParent;
        private readonly IRolePosition _serviceRolePosition;
        public GetRespondentSurveyHandler(ISchoolDbContext dbContext, IParent serviceParent, IRolePosition serviceRolePosition)
        {
            _dbContext = dbContext;
            _serviceParent = serviceParent;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveySummaryUserRespondentRequest>();

            var predicate = PredicateBuilder.Create<TrPublishSurveyRespondent>(x => x.IsActive);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.PublishSurvey.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.PublishSurvey.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdPublishSurvey))
                predicate = predicate.And(x => x.IdPublishSurvey == param.IdPublishSurvey);

            var listSurveyTemplateRespondent = await _dbContext.Entity<TrPublishSurveyRespondent>()
                                .Include(e => e.PublishSurveyDepartments).ThenInclude(e => e.Department).ThenInclude(e => e.DepartmentLevels)
                                .Include(e => e.PublishSurveyPositions).ThenInclude(e => e.TeacherPosition)
                                .Include(e => e.PublishSurveyUsers).ThenInclude(e => e.User)
                                .Include(e => e.PublishSurveyUsers).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.RolePositions).ThenInclude(e => e.Role).ThenInclude(e => e.RoleGroup)
                                .Include(e => e.PublishSurveyGrades).ThenInclude(e => e.Grade)
                                .Include(e => e.PublishSurvey).ThenInclude(e => e.SurveyTemplate)
                                .Include(e => e.PublishSurvey).ThenInclude(e => e.AcademicYear)
                                .Where(predicate)
                                .ToListAsync(CancellationToken);

            var idSchool = listSurveyTemplateRespondent.Select(e => e.PublishSurvey.AcademicYear.IdSchool).FirstOrDefault();

            List<GetSurveySummaryUserRespondentResult> listRespondent = new List<GetSurveySummaryUserRespondentResult>();
            List<GetUserRolePosition> listUserRolePosition = new List<GetUserRolePosition>();
            foreach (var item in listSurveyTemplateRespondent)
            {
                UserRolePersonalOptionRole role = UserRolePersonalOptionRole.ALL;
                switch (item.Role)
                {
                    case PublishSurveyRole.Staff:
                        role = UserRolePersonalOptionRole.STAFF;
                        break;
                    case PublishSurveyRole.Teacher:
                        role = UserRolePersonalOptionRole.TEACHER;
                        break;
                    case PublishSurveyRole.Student:
                        role = UserRolePersonalOptionRole.STUDENT;
                        break;
                    case PublishSurveyRole.Parent:
                        role = UserRolePersonalOptionRole.PARENT;
                        break;
                    case PublishSurveyRole.All:
                    default:
                        break;
                }

                UserRolePersonalOptionType option = UserRolePersonalOptionType.All;
                var personal = new List<string>();

                switch (item.Option)
                {
                    case PublishSurveyOption.All:
                        option = UserRolePersonalOptionType.All;
                        break;
                    case PublishSurveyOption.Position:
                        option = UserRolePersonalOptionType.Position;
                        personal.AddRange(item.PublishSurveyUsers.Select(e => e.IdUser).ToList());
                        break;
                    case PublishSurveyOption.SpecificUser:
                        option = UserRolePersonalOptionType.Personal;
                        break;
                    case PublishSurveyOption.Department:
                        option = UserRolePersonalOptionType.Department;
                        break;
                    case PublishSurveyOption.Level:
                        option = UserRolePersonalOptionType.Level;
                        break;
                    case PublishSurveyOption.Grade:
                        option = UserRolePersonalOptionType.Grade;
                        break;
                    case null:
                        option = UserRolePersonalOptionType.None;
                        break;
                    default:
                        break;
                }

                var newUserRolePosition = new GetUserRolePosition
                {
                    Role = role,
                    Option = option,
                    Departemens = item.PublishSurveyDepartments.Select(e => e.IdDepartement).Distinct().ToList(),
                    TeacherPositions = item.PublishSurveyPositions.Select(e => e.IdTeacherPosition).Distinct().ToList(),
                    Level = item.PublishSurveyGrades.Select(e => e.IdLevel).Distinct().ToList(),
                    Homeroom = item.PublishSurveyGrades.Where(e => e.IdHomeroom != null).Select(e => e.IdHomeroom).Distinct().ToList(),
                    Personal = item.PublishSurveyUsers.Select(e => e.IdUser).Distinct().ToList(),
                    IdUserRolePositions = item.IdPublishSurvey
                };
                listUserRolePosition.Add(newUserRolePosition);
            }

            var paramUserRolePosition = new GetUserRolePositionRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdSchool = idSchool,
                UserRolePositions = listUserRolePosition,
                IdUser = param.IdUserParent == null
                            ? param.IdUser == null
                                ? null : param.IdUser
                            : param.IdUserParent,
            };

            string jsonString = JsonConvert.SerializeObject(paramUserRolePosition);

            var getUserRolePositionService = await _serviceRolePosition.GetUserRolePosition(paramUserRolePosition);
            if (getUserRolePositionService.IsSuccess)
            {
                if (!getUserRolePositionService.Payload.Any())
                    return Request.CreateApiResult2();
                //throw new Exception("User doesn't have role position");
            }
            else
            {
                throw new Exception(getUserRolePositionService.Message);
            }
            var getUserRolePosition = getUserRolePositionService.IsSuccess
                                        ? getUserRolePositionService.Payload
                                        .GroupBy(e => new
                                        {
                                            IdUser = e.IdUser,
                                            Role = e.Role,
                                            IdUserChild = e.IdUserChild,
                                            IdHomeroomStudent = e.IdHomeroomStudent,
                                            IdLevel = e.Level == null ? null : e.Level.Id,
                                            Level = e.Level == null ? null : e.Level.Description,
                                            LevelCode = e.Level == null ? null : e.Level.Code,
                                            LevelOrder = e.Level == null ? 0 : e.Level.OrderNumber,
                                            IdGrade = e.Grade == null ? null : e.Grade.Id,
                                            Grade = e.Grade == null ? null : e.Grade.Description,
                                            GradeCode = e.Grade == null ? null : e.Grade.Code,
                                            GradeOrder = e.Grade == null ? 0 : e.Grade.OrderNumber,
                                            IdHomeroom = e.Homeroom == null ? null : e.Homeroom.Id,
                                            Homeroom = e.Homeroom == null ? null : e.Homeroom.Description,
                                            Semester = e.Homeroom == null ? null : e.Homeroom.Semester,
                                            IdPublishSurvey = e.IdUserRolePositions,
                                        })
                                        .Select(e => new GetSurveySummaryUserRespondentResult
                                        {
                                            IdUser = e.Key.IdUser,
                                            Role = e.Key.Role,
                                            IdUserChild = e.Key.IdUserChild == null ? e.Key.IdUser : e.Key.IdUserChild,
                                            IdPusblishSurvey = e.Key.IdPublishSurvey,
                                            IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                            Level = new ItemValueVmWithOrderNumber
                                            {
                                                Id = e.Key.IdLevel,
                                                Description = e.Key.Level,
                                                Code = e.Key.LevelCode,
                                                OrderNumber = e.Key.LevelOrder
                                            },
                                            Grade = new ItemValueVmWithOrderNumber
                                            {
                                                Id = e.Key.IdGrade,
                                                Description = e.Key.Grade,
                                                Code = e.Key.GradeCode,
                                                OrderNumber = e.Key.GradeOrder
                                            },
                                            Homeroom = new SurveySummaryUserRespondentHomeroom
                                            {
                                                Id = e.Key.IdHomeroom,
                                                Description = e.Key.Homeroom,
                                                Semester = e.Key.Semester
                                            }
                                        })
                                        .ToList()
                                        : null;

            var getUserParentStudent = getUserRolePosition
                                        .Where(e => e.Homeroom.Semester == param.Semester
                                                    && (e.Role == UserRolePersonalOptionRole.PARENT.GetDescription()
                                                        || e.Role == UserRolePersonalOptionRole.STUDENT.GetDescription()))
                                        .ToList();

            var getUserStaffTeacher = getUserRolePosition
                                        .Where(e => (e.Role == UserRolePersonalOptionRole.STAFF.GetDescription()
                                                        || e.Role == UserRolePersonalOptionRole.TEACHER.GetDescription()))
                                        .ToList();

            getUserRolePosition = getUserParentStudent.Union(getUserStaffTeacher).ToList();

            if (!string.IsNullOrEmpty(param.IdUser))
            {
                var listPublishSurvey = listSurveyTemplateRespondent
                            .Select(e => new
                            {
                                e.IdPublishSurvey,
                                e.PublishSurvey.SubmissionOption,
                                Respondent = e.PublishSurvey.PublishSurveyRespondents.Select(e => e.Role).ToList()
                            })
                            .Distinct().ToList();

                foreach (var item in listPublishSurvey)
                {
                    var SubmissionOption = item.SubmissionOption;

                    if (SubmissionOption == null)
                    {
                        var listUserRolePositionByIdPublish = getUserRolePosition.Where(e => e.IdPusblishSurvey == item.IdPublishSurvey && e.IdUser==param.IdUser).ToList();
                        listRespondent.AddRange(listUserRolePositionByIdPublish);
                    }
                    else
                    {
                        List<GetSurveySummaryUserRespondentResult> listRespondentById = new List<GetSurveySummaryUserRespondentResult>();

                        if (SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerFamily)
                            //parent only
                            listRespondentById = getUserRolePosition.Where(e => e.IdPusblishSurvey == item.IdPublishSurvey && e.IdUser == param.IdUser && e.Role==RoleConstant.Parent).ToList();

                        if (SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerChild)
                            //child only
                            listRespondentById = getUserRolePosition.Where(e => e.IdPusblishSurvey == item.IdPublishSurvey).ToList();

                        else if (SubmissionOption == PublishSurveySubmissionOption.Submit1ReviewPerChildOr1ReviewPerFamily)
                            //all
                            listRespondentById = getUserRolePosition.Where(e => e.IdPusblishSurvey == item.IdPublishSurvey).ToList();

                        listRespondent.AddRange(listRespondentById);
                    }
                }
            }
            else
                listRespondent.AddRange(getUserRolePosition);

            return Request.CreateApiResult2(listRespondent as object);
        }

    }


}
