using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
  public class ResetMeritDemeritPointV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public ResetMeritDemeritPointV2Handler(IStudentDbContext DbContext, IMachineDateTime datetime)
        {
            _dbContext = DbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ResetMeritDemeritPointV2Request, ResetMeritDemeritPointV2Validator>();

            if(body.IdAcademicYear == null)
            {
                body.IdAcademicYear = await _dbContext.Entity<MsPeriod>()
                                .Include(e=>e.Grade).ThenInclude(e=>e.MsLevel)
                               .Where(e => e.Grade.MsLevel.MsAcademicYear.IdSchool==body.IdSchool 
                                        && (_datetime.ServerTime.Date >= e.StartDate.Date && _datetime.ServerTime.Date <= e.EndDate.Date))
                               .Select(e=>e.Grade.MsLevel.IdAcademicYear)
                               .FirstOrDefaultAsync(CancellationToken);
            }

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.Homeroom)
                            .Where(e => e.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                            .Select(e => new HomeroomStudent
                            {
                                IdStudent = e.IdStudent,
                                IdHomeroomStudent = e.Id,
                                IdGrade = e.Homeroom.IdGrade,
                                Semester = e.Homeroom.Semester,
                            })
                            .ToListAsync(CancellationToken);

            var listIdGrade = listHomeroomStudent.Select(e => e.IdGrade).Distinct().ToList();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                            .Where(e => listIdGrade.Contains(e.IdGrade))
                            .ToListAsync(CancellationToken);

            foreach (var idGrade in listIdGrade)
            {
                var startdateSemester2 = listPeriod.Where(e => e.IdGrade == idGrade && e.Semester == 2).Select(e => e.StartDate).Min();

                if (_datetime.ServerTime.Date >= startdateSemester2.Date)
                {
                    var jobMeritDemerit = await JobMeritDemerit(idGrade, listHomeroomStudent, body.IdAcademicYear, body.IdSchool);
                }
            }

            return Request.CreateApiResult2();
        }

        private async Task<string> JobMeritDemerit(string idGrade, List<HomeroomStudent> listHomeroomStudent, string IdAcademicYear, string IdSchool)
        {
            var listHomeroomStudentByGradeSmt1 = listHomeroomStudent.Where(e => e.IdGrade == idGrade && e.Semester == 1).ToList();
            var listIdHomeroomStudentSmt1 = listHomeroomStudent.Where(e => e.IdGrade == idGrade && e.Semester == 1).Select(e => e.IdHomeroomStudent).ToList();

            var listStudentPoint = await _dbContext.Entity<TrStudentPoint>()
                            .Include(e => e.HomeroomStudent)
                            .Where(e=>e.HomeroomStudent.Homeroom.IdGrade==idGrade)
                            .ToListAsync(CancellationToken);

            var listStudentPointSmt1 = listStudentPoint.Where(e => e.HomeroomStudent.Semester == 1 && listIdHomeroomStudentSmt1.Contains(e.IdHomeroomStudent)).ToList();
            var listStudentPointSmt2 = listStudentPoint.Where(e => e.HomeroomStudent.Semester == 2).ToList();

            var listScoreContinuationSetting = await _dbContext.Entity<MsScoreContinuationSetting>()
                            .Where(e => e.IdGrade == idGrade)
                            .ToListAsync(CancellationToken);

            var listEntryMeritStudent = await _dbContext.Entity<TrEntryMeritStudent>()
                           .Where(e => e.HomeroomStudent.Homeroom.IdGrade == idGrade && e.Status=="Approved")
                           .ToListAsync(CancellationToken);

            var listEntryDemeritStudent = await _dbContext.Entity<TrEntryDemeritStudent>()
                        .Include(e=>e.MeritDemeritMapping).ThenInclude(e=>e.LevelOfInteraction).ThenInclude(e=>e.Parent)
                         .Where(e => e.HomeroomStudent.Homeroom.IdGrade == idGrade && e.Status == "Approved")
                         .ToListAsync(CancellationToken);

            var lisSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                        .Where(e => e.IdAcademicYear== IdAcademicYear)
                        .ToListAsync(CancellationToken);

            var getScoreContinuationSettingMerit = listScoreContinuationSetting.Where(e => e.Category == MeritDemeritCategory.Merit).FirstOrDefault();
            var getScoreContinuationSettingDemerit = listScoreContinuationSetting.Where(e => e.Category == MeritDemeritCategory.AccountabilityPoints).FirstOrDefault();
            //List<string> idStudentPointModif = new List<string>();
            foreach (var item in listStudentPointSmt1)
            {
                var idStudent = item.HomeroomStudent.IdStudent;
                var idHomeroomStudentSmt2 = listHomeroomStudent
                                            .Where(e => e.IdGrade == idGrade && e.Semester == 2 && e.IdStudent == idStudent)
                                            .Select(e=>e.IdHomeroomStudent).FirstOrDefault();

                if (idHomeroomStudentSmt2 == null)
                    continue;

                var meritPoint = 0;
                var DemeritPoint = 0;
                var isResetDemerit = false;
                string idLevelOfInteraction = null;
                string idSanctionMapping = null;

                if (getScoreContinuationSettingMerit == null)
                {
                    meritPoint = 0;
                }
                else
                {
                    var getResetContinuousScore = ResetContinuousScore(item.MeritPoint, getScoreContinuationSettingMerit);
                    meritPoint = getResetContinuousScore.Point;
                }

                if (getScoreContinuationSettingDemerit == null)
                {
                    DemeritPoint = 0;
                    isResetDemerit = true;
                }
                else
                {
                    var getResetContinuousScore = ResetContinuousScore(item.DemeritPoint, getScoreContinuationSettingDemerit);
                    DemeritPoint = getResetContinuousScore.Point;
                    isResetDemerit = getResetContinuousScore.IsReset;
                }

                var listStudentPointSmt2ByStudent = listStudentPointSmt2.Where(e=>e.HomeroomStudent.IdStudent==idStudent).FirstOrDefault();

                var listEntryMeritStudentByHomeroomStudentSmt2 = listEntryMeritStudent.Where(e => e.IdHomeroomStudent == idHomeroomStudentSmt2).ToList();
                var listEntryDemeritStudentByHomeroomStudentSmt2 = listEntryDemeritStudent.Where(e => e.IdHomeroomStudent == idHomeroomStudentSmt2).ToList();

                var pointMeritSmt2 = listEntryMeritStudentByHomeroomStudentSmt2.Select(e => e.Point).Sum();
                var pointDemeritSmt2 = listEntryDemeritStudentByHomeroomStudentSmt2.Select(e => e.Point).Sum();

                meritPoint += pointMeritSmt2;
                DemeritPoint += pointDemeritSmt2;

                if (IdSchool == "2")
                {
                    var getLevelOfInteraction = listEntryDemeritStudent.Where(e => e.HomeroomStudent.IdStudent == item.HomeroomStudent.IdStudent && e.HomeroomStudent.Semester==1)
                                                  .Select(e => new
                                                  {
                                                      Id = e.MeritDemeritMapping.LevelOfInteraction.Id,
                                                      Parent = Convert.ToInt32(e.MeritDemeritMapping.LevelOfInteraction.Parent.NameLevelOfInteraction),
                                                      child = e.MeritDemeritMapping.LevelOfInteraction.NameLevelOfInteraction,
                                                  })
                                                  .OrderBy(e => e.Parent).ThenBy(e => e.child)
                                                  .LastOrDefault();

                    if (getLevelOfInteraction != null)
                    {
                        if (getLevelOfInteraction.Parent >= 3)
                        {
                            isResetDemerit = false;
                            DemeritPoint = listEntryDemeritStudent.Where(e => e.HomeroomStudent.IdStudent == item.HomeroomStudent.IdStudent)
                                                .Select(e => e.Point)
                                                .Sum();
                        }
                    }
                }

                if (isResetDemerit)
                {
                    var getLevelOfInteraction = listEntryDemeritStudentByHomeroomStudentSmt2
                                                    .Select(e => new
                                                    {
                                                        Id = e.MeritDemeritMapping.LevelOfInteraction.Id,
                                                        Parent = Convert.ToInt32(e.MeritDemeritMapping.LevelOfInteraction.Parent.NameLevelOfInteraction),
                                                        child = e.MeritDemeritMapping.LevelOfInteraction.NameLevelOfInteraction,
                                                    })
                                                    .OrderBy(e=>e.Parent).ThenBy(e=>e.child)
                                                    .LastOrDefault();

                    if(getLevelOfInteraction!=null)
                        idLevelOfInteraction = getLevelOfInteraction.Id;
                }
                else
                {
                    var getLevelOfInteraction = listEntryDemeritStudent.Where(e=>e.HomeroomStudent.IdStudent==item.HomeroomStudent.IdStudent)
                                                  .Select(e => new
                                                  {
                                                      Id = e.MeritDemeritMapping.LevelOfInteraction.Id,
                                                      Parent = Convert.ToInt32(e.MeritDemeritMapping.LevelOfInteraction.Parent.NameLevelOfInteraction),
                                                      child = e.MeritDemeritMapping.LevelOfInteraction.NameLevelOfInteraction,
                                                  })
                                                  .OrderBy(e => e.Parent).ThenBy(e => e.child)
                                                  .LastOrDefault();

                    if (getLevelOfInteraction != null)
                        idLevelOfInteraction = getLevelOfInteraction.Id;
                }

                var getSanctionMapping = lisSanctionMapping.Where(e=>e.Min<= DemeritPoint && e.Max>= DemeritPoint).FirstOrDefault();
                if (getSanctionMapping == null)
                    idSanctionMapping = null;
                else
                    idSanctionMapping = getSanctionMapping.Id;

                if (listStudentPointSmt2ByStudent == null)
                {
                    TrStudentPoint newStudentPoint = new TrStudentPoint
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdHomeroomStudent = idHomeroomStudentSmt2,
                        MeritPoint = meritPoint,
                        DemeritPoint = DemeritPoint,
                        IdLevelOfInteraction = idLevelOfInteraction,
                        IdSanctionMapping = idSanctionMapping
                    };
                    _dbContext.Entity<TrStudentPoint>().Add(newStudentPoint);
                }
                else
                {
                    if(listStudentPointSmt2ByStudent.MeritPoint != meritPoint || listStudentPointSmt2ByStudent.DemeritPoint != DemeritPoint)
                    {
                        listStudentPointSmt2ByStudent.MeritPoint = meritPoint;
                        listStudentPointSmt2ByStudent.DemeritPoint = DemeritPoint;
                        listStudentPointSmt2ByStudent.IdSanctionMapping = idSanctionMapping;
                        listStudentPointSmt2ByStudent.IdLevelOfInteraction = idLevelOfInteraction;
                        _dbContext.Entity<TrStudentPoint>().Update(listStudentPointSmt2ByStudent);
                        //idStudentPointModif.Add(listStudentPointSmt2ByStudent.Id);
                    }
                }
            }

            //var removeStudentPoints = listStudentPoint.Where(e => e.HomeroomStudent.Semester == 2 && !idStudentPointModif.Contains(e.Id)).ToList();
            //removeStudentPoints.ForEach(e =>e.IsActive = false);
            //_dbContext.Entity<TrStudentPoint>().UpdateRange(removeStudentPoints);
            await _dbContext.SaveChangesAsync();

            return default;
        }

        private static ResetContinuousScoreResult ResetContinuousScore(int pointSmt1, MsScoreContinuationSetting getScoreContinuationSetting)
        {
            var pointSmt2 = 0;
            var isResetMerit = false;
            if (getScoreContinuationSetting.ScoreContinueOption == ScoreContinueOption.Continue)
            {
                pointSmt2 = pointSmt1;
                isResetMerit = true;
            }
            else
            {
                var pointReset = getScoreContinuationSetting.Score;

                //"="
                if (getScoreContinuationSetting.Operation== OperationOption.Equal) 
                {
                    if (pointSmt1 == pointReset)
                    {
                        pointSmt2 = 0;
                        isResetMerit = true;
                    }
                    else
                    {
                        pointSmt2 = pointSmt1;
                        isResetMerit = false;
                    }
                }

                //">"
                if (getScoreContinuationSetting.Operation == OperationOption.GreaterThan)
                {
                    if (pointSmt1 > pointReset)
                    {
                        pointSmt2 = 0;
                        isResetMerit = true;
                    }
                    else
                    {
                        pointSmt2 = pointSmt1;
                        isResetMerit = false;
                    }
                }

                //"<"
                if (getScoreContinuationSetting.Operation == OperationOption.LessThan)
                {
                    if (pointSmt1 < pointReset)
                    {
                        pointSmt2 = 0;
                        isResetMerit = true;
                    }
                    else
                    {
                        pointSmt2 = pointSmt1;
                        isResetMerit = false;
                    }
                }

                //">="
                if (getScoreContinuationSetting.Operation == OperationOption.GreaterThanOrEqual) 
                {
                    if (pointSmt1 >= pointReset)
                    {
                        pointSmt2 = 0;
                        isResetMerit = true;
                    }
                    else
                    {
                        pointSmt2 = pointSmt1;
                        isResetMerit = false;
                    }
                }

                //"<="
                if (getScoreContinuationSetting.Operation == OperationOption.LessThanOrEqual)
                {
                    if (pointSmt1 <= pointReset)
                    {
                        pointSmt2 = 0;
                        isResetMerit = true;
                    }
                    else
                    {
                        pointSmt2 = pointSmt1;
                        isResetMerit = false;
                    }
                }
            }

            ResetContinuousScoreResult result = new ResetContinuousScoreResult
            {
                Point = pointSmt2,
                IsReset = isResetMerit
            };

            return result;
        }

    }

    public class HomeroomStudent
    {
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
    }

    public class ResetContinuousScoreResult
    {
        public int Point { get; set; }
        public bool IsReset { get; set; }
    }
}


