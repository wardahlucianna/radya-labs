using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition
{
    public class GetListFilterAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetHomeroomTeacherPrivilegeHandler _getHomeroomTeacherPrivilegeHandler;

        public GetListFilterAttendanceHandler(
            IAttendanceDbContext DbContext,
            GetHomeroomTeacherPrivilegeHandler getHomeroomTeacherPrivilegeHandler
            )
        {
            _dbContext = DbContext;
            _getHomeroomTeacherPrivilegeHandler = getHomeroomTeacherPrivilegeHandler;

        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListFilterAttendanceRequest>(
                nameof(GetListFilterAttendanceRequest.IdUser),
                nameof(GetListFilterAttendanceRequest.IdSchool),
                nameof(GetListFilterAttendanceRequest.IdAcademicYear)
                );

            var retVal = new GetListFilterAttendanceResult();
            var StudentEnrollmentList = new List<GetHomeroomTeacherPrivilegeResult>();

            var queryLevel = new List<GetListFilterAttendanceResult_Level>();
            var queryGrade = new List<GetListFilterAttendanceResult_Grade>();
            var querySemester = new List<GetListFilterAttendanceResult_Semester>();
            var queryTerm = new List<GetListFilterAttendanceResult_Term>();

            StudentEnrollmentList = await _getHomeroomTeacherPrivilegeHandler.GetHomeroomTeacherPrivileges(new GetHomeroomTeacherPrivilegeRequest
            {
                IdSchool = param.IdSchool,
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                Semester = null,
                IncludeClassAdvisor = true,
                IncludeSubjectTeacher = true
            });

            var getAccessLevel = StudentEnrollmentList
                        .Select(x => x.IdLevel).Distinct().ToList();

            var getAccessGrade = StudentEnrollmentList
                        .Select(x => x.IdGrade).Distinct().ToList();

            if (param.ShowLevel)
            {
                var queryLevelList = await _dbContext.Entity<MsLevel>()
                        .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => getAccessLevel.Any(y => y == x.Id))
                        .Select(x => new
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            OrderNumber = x.OrderNumber
                        }).Distinct()
                        .ToListAsync(CancellationToken);

                queryLevel = queryLevelList
                        .Select(x => new GetListFilterAttendanceResult_Level
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            OrderNumber = x.OrderNumber
                        }).OrderBy(x => x.OrderNumber).ThenBy(x => x.Code).ThenBy(x => x.Description)
                        .ToList();

                retVal.Level = queryLevel;
            }
            if (param.ShowGrade)
            {
                var queryGradeList = await _dbContext.Entity<MsGrade>()
                        .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => getAccessGrade.Any(y => y == x.Id))
                        .Select(x => new
                        {
                            IdLevel = x.IdLevel,
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            OrderNumber = x.OrderNumber
                        }).Distinct()
                        .ToListAsync(CancellationToken);

                queryGrade = queryGradeList
                       .Select(x => new GetListFilterAttendanceResult_Grade
                       {
                           IdLevel = x.IdLevel,
                           Id = x.Id,
                           Code = x.Code,
                           Description = x.Description,
                           OrderNumber = x.OrderNumber
                       }).OrderBy(x => x.OrderNumber).ThenBy(x => x.Code).ThenBy(x => x.Description)
                       .ToList();

                retVal.Grade = queryGrade;
            }
            if (param.ShowSemester || param.ShowTerm)
            {
                var queryPeriod = await _dbContext.Entity<MsPeriod>()
                        .Include(x => x.Grade)
                        .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => getAccessGrade.Any(y => y == x.IdGrade))
                        .OrderBy(x => x.OrderNumber)
                            .ThenBy(x => x.Semester)
                            .ThenBy(x => x.Code)
                        .ToListAsync(CancellationToken);

                if (param.ShowSemester)
                {
                    var querySemesterList = queryPeriod
                            .Select(x => new
                            {
                                IdGrade = x.IdGrade,
                                Id = x.Semester.ToString(),
                                Code = x.Semester.ToString(),
                                Description = x.Semester.ToString(),
                                OrderNumber = x.OrderNumber
                            }).Distinct()
                            .ToList();

                    querySemester = querySemesterList
                            .Select(x => new GetListFilterAttendanceResult_Semester
                            {
                                IdGrade = x.IdGrade,
                                Id = x.Id,
                                Code = x.Code,
                                Description = x.Description,
                                OrderNumber = x.OrderNumber
                            }).OrderBy(x => x.OrderNumber).ThenBy(x => x.Code).ThenBy(x => x.Description)
                            .ToList();

                    retVal.Semester = querySemester;
                }
                if (param.ShowTerm)
                {
                    var queryTermList = queryPeriod
                            .Select(x => new
                            {
                                IdGrade = x.IdGrade,
                                Semester = x.Semester,
                                Id = x.Code.Substring(x.Code.Length - 1),
                                Code = x.Code.Substring(x.Code.Length - 1).ToString(),
                                Description = "Term " + x.Code.Substring(x.Code.Length - 1).ToString(),
                                OrderNumber = x.OrderNumber
                            }).Distinct()
                            .ToList();

                    queryTerm = queryTermList
                            .Select(x => new GetListFilterAttendanceResult_Term
                            {
                                IdGrade = x.IdGrade,
                                Semester = x.Semester,
                                Id = x.Id,
                                Code = x.Code,
                                Description = x.Description,
                                OrderNumber = x.OrderNumber
                            }).OrderBy(x => x.OrderNumber).ThenBy(x => x.Code).ThenBy(x => x.Description)
                            .ToList();

                    retVal.Term = queryTerm;
                }
            }

            return Request.CreateApiResult2(retVal as object);
        }
    }
}
