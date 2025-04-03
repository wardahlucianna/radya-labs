using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularStudentScoreHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetExtracurricularStudentScoreHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularStudentScoreRequest>(nameof(GetExtracurricularStudentScoreRequest.IdAcademicYear), nameof(GetExtracurricularStudentScoreRequest.Semester), nameof(GetExtracurricularStudentScoreRequest.IdExtracurricular));

            var getExtracurricularScoreComp = await _dbContext.Entity<MsExtracurricularScoreCompMapping>()
                        .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                        .Where(x => x.ExtracurricularScoreCompCategory.IdAcademicYear == param.IdAcademicYear)
                        .Select(x => new {
                            CalculationType = x.ExtracurricularScoreCompCategory.ExtracurricularScoreCalculationType.CalculationType,
                            ScoreGrades = x.ExtracurricularScoreCompCategory.ExtracurricularScoreCalculationType.ExtracurricularScoreGrades
                                    .Select(y => new { 
                                        MinScore = y.MinScore,
                                        MaxScore = y.MaxScore,
                                        Grade = y.Grade
                                    })
                                    .ToList(),
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            var extracurricularData = await _dbContext.Entity<MsExtracurricular>().Include(x => x.ExtracurricularGradeMappings).ThenInclude(y => y.Grade)
                                    .Where(a => a.Id == param.IdExtracurricular).FirstOrDefaultAsync();
            if (extracurricularData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["extracurricular"], "Id", param.IdExtracurricular));

            var ScoreComponent = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                                                .Include(x => x.ExtracurricularScoreCompCategory)
                                                .ThenInclude(y => y.ExtracurricularScoreCompMappings)
                                                .Where(a => a.IdAcademicYear == param.IdAcademicYear
                                                && a.ExtracurricularScoreCompCategory.ExtracurricularScoreCompMappings.Any(b => b.IdExtracurricular == param.IdExtracurricular))
                                                .OrderBy(a => a.OrderNumber)
                                                .ToListAsync();

            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                .Include(x => x.Extracurricular)
                .Include(x => x.Student)
                .Where(x => x.IdExtracurricular == param.IdExtracurricular && x.Extracurricular.Semester == param.Semester)
                .Select(x => new
                {
                    x.IdExtracurricular,
                    x.Extracurricular.Semester,
                    x.Extracurricular.Name,
                    x.IdStudent,
                    x.Student.FirstName,
                    x.Student.LastName
                }).Distinct()
                .ToListAsync(CancellationToken);


            var StudentScoreComponent = getExtracurricularParticipant
                         .SelectMany(t1 => ScoreComponent, (t1, t2) => new GetExtracurricularStudentScoreVm
                         {
                             ExtracurricularName = t1.Name,
                             IdStudent = t1.IdStudent,
                             StudentName = String.Format("{0} {1}", t1.FirstName, t1.LastName),
                             OrderNumberComponent = t2.OrderNumber,
                             idScoreComponent = t2.Id,
                             ScoreComponentName = t2.Description
                         }).ToList()
                         .GroupJoin(_dbContext.Entity<TrExtracurricularScoreEntry>().Include(a => a.ExtracurricularScoreLegend).Where(b => b.IdExtracurricular == param.IdExtracurricular),
                                    p => (p.IdStudent, p.idScoreComponent),
                                    h => (h.IdStudent, h.IdExtracurricularScoreComponent),
                                    (p, h) => new { p, h })
                         .SelectMany(x => x.h.DefaultIfEmpty(),
                                        (studentComponent, studentScore) => new { studentComponent, studentScore })
                         .Select(a => new GetExtracurricularStudentScoreVm
                         {
                             ExtracurricularName = a.studentComponent.p.ExtracurricularName,
                             IdStudent = a.studentComponent.p.IdStudent,
                             StudentName = a.studentComponent.p.StudentName,
                             OrderNumberComponent = a.studentComponent.p.OrderNumberComponent,
                             idScoreComponent = a.studentComponent.p.idScoreComponent,
                             ScoreComponentName = a.studentComponent.p.ScoreComponentName,
                             IdExtracurricularScoreEntry = (a.studentScore != null ? a.studentScore.Id : null),
                             score = (a.studentScore != null ? new ItemValueVm(a.studentScore.IdExtracurricularScoreLegend, a.studentScore.ExtracurricularScoreLegend.Score) : null),
                             LastestAudit = (a.studentScore != null ? Convert.ToDateTime((a.studentScore.DateUp != null ? a.studentScore.DateUp : a.studentScore.DateIn)).ToString("dd MMM yyyy HH:mm") : null),
                             AuditActivity = (a.studentScore != null ? (a.studentScore.DateUp != null ? "U" : "I") : null)
                         }).ToList();
                     

            var studentHomeroom = await _dbContext.Entity<MsHomeroomStudent>()
                             .Include(x => x.Student)
                                .ThenInclude(y => y.ExtracurricularParticipants)
                                .ThenInclude(y => y.Extracurricular)
                             .Where(a => a.Student.ExtracurricularParticipants.Any(b => b.IdExtracurricular == param.IdExtracurricular && b.Extracurricular.Semester == param.Semester && b.Status == true))
                             //.Where(a => StudentScoreComponent.Select(b => b.IdStudent).Contains(a.IdStudent))
                             .Where(a => a.Homeroom.IdAcademicYear == param.IdAcademicYear
                                         && a.Homeroom.Semester == param.Semester)
                             .Select(a => new {
                                 a.IdStudent,
                                 StudentName = (a.Student.FirstName == null ? "" : a.Student.FirstName + " ")+ a.Student.LastName,                               
                                 GradeName = a.Homeroom.Grade.Code + a.Homeroom.GradePathwayClassroom.Classroom.Code
                             }).Distinct()
                             .OrderBy(a => a.StudentName)
                             .ToListAsync();

            GetExtracurricularStudentScoreResult ReturnResult = new GetExtracurricularStudentScoreResult();
            ReturnResult.Header = ScoreComponent.Select(a => new ExtracurricularComponentScore_HeaderVm()
            {
                idScoreComponent = a.Id,
                ScoreComponentName = a.Description
            }).ToList();


            ReturnResult.Body = studentHomeroom.Select(a => new ExtracurricularComponentScore_BodyVm() {
                ExtracurricularName = extracurricularData.Name + " - " + string.Join("; ", extracurricularData.ExtracurricularGradeMappings.Select(b => b.Grade.Description)),
                IdStudent = a.IdStudent,
                StudentName = a.StudentName,
                Homeroom = a.GradeName,
                IntScore = (StudentScoreComponent.Count > 0 ? (getExtracurricularScoreComp.CalculationType.ToLower() == "sum"
                        ? Math.Round(StudentScoreComponent.Where(x => x.IdStudent == a.IdStudent).Sum(x => Convert.ToDecimal(x.score?.Description)), 2)
                        : Math.Round((StudentScoreComponent.Where(x => x.IdStudent == a.IdStudent).Sum(x => Convert.ToDecimal(x.score?.Description)) / StudentScoreComponent.Where(x => x.IdStudent == a.IdStudent).Count()), 2)) : 0),
                 
                FinalScore = (StudentScoreComponent.Count > 0 ? (getExtracurricularScoreComp.CalculationType.ToLower() == "sum"
                        ? StudentScoreComponent.Where(x => x.IdStudent == a.IdStudent).Sum(x => Convert.ToDecimal(x.score?.Description)).ToString("#.##")
                        : (StudentScoreComponent.Where(x => x.IdStudent == a.IdStudent).Sum(x => Convert.ToDecimal(x.score?.Description)) / StudentScoreComponent.Where(x => x.IdStudent == a.IdStudent).Count()).ToString("#.##")) : "0"),
                ComponentScores = StudentScoreComponent.Where(b => b.IdStudent == a.IdStudent)
                                                                                .OrderBy(c => c.OrderNumberComponent)
                                                                                .Select(d => new ExtracurricularComponentScoreVm()
                                                                                {
                                                                                    IdScoreComponent = d.idScoreComponent,
                                                                                    ScoreComponentName = d.ScoreComponentName,
                                                                                    IdExtracurricularScoreEntry = d.IdExtracurricularScoreEntry,
                                                                                    score = d.score,
                                                                                    AuditActivity = d.AuditActivity,
                                                                                    LastestAudit = d.LastestAudit
                                                                                }).ToList()
            }).Distinct().ToList();
            string finalGrading = null;
            foreach (var item in ReturnResult.Body)
            {
                if (getExtracurricularScoreComp != null)
                {
                    finalGrading = getExtracurricularScoreComp.ScoreGrades.Where(x => item.IntScore >= x.MinScore && item.IntScore <= x.MaxScore).Select(x => x.Grade).FirstOrDefault();
                    item.Grade = finalGrading != null ? finalGrading : "N/A";
                }
                else
                {
                    item.Grade = "N/A";
                }
            }

            ReturnResult.CalculationType = getExtracurricularScoreComp?.CalculationType;

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
