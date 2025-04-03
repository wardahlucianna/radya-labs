using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentPoint;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentPoint
{
    public class GetDetailStudentMeritDemeritPointHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _studentDbContext;

        public GetDetailStudentMeritDemeritPointHandler(IStudentDbContext studentDbContext)
        {
            _studentDbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            //var param = Request.ValidateParams<GetDetailStudentMeritDemeritPointRequest>(
            //   nameof(GetDetailStudentMeritDemeritPointRequest.IdAcadyear),
            //   nameof(GetDetailStudentMeritDemeritPointRequest.IdStudent));
            //var predicate = PredicateBuilder.Create<TrStudentPoint>(x => 1 == 1
            //&& x.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear
            //&& x.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear
            //&& x.IdHomeroomStudent == param.IdStudent
            //);
            //if (param.Semester != null)
            //    predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Semester == param.Semester && x.HomeroomStudent.Homeroom.Semester == param.Semester);
            //var acadYearDetail = await _studentDbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcadyear)
            //    .Select(x => new CodeWithIdVm
            //    {
            //        Id = x.Id,
            //        Code = x.Code,
            //        Description = x.Description,
            //    }).FirstOrDefaultAsync();
            //var homeroomTeacher = await _studentDbContext.Entity<MsHomeroomTeacher>()
            //    .Include(x => x.Staff)
            //    .Include(x => x.Homeroom)
            //        .ThenInclude(x => x.HomeroomStudents)
            //    .Include(x => x.Homeroom)
            //        .ThenInclude(x => x.MsGradePathwayClassroom)
            //            .ThenInclude(x => x.GradePathway)
            //                .ThenInclude(x => x.Grade)
            //                    .ThenInclude(x => x.MsLevel)
            //    .Where(x => x.Homeroom.HomeroomStudents.Any(y => y.IdStudent == param.IdStudent))
            //    .Where(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear)
            //    .Select(x => x.Staff.FirstName + " " + x.Staff.LastName)
            //    .ToListAsync(CancellationToken);
            //var dataDemerit = await _studentDbContext.Entity<TrStudentPoint>()
            //  .Include(x => x.HomeroomStudent)
            //          .ThenInclude(x => x.Homeroom)
            //              .ThenInclude(x => x.MsGradePathwayClassroom)
            //                  .ThenInclude(x => x.GradePathway)
            //                      .ThenInclude(x => x.Grade)
            //                          .ThenInclude(x => x.MsLevel)
            //                              .ThenInclude(x => x.MsAcademicYear)
            //    .Include(x => x.HomeroomStudent)
            //            .ThenInclude(x => x.Student)
            //  .Include(x => x.LevelOfInteraction)
            //  .Include(x => x.SanctionMapping)
            //  .Where(x => x.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear)
            //  .Where(x => x.IdHomeroomStudent == param.IdStudent)
            //  .Select(x => new
            //  {
            //      x.HomeroomStudent.IdStudent,
            //      x.LevelOfInteraction.NameLevelOfInteraction,
            //      x.SanctionMapping.SanctionName,
            //      x.DemeritPoint,
            //      //x.EntryDemeritStudent.DateIn,
            //      //x.EntryDemeritStudent.MeritDemeritMapping.DisciplineName,
            //      //x.EntryDemeritStudent.Note,
            //      //x.EntryDemeritStudent.Point
            //  })
            //  .ToListAsync(CancellationToken);

            //var dataMerit = await _studentDbContext.Entity<TrStudentPoint>()
            //  .Include(x => x.EntryMeritStudent)
            //      .ThenInclude(x => x.HomeroomStudent)
            //          .ThenInclude(x => x.Homeroom)
            //              .ThenInclude(x => x.MsGradePathwayClassroom)
            //                  .ThenInclude(x => x.GradePathway)
            //                      .ThenInclude(x => x.Grade)
            //                          .ThenInclude(x => x.MsLevel)
            //                              .ThenInclude(x => x.MsAcademicYear)
            //  .Where(x => x.EntryDemeritStudent.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcadyear)
            //  .Where(x => x.IdHomeroomStudent == param.IdStudent)
            //  .Select(x => new
            //  {
            //      x.HomeroomStudent.IdStudent,
            //      x.MeritPoint,
            //      x.EntryDemeritStudent.DateIn,
            //      x.EntryDemeritStudent.MeritDemeritMapping.DisciplineName,
            //      x.EntryDemeritStudent.Note,
            //      x.EntryDemeritStudent.Point
            //  })
            //  .ToListAsync(CancellationToken);

            //var result = new GetDetailStudentMeritDemeritPointResult
            //{
            //    Acadyear = acadYearDetail,
            //    DemeritPoint = dataDemerit
            //    .GroupBy(x => new
            //    {
            //        x.IdStudent
            //    })
            //    .Select(x => x.Sum(x => x.DemeritPoint))
            //    .FirstOrDefault(),
            //    MeritPoint = dataMerit
            //    .GroupBy(x => new
            //    {
            //        x.IdStudent
            //    })
            //    .Select(x => x.Sum(x => x.MeritPoint))
            //    .FirstOrDefault(),
            //    LevelOfInteraction = dataDemerit
            //    .GroupBy(x => new
            //    {
            //        x.NameLevelOfInteraction
            //    })
            //    .Select(x => x.Key.NameLevelOfInteraction)
            //    .FirstOrDefault(),
            //    Sanction = dataDemerit
            //    .GroupBy(x => new
            //    {
            //        x.SanctionName
            //    })
            //    .Select(x => x.Key.SanctionName)
            //    .FirstOrDefault(),
            //    DetailDemeritPoint = dataDemerit
            //    .Select(x => new PointDetailResult
            //    {
            //        Date = x.DateIn.Value,
            //        DiciplineName = x.DisciplineName,
            //        //Note = x.Note,
            //        Point = x.Point,
            //        TeacherName = string.Join(",", homeroomTeacher.Select(x => x))
            //    })
            //    .ToList(),
            //    DetailMeritPoint = dataMerit
            //    .Select(x => new PointDetailResult
            //    {
            //        Date = x.DateIn.Value,
            //        DiciplineName = x.DisciplineName,
            //        Note = x.Note,
            //        Point = x.Point,
            //        TeacherName = string.Join(",", homeroomTeacher.Select(x => x))
            //    })
            //    .ToList(),
            //};
            //return Request.CreateApiResult2(result as object);
            throw new NotImplementedException();
        }
    }
}
