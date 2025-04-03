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
using Microsoft.Extensions.Logging;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class CalculateMeritDemeritPointHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly ILogger<CalculateMeritDemeritPointHandler> _logger;
        private readonly IMachineDateTime _datetime;
        public CalculateMeritDemeritPointHandler(IStudentDbContext EntryMeritDemetitDbContext, ILogger<CalculateMeritDemeritPointHandler> logger, IMachineDateTime datetime)
        {
            _dbContext = EntryMeritDemetitDbContext;
            _logger = logger;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CalculateMeritDemeritPointRequest, CalculateMeritDemeritPointValidator>();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                                .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
                               .Where(e => e.Grade.MsLevel.MsAcademicYear.IdSchool == body.IdSchool
                                        && (_datetime.ServerTime.Date >= e.StartDate.Date && _datetime.ServerTime.Date <= e.EndDate.Date))
                               .ToListAsync(CancellationToken);

            if (body.IdAcademicYear == null)
            {
                body.IdAcademicYear = listPeriod
                               .Select(e => e.Grade.MsLevel.IdAcademicYear)
                               .FirstOrDefault();
            }

            var listStudentPoint = await _dbContext.Entity<TrStudentPoint>()
                                       .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                       .Include(e=>e.LevelOfInteraction)
                                       .Where(e => e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                                       .ToListAsync(CancellationToken);

            var listStudentPointSmt1 = listStudentPoint.Where(e => e.HomeroomStudent.Homeroom.Semester == 1).ToList();
            var listStudentPointSmt2 = listStudentPoint.Where(e => e.HomeroomStudent.Homeroom.Semester == 2).ToList();
            var listIdStudent = listStudentPoint.Select(e => e.HomeroomStudent.IdStudent).Distinct().ToList();

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(e=>e.Homeroom)
                                        .Where(e => listIdStudent.Contains(e.IdStudent) && e.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                                        .Select(e => new
                                        {
                                            e.Homeroom.Semester,
                                            idHomeroomStudent = e.Id,
                                            e.IdStudent,
                                            e.Homeroom.IdGrade
                                        })
                                        .ToListAsync(CancellationToken);

            var listIdHomeroomStudentSmt1 = listHomeroomStudent.Where(e => e.Semester == 1).Select(e => e.idHomeroomStudent).ToList();
            var listIdHomeroomStudentSmt2 = listHomeroomStudent.Where(e => e.Semester == 2).Select(e => e.idHomeroomStudent).ToList();

            List<GetStudent> ListStudent = new List<GetStudent>();
            foreach (var idStudent in listIdStudent)
            {
                var IdHomeroomStudentSmt1 = listHomeroomStudent
                                                .Where(e => e.Semester == 1 && e.IdStudent==idStudent)
                                                .Select(e => e.idHomeroomStudent)
                                                .FirstOrDefault();

                var IdHomeroomStudentSmt2 = listHomeroomStudent
                                                .Where(e => e.Semester == 2 && e.IdStudent == idStudent)
                                                .Select(e => e.idHomeroomStudent)
                                                .FirstOrDefault();

                var idGrade = listHomeroomStudent
                                .Where(e => e.IdStudent == idStudent)
                                .Select(e => e.IdGrade)
                                .FirstOrDefault();

                if (IdHomeroomStudentSmt2 != null)
                {
                    ListStudent.Add(new GetStudent
                    {
                        IdStudent = idStudent,
                        IdHomeroomStudentSmt1 = IdHomeroomStudentSmt1 == null ? "" : IdHomeroomStudentSmt1,
                        IdHomeroomStudentSmt2 = IdHomeroomStudentSmt2 == null ? "" : IdHomeroomStudentSmt2,
                        IdGrade = idGrade == null ? "" : idGrade
                    });
                }
            }

            var listEntryMeritStudentSmt2 = await _dbContext.Entity<TrEntryMeritStudent>()
                                        .Where(e => listIdHomeroomStudentSmt2.Contains(e.IdHomeroomStudent) && e.Status == "Approved" && e.RequestType== RequestType.Create && e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                                        .Select(e => new
                                        {
                                            point = e.Point,
                                            idHomeroomStudent = e.IdHomeroomStudent,
                                        })
                                        .ToListAsync(CancellationToken);
            
            var listEntryDemerit = await _dbContext.Entity<TrEntryDemeritStudent>()
                                    .Include(e => e.MeritDemeritMapping).ThenInclude(e => e.LevelOfInteraction)
                                    .Where(e => listIdStudent.Contains(e.HomeroomStudent.IdStudent) && e.Status == "Approved" && e.RequestType == RequestType.Create && e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear==body.IdAcademicYear)
                                    //.Where(e=>e.HomeroomStudent.IdStudent== "2070005465" && e.Status == "Approved")
                                    .Select(e => new
                                    {
                                        point = e.Point,
                                        idHomeroomStudent = e.IdHomeroomStudent,
                                        idLevelOfOnfraction = e.MeritDemeritMapping.IdLevelOfInteraction,
                                        idStudent = e.HomeroomStudent.IdStudent,
                                    })
                                    .ToListAsync(CancellationToken);

            var listEntryDemeritStudentSmt2 = listEntryDemerit
                                                .Where(e => listIdHomeroomStudentSmt2.Contains(e.idHomeroomStudent))
                                                .Select(e => new
                                                {
                                                    point = e.point,
                                                    idHomeroomStudent = e.idHomeroomStudent,
                                                    idLevelOfOnfraction = e.idLevelOfOnfraction,
                                                })
                                                .ToList();

            var listScoreContinuationSetting = await _dbContext.Entity<MsScoreContinuationSetting>()
                                               .Where(e => e.Grade.MsLevel.IdAcademicYear==body.IdAcademicYear)
                                               .Select(e => new
                                               {
                                                   e.IdGrade,
                                                   e.Score,
                                                   e.ScoreContinueEvery,
                                                   e.ScoreContinueOption,
                                                   e.Category
                                               })
                                               .ToListAsync(CancellationToken);

            var listLevelOfInfraction = await _dbContext.Entity<MsLevelOfInteraction>()
                                   .Include(e => e.Parent)
                                   .Where(e => e.IdSchool==body.IdSchool)
                                   .ToListAsync(CancellationToken);

            var GetLevelOfInfraction = listLevelOfInfraction
                                        .Select(e => new
                                        {
                                            idLevelOfInfraction = e.Id,
                                            nameLevelOfInfraction = e.Parent == null ? $"{e.NameLevelOfInteraction}-" : e.Parent.NameLevelOfInteraction + e.NameLevelOfInteraction,
                                        })
                                   .OrderBy(e => e.nameLevelOfInfraction).ToList();

            var listSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                                  .Where(e => e.IdAcademicYear == body.IdAcademicYear)
                                  .ToListAsync(CancellationToken);

            foreach (var student in ListStudent)
            {
                var semester = listPeriod.Where(e => e.IdGrade == student.IdGrade).Select(e => e.Semester).FirstOrDefault();

                if (semester != 2)
                    continue;

                var newPointDemerit = 0;
                var newPointMerit = 0;
                var pointDemeritSmt1 = 0;
                var pointMeritSm1 = 0;
                var scoreResetMerit = 0;
                var scoreResetDemerit = 0;
                var isReset = false;
                ScoreContinueOption ScoreContinueOptionMerit = ScoreContinueOption.Reset;
                ScoreContinueEvery ScoreContinueEveryMerit = ScoreContinueEvery.Semester;
                ScoreContinueOption ScoreContinueOptionDemerit = ScoreContinueOption.Reset;
                ScoreContinueEvery ScoreContinueEveryDemerit = ScoreContinueEvery.Semester;

                var ScoreContinuationSettingMerit = listScoreContinuationSetting.Where(e => e.IdGrade == student.IdGrade && e.Category== MeritDemeritCategory.Merit).FirstOrDefault();
                var ScoreContinuationSettingDemerit = listScoreContinuationSetting.Where(e => e.IdGrade == student.IdGrade && e.Category == MeritDemeritCategory.AccountabilityPoints).FirstOrDefault();
                var studentPointSmt1 = listStudentPointSmt1.Where(e => e.HomeroomStudent.IdStudent == student.IdStudent).FirstOrDefault();
                if (studentPointSmt1 != null)
                {
                    pointDemeritSmt1 = studentPointSmt1.DemeritPoint;
                    pointMeritSm1 = studentPointSmt1.MeritPoint;
                }

                if (ScoreContinuationSettingMerit != null)
                {
                    scoreResetMerit = Convert.ToInt32(ScoreContinuationSettingMerit.Score);
                    ScoreContinueOptionMerit = ScoreContinuationSettingMerit.ScoreContinueOption;
                    ScoreContinueEveryMerit = ScoreContinuationSettingMerit.ScoreContinueEvery;
                }

                if (ScoreContinuationSettingDemerit != null)
                {
                    scoreResetDemerit = Convert.ToInt32(ScoreContinuationSettingDemerit.Score);
                    ScoreContinueOptionDemerit = ScoreContinuationSettingDemerit.ScoreContinueOption;
                    ScoreContinueEveryDemerit = ScoreContinuationSettingDemerit.ScoreContinueEvery;
                }

                #region Reset/continue Demerit
                if (ScoreContinueOptionDemerit == ScoreContinueOption.Reset)
                {
                    if (ScoreContinueEveryDemerit == ScoreContinueEvery.Semester)
                    {
                        if (pointDemeritSmt1 < 0)
                        {
                            if (pointDemeritSmt1 >= scoreResetDemerit)
                            {
                                newPointDemerit = 0;
                                isReset = true;
                            }
                            else
                            {
                                newPointDemerit = pointDemeritSmt1;
                                isReset = false;
                            }
                        }
                        else if (pointDemeritSmt1 > 0)
                        {
                            if (pointDemeritSmt1 <= scoreResetDemerit)
                            {
                                newPointDemerit = 0;
                                isReset = true;
                            }
                            else
                            {
                                newPointDemerit = pointDemeritSmt1;
                                isReset = false;
                            }
                        }
                    }
                    else if (ScoreContinueEveryDemerit == ScoreContinueEvery.Academicyear)
                    {
                        newPointDemerit = pointDemeritSmt1;
                    }
                }
                else if (ScoreContinueOptionDemerit == ScoreContinueOption.Continue)
                {
                    newPointDemerit = pointDemeritSmt1;
                }
                #endregion

                #region demerit point smt 2
                if (body.IdSchool == "2")
                {
                    if (studentPointSmt1 != null)
                    {
                        var levelOfInfraction = studentPointSmt1.LevelOfInteraction == null ? "" : studentPointSmt1.LevelOfInteraction.NameLevelOfInteraction;

                        var pointDemeritSmt2 = listEntryDemeritStudentSmt2.Where(e => e.idHomeroomStudent == student.IdHomeroomStudentSmt2).Sum(e => e.point);

                        if (levelOfInfraction == "3" || levelOfInfraction == "4")
                        {
                            newPointDemerit = studentPointSmt1.DemeritPoint + pointDemeritSmt2;
                            isReset = false;
                        }
                        else
                        {
                            newPointDemerit += pointDemeritSmt2;
                        }
                    }

                }
                else
                {
                    var pointDemeritSmt2 = listEntryDemeritStudentSmt2.Where(e => e.idHomeroomStudent == student.IdHomeroomStudentSmt2).Sum(e => e.point);
                    newPointDemerit += pointDemeritSmt2;
                }


                #endregion

                #region Get level of infraction
                string idLevelOfInteraction = null;
                if (isReset == true)
                {
                    if (listEntryDemeritStudentSmt2.Any())
                    {
                        var listIdLevelOfOnfraction = listEntryDemeritStudentSmt2.Where(e => e.idHomeroomStudent == student.IdHomeroomStudentSmt2).Select(e => e.idLevelOfOnfraction).ToList();
                        var listLevelOfInfractionById = GetLevelOfInfraction.Where(e => listIdLevelOfOnfraction.Contains(e.idLevelOfInfraction)).ToList();
                        idLevelOfInteraction = listLevelOfInfractionById.OrderBy(e => e.nameLevelOfInfraction).Select(e => e.idLevelOfInfraction).LastOrDefault();
                    }
                    else
                    {
                        idLevelOfInteraction = null;
                    }
                }
                else if (isReset == false && listEntryDemerit.Any())
                {
                    if (listEntryDemerit.Any())
                    {
                        var listIdLevelOfInfraction = listEntryDemerit.Where(e => e.idStudent == student.IdStudent).Select(e => e.idLevelOfOnfraction).ToList();
                        var listLevelOfInfractionById = GetLevelOfInfraction.Where(e => listIdLevelOfInfraction.Contains(e.idLevelOfInfraction)).ToList();
                        idLevelOfInteraction = listLevelOfInfractionById.OrderBy(e => e.nameLevelOfInfraction).Select(e => e.idLevelOfInfraction).LastOrDefault();
                    }
                    else
                    {
                        idLevelOfInteraction = null;
                    }
                }
                #endregion

                #region Sunction
                var idSanctionMapping = listSanctionMapping.Where(e => newPointDemerit >= e.Min && newPointDemerit <= e.Max).Select(e => e.Id).FirstOrDefault();
                #endregion

                #region Reset/continue Merit
                if (ScoreContinueOptionMerit == ScoreContinueOption.Reset)
                {
                    if (ScoreContinueEveryMerit == ScoreContinueEvery.Semester)
                    {
                        if (pointMeritSm1 < 0)
                        {
                            if (pointMeritSm1 >= scoreResetMerit)
                                newPointMerit = 0;
                            else
                                newPointMerit = pointMeritSm1;
                        }
                        else if (pointMeritSm1 > 0)
                        {
                            if (pointMeritSm1 >= scoreResetMerit)
                                newPointMerit = 0;
                            else
                                newPointMerit = pointMeritSm1;
                        }
                    }
                    else if (ScoreContinueEveryMerit == ScoreContinueEvery.Academicyear)
                    {
                        newPointMerit = pointMeritSm1;
                    }
                }
                else if (ScoreContinueOptionMerit == ScoreContinueOption.Continue)
                {
                    newPointMerit = pointMeritSm1;
                }
                #endregion

                #region Merit point smt 2
                var pointMeritSmt2 = listEntryMeritStudentSmt2.Where(e => e.idHomeroomStudent == student.IdHomeroomStudentSmt2).Sum(e => e.point);
                newPointMerit += pointMeritSmt2;
                #endregion

                #region Update/insert student point
                var updatePointStudentSmt2 = listStudentPointSmt2.Where(e => e.IdHomeroomStudent == student.IdHomeroomStudentSmt2).FirstOrDefault();
                if (updatePointStudentSmt2 != null)
                {
                    if (updatePointStudentSmt2.MeritPoint == newPointMerit 
                            && updatePointStudentSmt2.DemeritPoint == newPointDemerit 
                            && updatePointStudentSmt2.IdSanctionMapping == idSanctionMapping
                            && updatePointStudentSmt2.IdLevelOfInteraction == idLevelOfInteraction)
                        continue;
                    else
                    {
                        updatePointStudentSmt2.MeritPoint = newPointMerit;
                        updatePointStudentSmt2.DemeritPoint = newPointDemerit;
                        updatePointStudentSmt2.IdSanctionMapping = idSanctionMapping;
                        updatePointStudentSmt2.IdLevelOfInteraction = idLevelOfInteraction;
                        _dbContext.Entity<TrStudentPoint>().Update(updatePointStudentSmt2);
                    }
                }
                else
                {
                    TrStudentPoint newStudentPoint = new TrStudentPoint
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdHomeroomStudent = student.IdHomeroomStudentSmt2,
                        DemeritPoint = newPointDemerit,
                        MeritPoint = newPointMerit,
                        IdLevelOfInteraction = idLevelOfInteraction,
                        IdSanctionMapping = idSanctionMapping
                    };

                    _dbContext.Entity<TrStudentPoint>().Add(newStudentPoint);
                }
                #endregion
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }

    public class GetStudent
    {
        public string IdStudent { get; set; }
        public string IdHomeroomStudentSmt1 { get; set; }
        public string IdHomeroomStudentSmt2 { get; set; }
        public string IdGrade { get; set; }
    }
}
