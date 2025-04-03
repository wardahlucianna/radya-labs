using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class GetStudentStatusListByAYHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentStatusListByAYHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static readonly string[] _columns = { "studentId", "studentName", "homeroomName" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "Student.Id" },
            { _columns[1], "Student.Name" },
            { _columns[2], "Homeroom.Description" }
        };

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentStatusListByAYRequest>(
                            nameof(GetStudentStatusListByAYRequest.IdSchool),
                            nameof(GetStudentStatusListByAYRequest.IdAcademicYear),
                            nameof(GetStudentStatusListByAYRequest.Semester)
                            );

            var predicate = PredicateBuilder.Create<TrStudentStatus>(x => true);

            if (!string.IsNullOrWhiteSpace(param.SearchStudentKeyword))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Student.Id, $"%{param.SearchStudentKeyword}%")
                    || EF.Functions.Like(
                        (string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName) +
                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName) +
                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName)
                        , $"%{param.SearchStudentKeyword}%")
                    );

            // get startDate and endDate AY
            var gerPeriodAY = await _dbContext.Entity<MsPeriod>()
                                .Include(x => x.Grade)
                                    .ThenInclude(x => x.MsLevel)
                                .Where(x => x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                .GroupBy(x => x.Grade.MsLevel.IdAcademicYear)
                                .Select(x => new
                                {
                                    IdAcademicYear = x.Key,
                                    MinAYStartDate = x.Min(y => y.StartDate),
                                    MaxAYEndDate = x.Max(y => y.EndDate)
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            var studentRawList = _dbContext.Entity<TrStudentStatus>()
                                        .Include(x => x.StudentStatus)
                                        .Include(x => x.Student)
                                        .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                    x.CurrentStatus == "A" &&
                                                    (string.IsNullOrEmpty(param.IdStudentStatus) ? true : x.IdStudentStatus == int.Parse(param.IdStudentStatus))
                                                    )
                                        .Where(predicate)
                                        .ToList()
                                        .GroupBy(x => x.IdStudent)
                                        .Select(x => x.OrderByDescending(y => y.StartDate).ThenByDescending(y => y.DateIn).FirstOrDefault())
                                        .ToList();

            var studentWithHomeroomList = studentRawList
                                            .GroupJoin(
                                                _dbContext.Entity<MsHomeroomStudent>()
                                                 .Include(x => x.Homeroom)
                                                     .ThenInclude(x => x.Grade)
                                                     .ThenInclude(x => x.MsLevel)
                                                     .ThenInclude(x => x.MsAcademicYear)
                                                     .ThenInclude(x => x.MsSchool)
                                                 .Include(x => x.Homeroom)
                                                     .ThenInclude(x => x.MsGradePathwayClassroom)
                                                     .ThenInclude(x => x.Classroom)
                                                 .Include(x => x.Student)
                                                 .Where(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear &&
                                                             x.Semester == param.Semester &&
                                                             (string.IsNullOrEmpty(param.IdLevel) ? true : x.Homeroom.Grade.MsLevel.Id == param.IdLevel) &&
                                                             (string.IsNullOrEmpty(param.IdGrade) ? true : x.Homeroom.Grade.Id == param.IdGrade) &&
                                                             (string.IsNullOrEmpty(param.IdHomeroom) ? true : x.Homeroom.Id == param.IdHomeroom)
                                                             ),
                                                studentStatus => studentStatus.IdStudent,
                                                homeroomStudent => homeroomStudent.IdStudent,
                                                (studentStatus, homeroomStudent) => new { studentStatus, homeroomStudent }
                                            )
                                            .SelectMany(
                                                x => x.homeroomStudent.DefaultIfEmpty(),
                                                (studentStatus, homeroomStudent) => new GetStudentStatusListByAYResult
                                                {
                                                    Student = new NameValueVm
                                                    {
                                                        Id = studentStatus.studentStatus.IdStudent,
                                                        Name = NameUtil.GenerateFullName(studentStatus.studentStatus.Student.FirstName, studentStatus.studentStatus.Student.MiddleName, studentStatus.studentStatus.Student.LastName)
                                                    },
                                                    Homeroom = new ItemValueVm
                                                    {
                                                        Id = homeroomStudent?.IdHomeroom,
                                                        Description = homeroomStudent?.Homeroom.Grade.Description + homeroomStudent?.Homeroom.MsGradePathwayClassroom.Classroom.Description
                                                    },
                                                    LatestStudentStatus = new ItemValueVm
                                                    {
                                                        Id = studentStatus.studentStatus.IdStudentStatus.ToString(),
                                                        Description = studentStatus.studentStatus.StudentStatus.LongDesc
                                                    },
                                                    LatestStudentStatusStartDate = studentStatus.studentStatus.StartDate,
                                                    StartDateAY = gerPeriodAY.MinAYStartDate,
                                                    EndDateAY = gerPeriodAY.MaxAYEndDate
                                                })
                                            .ToList();

            var finalStudentWithHomeroomList = studentWithHomeroomList
                                                .Where(x => (!string.IsNullOrEmpty(param.IdLevel) ||
                                                            !string.IsNullOrEmpty(param.IdGrade) ||
                                                            !string.IsNullOrEmpty(param.IdHomeroom))
                                                            ? (x.Homeroom.Id != null) : true)
                                                .ToList();

            var finalResult = finalStudentWithHomeroomList
                                .AsQueryable()
                                .OrderByDynamic(param, _aliasColumns)
                                .SetPagination(param)
                                .ToList();

            var count = param.CanCountWithoutFetchDb(finalStudentWithHomeroomList.Count)
                ? finalStudentWithHomeroomList.Count
                : finalStudentWithHomeroomList.Select(x => x.Student.Id).Count();

            return Request.CreateApiResult2(finalResult as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        #region unused code
        //private static readonly string[] _columns = { "studentId", "studentName", "homeroomName" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "homeroomStudent.IdStudent" },
        //    { _columns[1], "(homeroomStudent.Student.FirstName + homeroomStudent.Student.MiddleName + homeroomStudent.Student.LastName)" },
        //    { _columns[2], "(homeroomStudent.Homeroom.Grade.Description + homeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Description)" }
        //};

        //private static readonly string[] _columns = { "studentId", "studentName", "homeroomName" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "Key.idStudent" },
        //    { _columns[1], "Key.StudentFirstName" },
        //    { _columns[2], "Key.GradeDesc" }
        //};
        //protected override async Task<ApiErrorResult<object>> Handler()
        //{
        //    var param = Request.ValidateParams<GetStudentStatusListByAYRequest>(
        //                    nameof(GetStudentStatusListByAYRequest.IdSchool),
        //                    nameof(GetStudentStatusListByAYRequest.IdAcademicYear),
        //                    nameof(GetStudentStatusListByAYRequest.Semester)
        //                    );

        //    var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => true);

        //    if (!string.IsNullOrWhiteSpace(param.SearchStudentKeyword))
        //        predicate = predicate.And(x
        //            => EF.Functions.Like(x.Student.Id, $"%{param.SearchStudentKeyword}%")
        //            || EF.Functions.Like(
        //                (string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName) +
        //                (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName) + 
        //                (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName)
        //                , $"%{param.SearchStudentKeyword}%")
        //            );

        //    var getStudentListQuery = _dbContext.Entity<MsHomeroomStudent>()
        //                             .Include(x => x.Homeroom)
        //                                 .ThenInclude(x => x.Grade)
        //                                 .ThenInclude(x => x.MsLevel)
        //                                 .ThenInclude(x => x.MsAcademicYear)
        //                                 .ThenInclude(x => x.MsSchool)
        //                             .Include(x => x.Homeroom)
        //                                 .ThenInclude(x => x.MsGradePathwayClassroom)
        //                                 .ThenInclude(x => x.Classroom)
        //                             .Include(x => x.Student)
        //                             .Where(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear &&
        //                                         x.Semester == param.Semester &&
        //                                         (string.IsNullOrEmpty(param.IdLevel) ? true : x.Homeroom.Grade.MsLevel.Id == param.IdLevel) &&
        //                                         (string.IsNullOrEmpty(param.IdGrade) ? true : x.Homeroom.Grade.Id == param.IdGrade) &&
        //                                         (string.IsNullOrEmpty(param.IdHomeroom) ? true : x.Homeroom.Id == param.IdHomeroom)
        //                                         )
        //                             .Where(predicate)
        //                             .Join(_dbContext.Entity<TrStudentStatus>()
        //                                     .Include(x => x.StudentStatus)
        //                                     .Where(x => x.IdAcademicYear == param.IdAcademicYear)
        //                                     .OrderByDescending(x => x.StartDate),
        //                                     homeroomStudent => homeroomStudent.IdStudent,
        //                                     studentStatus => studentStatus.IdStudent,
        //                                     (homeroomStudent, studentStatus) => new { homeroomStudent, studentStatus })
        //                             .GroupBy(x => new
        //                             {
        //                                 IdAcademicYear = x.studentStatus.IdAcademicYear,
        //                                 IdStudent = x.homeroomStudent.Student.Id,
        //                                 StudentFirstName = x.homeroomStudent.Student.FirstName,
        //                                 StudentMiddleName = x.homeroomStudent.Student.MiddleName,
        //                                 StudentLastName = x.homeroomStudent.Student.LastName,
        //                                 IdGrade = x.homeroomStudent.Homeroom.Grade.Id,
        //                                 IdHomeroom = x.homeroomStudent.Homeroom.Id,
        //                                 GradeDesc = x.homeroomStudent.Homeroom.Grade.Description,
        //                                 ClassroomDesc = x.homeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Description
        //                             })
        //                             .OrderBy(x => x.Key.GradeDesc)
        //                             .ThenBy(x => x.Key.ClassroomDesc)
        //                             .ThenBy(x => x.Key.StudentFirstName)
        //                             .ThenBy(x => x.Key.StudentMiddleName)
        //                             .ThenBy(x => x.Key.StudentLastName);

        //    var getStudentList = getStudentListQuery
        //                            .OrderByDynamic(param, _aliasColumns)
        //                            .SetPagination(param)
        //                            .Select(x => new
        //                            {
        //                                IdAcademicYear = x.Key.IdAcademicYear,
        //                                Student = new NameValueVm
        //                                {
        //                                    Id = x.Key.IdStudent,
        //                                    Name = NameUtil.GenerateFullName(x.Key.StudentFirstName, x.Key.StudentMiddleName, x.Key.StudentLastName)
        //                                },
        //                                IdGrade = x.Key.IdGrade,
        //                                Homeroom = new ItemValueVm
        //                                {
        //                                    Id = x.Key.IdHomeroom,
        //                                    Description = x.Key.GradeDesc + x.Key.ClassroomDesc
        //                                },
        //                                LatestStudentStatusStartDate = x.Max(y => y.studentStatus.StartDate)
        //                            })
        //                            .ToList();

        //    var getStudentListLatestStatus = getStudentList
        //                                        .Join(_dbContext.Entity<TrStudentStatus>()
        //                                                .Include(x => x.StudentStatus)
        //                                                .OrderByDescending(x => x.UserIn),
        //                                                studentStatus => new { IdAcademicYear = studentStatus.IdAcademicYear, IdStudent = studentStatus.Student.Id, StartDate = studentStatus.LatestStudentStatusStartDate },
        //                                                latestStatusHelper => new { latestStatusHelper.IdAcademicYear, latestStatusHelper.IdStudent, latestStatusHelper.StartDate },
        //                                                (x, latestStatusHelper) => new
        //                                                {
        //                                                    IdAcademicYear = x.IdAcademicYear,
        //                                                    Student = x.Student,
        //                                                    IdGrade = x.IdGrade,
        //                                                    Homeroom = x.Homeroom,
        //                                                    LatestStudentStatus = new ItemValueVm
        //                                                    {
        //                                                        Id = latestStatusHelper.IdStudentStatus.ToString(),
        //                                                        Description = latestStatusHelper.StudentStatus.LongDesc
        //                                                    },
        //                                                    LatestStudentStatusStartDate = x.LatestStudentStatusStartDate
        //                                                })
        //                                        .ToList();

        //    var getStudentListAndPeriod = getStudentListLatestStatus
        //                                    .Join(_dbContext.Entity<MsPeriod>()
        //                                            .GroupBy(x => x.IdGrade)
        //                                            .Select(x => new
        //                                            {
        //                                                IdGrade = x.Key,
        //                                                MinStartDate = x.Min(x => x.StartDate),
        //                                                MaxEndDate = x.Max(x => x.EndDate),
        //                                            }),
        //                                            studentStatus => studentStatus.IdGrade,
        //                                            period => period.IdGrade,
        //                                            (studentStatus, period) => new GetStudentStatusListByAYResult
        //                                            {
        //                                                Student = studentStatus.Student,
        //                                                Homeroom = studentStatus.Homeroom,
        //                                                LatestStudentStatus = studentStatus.LatestStudentStatus,
        //                                                LatestStudentStatusStartDate = studentStatus.LatestStudentStatusStartDate,
        //                                                StartDateAY = period.MinStartDate,
        //                                                EndDateAY = period.MaxEndDate
        //                                            })
        //                                    .ToList();

        //    var count = param.CanCountWithoutFetchDb(getStudentListAndPeriod.Count)
        //        ? getStudentListAndPeriod.Count
        //        : await getStudentListQuery.Select(x => x.Key.IdStudent).CountAsync(CancellationToken);

        //    #region unused code
        //    //var getStudentListQuery = _dbContext.Entity<MsHomeroomStudent>()
        //    //                         .Include(x => x.Homeroom)
        //    //                             .ThenInclude(x => x.Grade)
        //    //                             .ThenInclude(x => x.MsLevel)
        //    //                             .ThenInclude(x => x.MsAcademicYear)
        //    //                             .ThenInclude(x => x.MsSchool)
        //    //                         .Include(x => x.Homeroom)
        //    //                             .ThenInclude(x => x.MsGradePathwayClassroom)
        //    //                             .ThenInclude(x => x.Classroom)
        //    //                         .Include(x => x.Student)
        //    //                         .Where(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear &&
        //    //                                     x.Semester == param.Semester &&
        //    //                                     (string.IsNullOrEmpty(param.IdLevel) ? true : x.Homeroom.Grade.MsLevel.Id == param.IdLevel) &&
        //    //                                     (string.IsNullOrEmpty(param.IdGrade) ? true : x.Homeroom.Grade.Id == param.IdGrade) &&
        //    //                                     (string.IsNullOrEmpty(param.IdHomeroom) ? true : x.Homeroom.Id == param.IdHomeroom)
        //    //                                     )
        //    //                         .Where(predicate)
        //    //                         .Join(_dbContext.Entity<TrStudentStatus>()
        //    //                                 .Include(x => x.StudentStatus)
        //    //                                 .Where(x => x.IdAcademicYear == param.IdAcademicYear)
        //    //                                 .OrderByDescending(x => x.StartDate),
        //    //                                 homeroomStudent => homeroomStudent.IdStudent,
        //    //                                 studentStatus => studentStatus.IdStudent,
        //    //                                 (homeroomStudent, studentStatus) => new { homeroomStudent, studentStatus })
        //    //                         .GroupBy(x => new { x.studentStatus.IdStudent, x.studentStatus.IdAcademicYear }, (key, y) => y.OrderByDescending(a => a.studentStatus.StartDate).ThenByDescending(a => a.studentStatus.DateIn).First())
        //    //                         .OrderBy(x => x.homeroomStudent.Homeroom.Grade.Description)
        //    //                         .ThenBy(x => x.homeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Description)
        //    //                         .ThenBy(x => x.homeroomStudent.Student.FirstName)
        //    //                         .ThenBy(x => x.homeroomStudent.Student.MiddleName)
        //    //                         .ThenBy(x => x.homeroomStudent.Student.LastName);

        //    //var getStudentList = getStudentListQuery
        //    //                        .OrderByDynamic(param, _aliasColumns)
        //    //                        .SetPagination(param)
        //    //                        .Select(x => new 
        //    //                        {
        //    //                            IdAcademicYear = x.studentStatus.IdAcademicYear,
        //    //                            Student = new NameValueVm
        //    //                            {
        //    //                                Id = x.homeroomStudent.IdStudent,
        //    //                                Name = NameUtil.GenerateFullName(x.homeroomStudent.Student.FirstName, x.homeroomStudent.Student.MiddleName, x.homeroomStudent.Student.LastName)
        //    //                            },
        //    //                            IdGrade = x.homeroomStudent.Homeroom.IdGrade,
        //    //                            Homeroom = new ItemValueVm
        //    //                            {
        //    //                                Id = x.homeroomStudent.IdHomeroom,
        //    //                                Description = x.homeroomStudent.Homeroom.Grade.Description + x.homeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Description
        //    //                            },
        //    //                            LatestStudentStatus = new ItemValueVm
        //    //                            {
        //    //                                Id = x.studentStatus.IdStudentStatus.ToString(),
        //    //                                Description = x.studentStatus.StudentStatus.LongDesc
        //    //                            },
        //    //                            LatestStudentStatusStartDate = x.studentStatus.StartDate
        //    //                        })
        //    //                        .ToList();

        //    //var getStudentListAndPeriod = getStudentList
        //    //                                .Join(_dbContext.Entity<MsPeriod>()
        //    //                                        .GroupBy(x => x.IdGrade)
        //    //                                        .Select(x => new
        //    //                                        {
        //    //                                            IdGrade = x.Key,
        //    //                                            MinStartDate = x.Min(x => x.StartDate),
        //    //                                            MaxEndDate = x.Max(x => x.EndDate),
        //    //                                        }),
        //    //                                        studentStatus => studentStatus.IdGrade,
        //    //                                        period => period.IdGrade,
        //    //                                        (studentStatus, period) => new GetStudentStatusListByAYResult
        //    //                                        {
        //    //                                            Student = studentStatus.Student,
        //    //                                            Homeroom = studentStatus.Homeroom,
        //    //                                            LatestStudentStatus = studentStatus.LatestStudentStatus,
        //    //                                            LatestStudentStatusStartDate = studentStatus.LatestStudentStatusStartDate,
        //    //                                            StartDateAY = period.MinStartDate,
        //    //                                            EndDateAY = period.MaxEndDate
        //    //                                        })
        //    //                                .ToList();

        //    //var count = param.CanCountWithoutFetchDb(getStudentListAndPeriod.Count)
        //    //    ? getStudentListAndPeriod.Count
        //    //    : await getStudentListQuery.Select(x => x.homeroomStudent.IdStudent).CountAsync(CancellationToken);
        //    #endregion

        //    return Request.CreateApiResult2(getStudentListAndPeriod as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        //}
        #endregion
    }
}
