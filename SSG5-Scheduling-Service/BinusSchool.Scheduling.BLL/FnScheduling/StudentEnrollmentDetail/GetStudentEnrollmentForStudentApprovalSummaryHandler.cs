using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollmentDetail
{
    public class GetStudentEnrollmentForStudentApprovalSummaryHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;
        //private readonly IApiService<IGrade> _gradeService;

        public GetStudentEnrollmentForStudentApprovalSummaryHandler(ISchedulingDbContext dbContext
            )
        {
            _dbContext = dbContext;
            //_gradeService = gradeService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = await Request.GetBody<GetStudentEnrollmentforStudentApprovalSummaryRequest>();

            var data = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(x => x.Homeroom)
                                        .ThenInclude(a => a.Grade)
                                        .ThenInclude(b => b.Level)
                                        .ThenInclude(c => c.AcademicYear)
                                        .ThenInclude(a => a.School)
                                        .Include(x => x.Student)
                                        .Where(x => x.Homeroom.Grade.Level.AcademicYear.School.Id == param.SchoolId &&
                                                    (string.IsNullOrEmpty(param.AcademicYearId) ? true : x.Homeroom.Grade.Level.AcademicYear.Id == param.AcademicYearId) &&
                                                    (string.IsNullOrEmpty(param.GradeId) ? true : x.Homeroom.Grade.Id == param.GradeId) &&
                                                    (string.IsNullOrEmpty(param.PathwayID) ? true : x.Homeroom.Id == param.PathwayID))
                                        .OrderByDescending(p => p.Homeroom.DateIn)
                                        .Select(p => new GetStudentEnrollmentforStudentApprovalSummaryResult
                                        {
                                            StudentId = p.IdStudent,
                                            AcademicYearId = p.Homeroom.IdAcademicYear
                                        }).ToListAsync();

            return Request.CreateApiResult2(data as object);
        }

    }
}
