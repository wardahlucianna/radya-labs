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
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.OpenApi.Validations.Rules;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class AttendanceRecapHandler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceRecapHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAttendanceRecapRequest>();

            var columns = new[] { "academicyear", "studentname", "studentid", "homeroom", "unsubmitted", "pending", "present", "late", "excusedabsence", "unexcusedabsence" };

            var predicate = PredicateBuilder.Create<TrAttendanceSummaryTerm>(x => x.IdSchool == param.IdSchool);

            var trAttendanceSummaryTerm = _dbContext.Entity<TrAttendanceSummaryTerm>()
                        .Include(x => x.Grade)
                        .Include(x => x.Level)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.GradePathwayClassroom)
                                .ThenInclude(x => x.Classroom)
                        .Include(x => x.Student)
                        .Include(x => x.AcademicYear)
                        .Where(predicate)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                trAttendanceSummaryTerm = trAttendanceSummaryTerm.Where(x => x.IdAcademicYear == param.IdAcademicYear);
            }
            if (!string.IsNullOrEmpty(param.IdLevel))
            {
                trAttendanceSummaryTerm = trAttendanceSummaryTerm.Where(x => x.IdLevel == param.IdLevel);
            }
            if (!string.IsNullOrEmpty(param.IdGrade))
            {
                trAttendanceSummaryTerm = trAttendanceSummaryTerm.Where(x => x.IdGrade == param.IdGrade);
            }
            if (!string.IsNullOrEmpty(param.IdHomeroom))
            {
                trAttendanceSummaryTerm = trAttendanceSummaryTerm.Where(x => x.Homeroom.GradePathwayClassroom.Classroom.Id == param.IdHomeroom);
            }
            if (!string.IsNullOrEmpty(param.IdStudent))
            {
                trAttendanceSummaryTerm = trAttendanceSummaryTerm.Where(x => x.IdStudent == param.IdStudent);
            }

            var data = trAttendanceSummaryTerm.Select(x => new GetAttendanceRecapResult
            {
                IdAcademicYear = x.IdAcademicYear,
                AcademicYear = x.AcademicYear.Description,
                IdStudent = x.IdStudent,
                StudentName = $"{NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)}",
                IdHomeroom = x.Homeroom.GradePathwayClassroom.Classroom.Id,
                Homeroom = $"{x.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                IdLevel = x.IdLevel,
                IdGrade = x.IdGrade
            }).Distinct().ToList();

            foreach (var item in data)
            {
                item.UnSubmitted = trAttendanceSummaryTerm.Where(x => x.IdStudent == item.IdStudent && x.Homeroom.GradePathwayClassroom.Classroom.Id == item.IdHomeroom && x.AttendanceWorkhabitName.ToLower() == AttendanceEntryStatus.Unsubmitted.GetDescription().ToLower()).Sum(x => x.Total);
                item.Pending = trAttendanceSummaryTerm.Where(x => x.IdStudent == item.IdStudent && x.Homeroom.GradePathwayClassroom.Classroom.Id == item.IdHomeroom && x.AttendanceWorkhabitName.ToLower() == AttendanceEntryStatus.Pending.GetDescription().ToLower()).Sum(x => x.Total);
                item.Present = trAttendanceSummaryTerm.Where(x => x.IdStudent == item.IdStudent && x.Homeroom.GradePathwayClassroom.Classroom.Id == item.IdHomeroom && x.AttendanceWorkhabitName.ToLower() == "present").Sum(x => x.Total);
                item.Late = trAttendanceSummaryTerm.Where(x => x.IdStudent == item.IdStudent && x.Homeroom.GradePathwayClassroom.Classroom.Id == item.IdHomeroom && x.AttendanceWorkhabitName.ToLower() == "late").Sum(x => x.Total);
                item.UnexcusedAbsence = trAttendanceSummaryTerm.Where(x => x.IdStudent == item.IdStudent && x.Homeroom.GradePathwayClassroom.Classroom.Id == item.IdHomeroom && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceCategory && x.AttendanceWorkhabitName.ToLower() == "unexcused").Sum(x => x.Total);
                item.ExcusedAbsence = trAttendanceSummaryTerm.Where(x => x.IdStudent == item.IdStudent && x.Homeroom.GradePathwayClassroom.Classroom.Id == item.IdHomeroom && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceCategory && x.AttendanceWorkhabitName.ToLower() == "excused").Sum(x => x.Total);
            }

            //searching
            if (!string.IsNullOrEmpty(param.Search))
            {
                data = data.Where(x => x.StudentName.ToLower().Contains(param.Search.ToLower()) || x.IdStudent.ToLower().Contains(param.Search.ToLower())).ToList();
            }
            //sorting
            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                switch (param.OrderBy.ToLower())
                {
                    case "academicyear":
                        data = data = param.OrderType == OrderType.Desc
                            ? data.OrderByDescending(x => x.IdAcademicYear).ToList()
                            : data.OrderBy(x => x.IdAcademicYear).ToList();
                        break;
                    case "studentname":
                        data = param.OrderType == OrderType.Desc
                            ? data.OrderByDescending(x => x.StudentName).ToList()
                            : data.OrderBy(x => x.StudentName).ToList();
                        break;
                    case "studentid":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.IdStudent).ToList()
                           : data.OrderBy(x => x.IdStudent).ToList();
                        break;
                    case "homeroom":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.Homeroom).ToList()
                           : data.OrderBy(x => x.Homeroom).ToList();
                        break;
                    case "unsubmitted":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.UnSubmitted).ToList()
                           : data.OrderBy(x => x.UnSubmitted).ToList();
                        break;
                    case "pending":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.Pending).ToList()
                           : data.OrderBy(x => x.Pending).ToList();
                        break;
                    case "present":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.Present).ToList()
                           : data.OrderBy(x => x.Present).ToList();
                        break;
                    case "late":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.Late).ToList()
                           : data.OrderBy(x => x.Late).ToList();
                        break;
                    case "excusedabsence":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.ExcusedAbsence).ToList()
                           : data.OrderBy(x => x.ExcusedAbsence).ToList();
                        break;
                    case "unexcusedabsence":
                        data = data = param.OrderType == OrderType.Desc
                           ? data.OrderByDescending(x => x.UnexcusedAbsence).ToList()
                           : data.OrderBy(x => x.UnexcusedAbsence).ToList();
                        break;
                }
            }

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = data
                    .ToList();

                items = result.ToList();
            }
            else
            {
                var result = data
                    .SetPagination(param)
                    .ToList();

                items = result.ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
             ? items.Count
             : data.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
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
