using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentPoint;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentPoint
{
    public class GetStudentPointSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _studentDbContext;

        public GetStudentPointSummaryHandler(IStudentDbContext studentDbContext)
        {
            _studentDbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            //var param = Request.ValidateParams<GetStudentPointSummaryRequest>(
            //    nameof(GetStudentPointSummaryRequest.IdAcadyear),
            //    nameof(GetStudentPointSummaryRequest.IdStudent));
            //var meritPoint = await _studentDbContext.Entity<TrStudentPoint>()
            //    .Include(x => x.EntryMeritStudent)
            //        .ThenInclude(x => x.HomeroomStudent)
            //            .ThenInclude(x => x.Homeroom)
            //                .ThenInclude(x => x.MsGradePathwayClassroom)
            //                    .ThenInclude(x => x.GradePathway)
            //                        .ThenInclude(x => x.Grade)
            //                            .ThenInclude(x => x.MsLevel)
            //                                .ThenInclude(x => x.MsAcademicYear)
            //    .Where(x => x.EntryDemeritStudent.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear)
            //    .Where(x => x.IdHomeroomStudent == param.IdStudent)
            //    .GroupBy(x => x.IdHomeroomStudent)
            //    .Select(x => x.Sum(y => y.MeritPoint))
            //    .FirstOrDefaultAsync();
            //var DemeritPoint = await _studentDbContext.Entity<TrStudentPoint>()
            //    .Include(x => x.EntryDemeritStudent)
            //        .ThenInclude(x => x.HomeroomStudent)
            //            .ThenInclude(x => x.Homeroom)
            //                .ThenInclude(x => x.MsGradePathwayClassroom)
            //                    .ThenInclude(x => x.GradePathway)
            //                        .ThenInclude(x => x.Grade)
            //                            .ThenInclude(x => x.MsLevel)
            //                                .ThenInclude(x => x.MsAcademicYear)
            //    .Where(x => x.EntryDemeritStudent.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear)
            //    .Where(x => x.IdHomeroomStudent == param.IdStudent)
            //    .GroupBy(x => x.IdHomeroomStudent)
            //    .Select(x => x.Sum(y => y.MeritPoint))
            //    .FirstOrDefaultAsync();
            //var result = new GetStudentPointSummaryResult
            //{
            //    MeritPoint = meritPoint,
            //    DemeritPoint = DemeritPoint
            //};
            //return Request.CreateApiResult2(result as object);
            throw new NotImplementedException();
        }
    }
}
