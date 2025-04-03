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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class ReportStudentToGcByCounselorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public ReportStudentToGcByCounselorHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetReportStudentToGcRequest>();
            string[] _columns = { "AcademicYear", "StudentName", "BinusanId","ReportedBy" , "ReportStudentDate", "ReportStudentNote", "Read" };

            if (param.EndDate < param.StartDate)
                throw new BadRequestException("Report student to GC with start date: " + param.StartDate + " and end date: " + param.EndDate + " are wrong number.");

            var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                             .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                             .Where(e => e.IdUser == param.IdUser &&
                                     e.MsNonTeachingLoad.IdAcademicYear == param.IdAcademicYear &&
                                     !string.IsNullOrEmpty(e.Data) &&
                                     (e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal)
                                     )
                             .ToListAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<TrGcReportStudent>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdUserCounselor == param.IdUser);
            var isNonTeachingLoad = false;
            if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal))
            {
                if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).Count() > 0)
                {
                    predicate = PredicateBuilder.Create<TrGcReportStudent>(x => x.IdAcademicYear == param.IdAcademicYear);
                    isNonTeachingLoad = true;
                }
            }

            var query = _dbContext.Entity<TrGcReportStudent>()
                    .Include(e => e.Student)
                    .Include(e => e.UserReport)
                    .Include(e => e.AcademicYear)
                    .Where(predicate)
                    .Select(e => new
                    {
                        id = e.Id,
                        AcademicYear = e.AcademicYear.Description,
                        StudentName = (e.Student.FirstName == null ? "" : e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                        BinusanId = e.IdStudent,
                        ReportedBy = e.UserReport.DisplayName,
                        ReportStudentDate = e.Date,
                        ReportStudentNote = e.Note,
                        IsRead = e.IsRead

                    });

            //filter
            if ((!string.IsNullOrEmpty(param.StartDate.ToString())) && (!string.IsNullOrEmpty(param.EndDate.ToString())))
                query = query.Where(x => x.ReportStudentDate.Date >= Convert.ToDateTime(param.StartDate).Date && x.ReportStudentDate.Date <= Convert.ToDateTime(param.EndDate).Date);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.ReportedBy.Contains(param.Search) || x.StudentName.Contains(param.Search));

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "BinusanId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusanId)
                        : query.OrderBy(x => x.BinusanId);
                    break;
                case "ReportedBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportedBy)
                        : query.OrderBy(x => x.ReportedBy);
                    break;
                case "ReportStudentDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportStudentDate)
                        : query.OrderBy(x => x.ReportStudentDate);
                    break;
                case "ReportStudentNote":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportStudentNote)
                        : query.OrderBy(x => x.ReportStudentNote);
                    break;
                case "Read":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IsRead)
                        : query.OrderBy(x => x.IsRead);
                    break;
            };

            IReadOnlyList<object> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReportStudentToGcByCounsolorResult
                {
                    Id = x.id,
                    AcademicYear = x.AcademicYear,
                    StudentName = x.StudentName,
                    BinusanId = x.BinusanId,
                    ReportedBy = x.ReportedBy,
                    ReportStudentDate = x.ReportStudentDate,
                    ReportStudentNote = x.ReportStudentNote,
                    IsRead = x.IsRead,
                    IsCounselor = isNonTeachingLoad?false:true
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReportStudentToGcByCounsolorResult
                {
                    Id = x.id,
                    AcademicYear = x.AcademicYear,
                    StudentName = x.StudentName,
                    BinusanId = x.BinusanId,
                    ReportedBy = x.ReportedBy,
                    ReportStudentDate = x.ReportStudentDate,
                    ReportStudentNote = x.ReportStudentNote,
                    IsRead = x.IsRead,
                    IsCounselor = isNonTeachingLoad ? false : true
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

    }
}
