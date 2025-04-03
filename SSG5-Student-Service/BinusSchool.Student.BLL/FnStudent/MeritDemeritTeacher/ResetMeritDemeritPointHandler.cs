using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class ResetMeritDemeritPointHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public ResetMeritDemeritPointHandler(IStudentDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ResetMeritDemeritPointRequest, ResetMeritDemeritPointValidator>();
            ScoreContinueOption ScoreContinueOptionMerit = ScoreContinueOption.Reset;
            ScoreContinueEvery ScoreContinueEveryMerit = ScoreContinueEvery.Semester;
            int? ScoreMerit = 0;
            ScoreContinueOption ScoreContinueOptionDemerit = ScoreContinueOption.Reset;
            ScoreContinueEvery ScoreContinueEveryDemerit = ScoreContinueEvery.Semester;
            int? ScoreDemerit = 0;

            var PreviousSemester = body.Semester == 2 ? 1 : 2;

            var CodeAcacedemicYear = await _dbContext.Entity<MsAcademicYear>()
                                            .Where(e => e.Id == body.IdAcademicYear)
                                            .Select(e => e.Code)
                                            .FirstOrDefaultAsync(CancellationToken);

            var PreviousCodeAcacedemicYear = (Convert.ToInt32(CodeAcacedemicYear)) - 1;

            var GetPreviousAcacedemicYear = await _dbContext.Entity<MsAcademicYear>()
                                            .Where(e => e.Code == PreviousCodeAcacedemicYear.ToString() && e.IdSchool == body.IdSchool)
                                            .FirstOrDefaultAsync(CancellationToken);

            var PreviousAcacedemicYear = body.Semester == 2 ? body.IdAcademicYear : GetPreviousAcacedemicYear==null?"": GetPreviousAcacedemicYear.Id;

            var predicate = PredicateBuilder.Create<TrStudentPoint>(x => x.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == PreviousAcacedemicYear && x.HomeroomStudent.Homeroom.Semester == PreviousSemester);

            var GetPreviousGrade = await _dbContext.Entity<MsGrade>()
                            .Where(e => e.Code == body.CodeGrade && e.MsLevel.IdAcademicYear == PreviousAcacedemicYear)
                            .FirstOrDefaultAsync(CancellationToken);

            var GetStudentPoint = await _dbContext.Entity<TrStudentPoint>()
                                        .Include(e => e.HomeroomStudent)
                                        .Where(x => x.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear
                                                && x.HomeroomStudent.Homeroom.Semester == body.Semester
                                                && x.HomeroomStudent.Homeroom.IdGrade == body.IdGrade)
                                        .Distinct().ToListAsync(CancellationToken);

            GetStudentPoint.ForEach(e=>e.IsActive=false);
            _dbContext.Entity<TrStudentPoint>().UpdateRange(GetStudentPoint);

            var GetPreviousStudentPoint = await _dbContext.Entity<TrStudentPoint>()
                                            .Include(e => e.HomeroomStudent)
                                            .Where(x => x.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == PreviousAcacedemicYear
                                                    && x.HomeroomStudent.Homeroom.Semester == PreviousSemester
                                                    && x.HomeroomStudent.Homeroom.IdGrade == GetPreviousGrade.Id)
                                            .Distinct().ToListAsync(CancellationToken);

            var GetScoreContinuationSetting = await _dbContext.Entity<MsScoreContinuationSetting>()
                                            .Where(x => x.IdGrade == body.IdGrade)
                                            .Distinct().ToListAsync(CancellationToken);

            var GetScoreContinuationSettingMerit = GetScoreContinuationSetting.Where(e => e.Category == MeritDemeritCategory.Merit).FirstOrDefault();
            var GetScoreContinuationSettingDemerit = GetScoreContinuationSetting.Where(e => e.Category == MeritDemeritCategory.AccountabilityPoints).FirstOrDefault();
            if (GetScoreContinuationSettingMerit!=null)
            {
                ScoreContinueOptionMerit = GetScoreContinuationSettingMerit.ScoreContinueOption;
                ScoreContinueEveryMerit = GetScoreContinuationSettingMerit.ScoreContinueEvery;
                ScoreMerit = GetScoreContinuationSettingMerit.Score;
            }

            if (GetScoreContinuationSettingDemerit != null)
            {
                ScoreContinueOptionDemerit = GetScoreContinuationSettingDemerit.ScoreContinueOption;
                ScoreContinueEveryDemerit = GetScoreContinuationSettingDemerit.ScoreContinueEvery;
                ScoreDemerit = GetScoreContinuationSettingDemerit.Score;
            }

            var GetHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                            .Include(e => e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel)
                                            .Where(x => x.Homeroom.IdGrade == body.IdGrade
                                                        && x.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear
                                                        && x.Homeroom.Semester == body.Semester)
                                            .Distinct().ToListAsync(CancellationToken);

            foreach (var item in GetPreviousStudentPoint)
            {
                var PointMerit = 0;
                var PointDemerit = 0;
                var IdSanctionMapping = "";
                var IdLevelOfInteraction = "";

                var IdHomeroomStudent = GetHomeroomStudent
                                        .Where(e => e.IdStudent == item.HomeroomStudent.IdStudent
                                                    && e.Homeroom.IdGrade == body.IdGrade
                                                    && e.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear
                                                    && e.Homeroom.Semester == body.Semester
                                        )
                                        .Select(e => e.Id)
                                        .FirstOrDefault();

                if (string.IsNullOrEmpty(IdHomeroomStudent))
                    continue;

                if (ScoreContinueOptionMerit == ScoreContinueOption.Continue)
                {
                    //continue
                    PointMerit = item.MeritPoint;
                }
                else
                {
                    //reset score
                    if (ScoreContinueEveryMerit == ScoreContinueEvery.Semester)
                    {
                        PointMerit = item.MeritPoint >= ScoreMerit ? 0 : item.MeritPoint;
                    }
                    else
                    {
                        //reset score for ay
                        if (body.Semester == 1)
                        {
                            PointMerit = item.MeritPoint >= ScoreMerit ? 0 : item.MeritPoint;
                        }
                        else
                        {
                            PointMerit = item.MeritPoint;
                        }
                    }
                }

                if (ScoreContinueOptionDemerit == ScoreContinueOption.Continue)
                {
                    //continue
                    PointDemerit = item.DemeritPoint;
                    IdSanctionMapping = item.IdSanctionMapping;
                    IdLevelOfInteraction = item.IdLevelOfInteraction;
                }
                else
                {
                    //reset score
                    if (ScoreContinueEveryDemerit == ScoreContinueEvery.Semester)
                    {
                        if (item.DemeritPoint >= ScoreDemerit)
                        {

                        }
                        else
                        {

                        }

                        PointDemerit = item.DemeritPoint >= ScoreDemerit ? 0 : item.DemeritPoint;
                        IdSanctionMapping = item.DemeritPoint >= ScoreDemerit ? null : item.IdSanctionMapping;
                        IdLevelOfInteraction = item.DemeritPoint >= ScoreDemerit ? null : item.IdLevelOfInteraction;
                    }
                    else
                    {
                        //reset score for ay
                        if (body.Semester == 1)
                        {
                            PointDemerit = item.DemeritPoint >= ScoreDemerit ? 0 : item.DemeritPoint;
                            IdSanctionMapping = item.DemeritPoint >= ScoreDemerit ? null : item.IdSanctionMapping;
                            IdLevelOfInteraction = item.DemeritPoint >= ScoreDemerit ? null : item.IdLevelOfInteraction;
                        }
                        else
                        {
                            PointDemerit = item.DemeritPoint;
                            IdSanctionMapping = item.IdSanctionMapping;
                            IdLevelOfInteraction = item.IdLevelOfInteraction;
                        }
                    }
                }

                var NewStudentPoint = new TrStudentPoint
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHomeroomStudent = IdHomeroomStudent,
                    MeritPoint = PointMerit,
                    DemeritPoint = PointDemerit,
                    IdSanctionMapping = IdSanctionMapping,
                    IdLevelOfInteraction = IdLevelOfInteraction,
                };
                _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
