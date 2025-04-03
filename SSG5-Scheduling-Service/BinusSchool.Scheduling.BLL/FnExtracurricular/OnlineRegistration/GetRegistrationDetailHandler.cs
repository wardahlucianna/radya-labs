using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration.Validator;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetRegistrationDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IOnlineRegistration _scpOnlineRegistrationApi;

        public GetRegistrationDetailHandler(ISchedulingDbContext dbContext,
            IOnlineRegistration scpOnlineRegistrationApi,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _scpOnlineRegistrationApi = scpOnlineRegistrationApi;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetRegistrationDetailRequest, GetRegistrationDetailValidator>();

            var studentGradeDetailResult = await _scpOnlineRegistrationApi.GetActiveStudentsGradeByStudent(new GetActiveStudentsGradeByStudentRequest
            {
                IdStudent = param.IdStudent
            });

            if (studentGradeDetailResult.Payload.Count() <= 0)
            {
                //throw new BadRequestException(null);
                return Request.CreateApiResult2();
            }

            var studentGradeDetailList = studentGradeDetailResult.Payload.ToList();

            var studentHomeroomList = studentGradeDetailList
                .Select(x => x)
                .Distinct()
                .OrderBy(x => x.Student.Name)
                .ToList();

            if (!string.IsNullOrEmpty(param.Search))
                studentHomeroomList = studentHomeroomList
                    .Where(x => x.Student.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                        || x.Homeroom.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            #region unused code
            //var studentHomeroomList = studentGradeDetailList
            //                        .Join(_dbContext.Entity<MsHomeroomStudent>()
            //                            .Include(hs => hs.Student)
            //                            .Include(hs => hs.Homeroom)
            //                            .ThenInclude(h => h.GradePathwayClassroom)
            //                            .ThenInclude(gpc => gpc.Classroom)
            //                            .Include(hs => hs.Homeroom)
            //                            .ThenInclude(h => h.Grade),
            //                            studentGrade => new { p1 = studentGrade.StudentId, p2 = studentGrade.Grade.Id, p3 = studentGrade.Semester },
            //                            homeroom => new { p1 = homeroom.IdStudent, p2 = homeroom.Homeroom.Grade.Id, p3 = homeroom.Homeroom.Semester },
            //                            (studentGrade, homeroom) => new
            //                            {
            //                                AcademicYear = studentGrade.AcadYear,
            //                                Grade = studentGrade.Grade,
            //                                Semester = studentGrade.Semester,
            //                                Student = new NameValueVm
            //                                {
            //                                    Id = homeroom.Student.Id,
            //                                    Name = (string.IsNullOrEmpty(homeroom.Student.FirstName.Trim()) ? "" : homeroom.Student.FirstName.Trim()) + (string.IsNullOrEmpty(homeroom.Student.LastName.Trim()) ? "" : (" " + homeroom.Student.LastName.Trim()))
            //                                },
            //                                Homeroom = new NameValueVm
            //                                {
            //                                    Id = homeroom.Homeroom.Id,
            //                                    Name = string.Format("{0}{1}", homeroom.Homeroom.Grade.Code, homeroom.Homeroom.GradePathwayClassroom.Classroom.Code)
            //                                }
            //                            })
            //                        .Where(x => // Search
            //                                    (EF.Functions.Like(x.Student.Name, param.SearchPattern()))
            //                                    ||
            //                                    (EF.Functions.Like(x.Homeroom.Name, param.SearchPattern()))
            //                        )
            //                        .Select(x => x)
            //                        .Distinct()
            //                        .OrderBy(x => x.Student.Name)
            //                        .ToList();
            #endregion

            var resultList = new List<GetRegistrationDetailResult>();

            foreach (var studentHomeroom in studentHomeroomList)
            {
                #region unused code
                //var studentHomeroom = await _dbContext.Entity<MsHomeroomStudent>()
                //                    .Include(hs => hs.Student)
                //                    .Include(hs => hs.Homeroom)
                //                    .ThenInclude(h => h.GradePathwayClassroom)
                //                    .ThenInclude(gpc => gpc.Classroom)
                //                    .Include(hs => hs.Homeroom)
                //                    .ThenInclude(h => h.Grade)
                //                    .Where(x => x.IdStudent == studentHomeroomDetail.StudentId &&
                //                                x.Homeroom.IdGrade == studentHomeroomDetail.Grade.Id &&
                //                                x.Homeroom.Semester == studentHomeroomDetail.Semester 

                //                                // Search
                //                                //(EF.Functions.Like(x.Student.FirstName.Trim() + x.Student.LastName.Trim(), param.SearchPattern()))
                //                                //||
                //                                //(EF.Functions.Like(string.Format("{0}{1}", x.Homeroom.Grade.Code, x.Homeroom.GradePathwayClassroom.Classroom.Code), param.SearchPattern()))
                //                                )
                //                    .Select(x => new 
                //                    {
                //                        Student = new NameValueVm
                //                        {
                //                            Id = x.Student.Id,
                //                            Name = (string.IsNullOrEmpty(x.Student.FirstName.Trim()) ? "" : x.Student.FirstName.Trim()) + (string.IsNullOrEmpty(x.Student.LastName.Trim()) ? "" : (" " + x.Student.LastName.Trim()))
                //                        },
                //                        Homeroom = new NameValueVm
                //                        {
                //                            Id = x.Homeroom.Id,
                //                            Name = string.Format("{0}{1}", x.Homeroom.Grade.Code, x.Homeroom.GradePathwayClassroom.Classroom.Code)
                //                        }
                //                    })
                //                    .FirstOrDefaultAsync(CancellationToken);
                #endregion

                var result = _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                .Include(x => x.ExtracurricularRule)
                                .Where(x => x.ExtracurricularRule.IdAcademicYear == studentHomeroom.AcadYear.Id &&
                                            x.ExtracurricularRule.Semester == studentHomeroom.Semester &&
                                            x.IdGrade == studentHomeroom.Grade.Id &&
                                            x.ExtracurricularRule.Status == true &&
                                            // validating if current date is inside registration period
                                            (_dateTime.ServerTime >= x.ExtracurricularRule.RegistrationStartDate &&
                                            _dateTime.ServerTime <= x.ExtracurricularRule.RegistrationEndDate)
                                            )
                                .Select(x => new GetRegistrationDetailResult
                                {
                                    AcademicYear = studentHomeroom.AcadYear,
                                    Grade = studentHomeroom.Grade,
                                    RegistrationStartDate = x.ExtracurricularRule.RegistrationStartDate.HasValue ? x.ExtracurricularRule.RegistrationStartDate.Value.ToString("yyyy-MM-dd HH:mm") : null,
                                    RegistrationEndDate = x.ExtracurricularRule.RegistrationEndDate.HasValue ? x.ExtracurricularRule.RegistrationEndDate.Value.ToString("yyyy-MM-dd HH:mm") : null,
                                    Student = studentHomeroom.Student,
                                    Homeroom = studentHomeroom.Homeroom,
                                    Semester = studentHomeroom.Semester,
                                    MaxEffective = x.ExtracurricularRule.MaxEffectives,
                                    MinEffective = x.ExtracurricularRule.MinEffectives,
                                    ReviewDate = x.ExtracurricularRule.ReviewDate.HasValue ? x.ExtracurricularRule.ReviewDate.Value.ToString("yyyy-MM-dd HH:mm") : null
                                })
                                .FirstOrDefault();

                if (result != null)
                    resultList.Add(result);
            }

            if (param.Return == CollectionType.Lov)
            {
                var items = resultList
                            .Select(x => new
                            {
                                Student = x.Student,
                                Homeroom = x.Homeroom
                            })
                            .ToList();

                return Request.CreateApiResult2(items as object);
            }
            else
            {
                var items = resultList
                             .SetPagination(param)
                             .Select(x => new GetRegistrationDetailResult
                             {
                                 AcademicYear = x.AcademicYear,
                                 Grade = x.Grade,
                                 RegistrationStartDate = x.RegistrationStartDate,
                                 RegistrationEndDate = x.RegistrationEndDate,
                                 Student = x.Student,
                                 Homeroom = x.Homeroom,
                                 Semester = x.Semester,
                                 MaxEffective = x.MaxEffective,
                                 MinEffective = x.MinEffective,
                                 ReviewDate = x.ReviewDate
                             })
                             .ToList();

                return Request.CreateApiResult2(items as object);
            }
        }

    }
}
