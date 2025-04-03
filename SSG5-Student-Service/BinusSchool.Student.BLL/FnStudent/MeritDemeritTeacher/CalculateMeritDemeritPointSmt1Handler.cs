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
    public class CalculateMeritDemeritPointSmt1Handler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly ILogger<CalculateMeritDemeritPointSmt1Handler> _logger;
        private readonly IMachineDateTime _datetime;

        public CalculateMeritDemeritPointSmt1Handler(IStudentDbContext EntryMeritDemetitDbContext, ILogger<CalculateMeritDemeritPointSmt1Handler> logger, IMachineDateTime datetime)
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
                                       .Where(e => e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear && e.HomeroomStudent.Homeroom.Semester == 1)
                                       .ToListAsync(CancellationToken);

            var listStudentPointSmt1 = listStudentPoint.Where(e => e.HomeroomStudent.Homeroom.Semester == 1).ToList();
            // var listStudentPointSmt1 = listStudentPoint.Where(e => e.HomeroomStudent.Homeroom.Semester == 2).ToList();
            var listIdStudent = listStudentPoint.Select(e => e.HomeroomStudent.IdStudent).Distinct().ToList();

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(e=>e.Homeroom)
                                        .Where(e => listIdStudent.Contains(e.IdStudent) && e.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear && e.Homeroom.Semester == 1)
                                        .Select(e => new
                                        {
                                            e.Homeroom.Semester,
                                            idHomeroomStudent = e.Id,
                                            e.IdStudent,
                                            e.Homeroom.IdGrade
                                        })
                                        .ToListAsync(CancellationToken);

            var listIdHomeroomStudentSmt1 = listHomeroomStudent.Where(e => e.Semester == 1).Select(e => e.idHomeroomStudent).ToList();
            // var listIdHomeroomStudentSmt2 = listHomeroomStudent.Where(e => e.Semester == 2).Select(e => e.idHomeroomStudent).ToList();

            List<GetStudent> ListStudent = new List<GetStudent>();
            foreach (var idStudent in listIdStudent)
            {
                var IdHomeroomStudentSmt1 = listHomeroomStudent
                                                .Where(e => e.Semester == 1 && e.IdStudent==idStudent)
                                                .Select(e => e.idHomeroomStudent)
                                                .FirstOrDefault();

                // var IdHomeroomStudentSmt2 = listHomeroomStudent
                //                                 .Where(e => e.Semester == 2 && e.IdStudent == idStudent)
                //                                 .Select(e => e.idHomeroomStudent)
                //                                 .FirstOrDefault();

                var idGrade = listHomeroomStudent
                                .Where(e => e.IdStudent == idStudent)
                                .Select(e => e.IdGrade)
                                .FirstOrDefault();

                if (IdHomeroomStudentSmt1 != null)
                {
                    ListStudent.Add(new GetStudent
                    {
                        IdStudent = idStudent,
                        IdHomeroomStudentSmt1 = IdHomeroomStudentSmt1 == null ? "" : IdHomeroomStudentSmt1,
                        // idHomeroomStudentSmt2 = IdHomeroomStudentSmt2 == null ? "" : IdHomeroomStudentSmt2,
                        IdGrade = idGrade == null ? "" : idGrade
                    });
                }
            }

            var listEntryMeritStudentSmt1 = await _dbContext.Entity<TrEntryMeritStudent>()
                                        .Where(e => listIdHomeroomStudentSmt1.Contains(e.IdHomeroomStudent) && e.Status == "Approved")
                                        .Select(e => new
                                        {
                                            point = e.Point,
                                            idHomeroomStudent = e.IdHomeroomStudent,
                                        })
                                        .ToListAsync(CancellationToken);
            
            var listEntryDemerit = await _dbContext.Entity<TrEntryDemeritStudent>()
                                    .Include(e => e.MeritDemeritMapping).ThenInclude(e => e.LevelOfInteraction)
                                    .Where(e => listIdStudent.Contains(e.HomeroomStudent.IdStudent) && e.Status == "Approved")
                                    .Select(e => new
                                    {
                                        point = e.Point,
                                        idHomeroomStudent = e.IdHomeroomStudent,
                                        idLevelOfOnfraction = e.MeritDemeritMapping.IdLevelOfInteraction,
                                    })
                                    .ToListAsync(CancellationToken);

            var listEntryDemeritStudentSmt1 = listEntryDemerit
                                                .Where(e => listIdHomeroomStudentSmt1.Contains(e.idHomeroomStudent))
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
                                                   e.ScoreContinueOption
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
                var newPointDemerit = 0;
                var newPointMerit = 0;

                newPointDemerit = listEntryDemeritStudentSmt1.Where(e => e.idHomeroomStudent == student.IdHomeroomStudentSmt1).Sum(e => e.point);
                newPointMerit = listEntryMeritStudentSmt1.Where(e => e.idHomeroomStudent == student.IdHomeroomStudentSmt1).Sum(e => e.point);

                #region Get level of infraction
                string idLevelOfInteraction = null;
                if (listEntryDemeritStudentSmt1.Any())
                {
                    var listIdLevelOfOnfraction = listEntryDemeritStudentSmt1.Where(e=>e.idHomeroomStudent==student.IdHomeroomStudentSmt1).Select(e => e.idLevelOfOnfraction).ToList();
                    var listLevelOfInfractionById = GetLevelOfInfraction.Where(e => listIdLevelOfOnfraction.Contains(e.idLevelOfInfraction)).ToList();
                    idLevelOfInteraction = listLevelOfInfractionById.OrderBy(e => e.nameLevelOfInfraction).Select(e => e.idLevelOfInfraction).LastOrDefault();
                }
                #endregion

                #region Sunction
                var idSanctionMapping = listSanctionMapping.Where(e => newPointDemerit >= e.Min && newPointDemerit <= e.Max).Select(e=>e.Id).FirstOrDefault();
                #endregion

                #region Update/insert student point
                var updatePointStudentSmt1 = listStudentPointSmt1.Where(e => e.IdHomeroomStudent == student.IdHomeroomStudentSmt1).FirstOrDefault();
                if (updatePointStudentSmt1 != null)
                {
                    if (updatePointStudentSmt1.MeritPoint == newPointMerit
                            && updatePointStudentSmt1.DemeritPoint == newPointDemerit
                            && updatePointStudentSmt1.IdSanctionMapping == idSanctionMapping
                            && updatePointStudentSmt1.IdLevelOfInteraction == idLevelOfInteraction)
                        continue;
                    else
                    {
                        updatePointStudentSmt1.MeritPoint = newPointMerit;
                        updatePointStudentSmt1.DemeritPoint = newPointDemerit;
                        updatePointStudentSmt1.IdSanctionMapping = idSanctionMapping;
                        updatePointStudentSmt1.IdLevelOfInteraction = idLevelOfInteraction;
                        _dbContext.Entity<TrStudentPoint>().Update(updatePointStudentSmt1);
                    }
                }
                else
                {
                    TrStudentPoint newStudentPoint = new TrStudentPoint
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdHomeroomStudent = student.IdHomeroomStudentSmt1,
                        DemeritPoint= newPointDemerit,
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

    public class GetStudentSmt1
    {
        public string idStudent { get; set; }
        public string idHomeroomStudentSmt1 { get; set; }
        // public string idHomeroomStudentSmt2 { get; set; }
        public string idGrade { get; set; }
    }
}
