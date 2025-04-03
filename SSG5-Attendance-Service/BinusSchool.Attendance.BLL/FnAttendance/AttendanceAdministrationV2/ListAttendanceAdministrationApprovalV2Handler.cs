using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class ListAttendanceAdministrationApprovalV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public ListAttendanceAdministrationApprovalV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListAttendanceAdministrationApprovalV2Request>(nameof(GetListAttendanceAdministrationApprovalV2Request.IdAcademicYear),nameof(GetListAttendanceAdministrationApprovalV2Request.Semester));
            var columns = new[] { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "Student", "StartDate", "EndDate", "AttendanceCategory", "Status" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0]   , "AcademicYear" },
                { columns[1]   , "Semester"},
                { columns[2]   , "Level"},
                { columns[3]   , "Grade"},
                { columns[4]   , "Homeroom"},
                { columns[5]   , "Student"},
                { columns[6]   , "StartDate"},
                { columns[7]   , "EndDate"},
                { columns[8]   , "AttendanceCategory"},
                { columns[9]   , "Status"},
            };
            var predicate = PredicateBuilder.Create<TrAttendanceAdministration>(x => x.StudentGrade.Grade.Level.IdAcademicYear == param.IdAcademicYear && x.NeedValidation == true);

            predicate = predicate.And(x => x.StudentGrade.Student.HomeroomStudents.Any(y => y.Homeroom.Semester == param.Semester));
            
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.StudentGrade.Grade.Level.Id == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.StudentGrade.Grade.Id == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.StudentGrade.Student.HomeroomStudents.Any(y => y.IdHomeroom == param.IdHomeroom));

            if (param.AttendanceCategory != null)
                predicate = predicate.And(x => x.Attendance.AttendanceCategory == param.AttendanceCategory);

            if (param.Status != null)
            {
                predicate = predicate.And(x => x.NeedValidation == true && x.StatusApproval == 0);
            }

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                EF.Functions.Like(x.StudentGrade.Student.Id, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.FirstName, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.LastName, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.MiddleName, param.SearchPattern())
                );



            var query = _dbContext.Entity<TrAttendanceAdministration>()
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.HomeroomStudents)
                            .ThenInclude(x => x.Homeroom)
                                .ThenInclude(x => x.GradePathwayClassroom)
                                    .ThenInclude(x => x.Classroom)
                .Include(x => x.Attendance)
               .Where(predicate)
               .Where(x => x.StudentGrade.Grade.Level.IdAcademicYear == param.IdAcademicYear);
            //.OrderByDynamic(param, aliasColumns);
            query = param.OrderBy switch
            {
                "AcademicYear" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length).ThenBy(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length)
                        : query.OrderByDescending(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length).ThenByDescending(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length),
                "Student" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.IdStudent.Length).ThenBy(x => x.StudentGrade.IdStudent)
                        : query.OrderByDescending(x => x.StudentGrade.IdStudent.Length).ThenByDescending(x => x.StudentGrade.IdStudent),
                "StartDate" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StartDate)
                        : query.OrderByDescending(x => x.StartDate),
                "EndDate" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.EndDate)
                        : query.OrderByDescending(x => x.EndDate),
                _ => query.OrderByDynamic(param, aliasColumns)
            };
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.StudentGrade.Student.FirstName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetListAttendanceAdministrationApprovalV2Result
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.Level.IdAcademicYear,
                            Code = x.StudentGrade.Grade.Level.AcademicYear.Code,
                            Description = x.StudentGrade.Grade.Level.AcademicYear.Description,
                        },
                        Semester = param.Semester,
                        Level = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.Level.Id,
                            Code = x.StudentGrade.Grade.Level.Code,
                            Description = x.StudentGrade.Grade.Level.Description
                        },
                        Grade = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.Id,
                            Code = x.StudentGrade.Grade.Code,
                            Description = x.StudentGrade.Grade.Description
                        },
                        ClassHomeroom = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == param.IdAcademicYear).IdHomeroom,
                            Code = x.StudentGrade.Grade.Code,
                            Description = x.StudentGrade.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == param.IdAcademicYear && y.Homeroom.Semester == param.Semester).Homeroom.GradePathwayClassroom.Classroom.Description,
                        },
                        Student = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Student.Id,
                            Code = string.Format("{0} {1} {2}",
                                x.StudentGrade.Student.FirstName,
                                x.StudentGrade.Student.MiddleName,
                                x.StudentGrade.Student.LastName
                            )
                        },
                        Attendance = new CodeWithIdVm
                        {
                            Id = x.Attendance.Id,
                            Code = x.Attendance.Code,
                            Description = x.Attendance.Description
                        },
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = x.StatusApproval == 1 ? "Approved" : x.StatusApproval == 2 ? "Declined" : "On Review" ,
                        CanApprove = x.NeedValidation == false ? false : x.NeedValidation == true && x.StatusApproval == 0 ? true : false
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
