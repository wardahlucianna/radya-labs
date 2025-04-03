using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetMasterParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetMasterParticipantHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetMasterParticipantRequest, GetMasterParticipantValidator>();

            var extracurricularDataList = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                                    .Include(egm => egm.Extracurricular)
                                    .ThenInclude(e => e.ExtracurricularGroup)
                                    .Include(egm => egm.Grade)
                                    .ThenInclude(g => g.Level)
                                    .ThenInclude(l => l.AcademicYear)
                                    .Where(x => x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
                                                x.Extracurricular.Semester == param.Semester &&
                                                (string.IsNullOrEmpty(param.IdLevel) ? true == true : x.Grade.Level.Id == param.IdLevel) &&
                                                (string.IsNullOrEmpty(param.IdGrade) ? true == true : x.Grade.Id == param.IdGrade) &&
                                                (string.IsNullOrEmpty(param.IdExtracurricular) ? true == true : x.IdExtracurricular == param.IdExtracurricular)
                                                )
                                    .Select(x => new
                                    {
                                        Extracurricular = new NameValueVm
                                        {
                                            Id = x.Extracurricular.Id,
                                            Name = x.Extracurricular.Name
                                        },
                                        ExtracurricularGroup = new NameValueVm
                                        {
                                            Id = x.Extracurricular.ExtracurricularGroup.Id,
                                            Name = x.Extracurricular.ExtracurricularGroup.Name
                                        },
                                        AcademicYear = new ItemValueVm
                                        {
                                            Id = param.IdAcademicYear,
                                            Description = x.Grade.Level.AcademicYear.Description
                                        },
                                        Category = x.Extracurricular.Category,
                                        Status = x.Extracurricular.Status,
                                        Semester = x.Extracurricular.Semester,
                                        ParticipantMin = x.Extracurricular.MinParticipant,
                                        ParticipantMax = x.Extracurricular.MaxParticipant,
                                        StartDate = x.Extracurricular.ElectivesStartDate,
                                        EndDate = x.Extracurricular.ElectivesEndDate,
                                        DefaultPrice = x.Extracurricular.Price
                                    })
                                    .Distinct()
                                    .OrderBy(x => x.AcademicYear.Description)
                                    .ThenBy(x => x.Semester)
                                    .ThenBy(x => x.Extracurricular.Name)
                                    .ToListAsync(CancellationToken);

            // get list grade
            var levelGradeRawList = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                                                    .Include(egm => egm.Grade)
                                                    .ThenInclude(g => g.Level)
                                                    .ThenInclude(l => l.AcademicYear)
                                                    .Where(x => x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
                                                                extracurricularDataList.Select(y => y.Extracurricular.Id).Any(y => y == x.IdExtracurricular))
                                                    .Select(x => new
                                                    {
                                                        IdExtracurricular = x.IdExtracurricular,
                                                        Level = new ItemValueVm
                                                        {
                                                            Id = x.Grade.Level.Id,
                                                            Description = x.Grade.Level.Description
                                                        },
                                                        Grade = new ItemValueVm
                                                        {
                                                            Id = x.Grade.Id,
                                                            Description = x.Grade.Description
                                                        }
                                                    })
                                                    .Distinct()
                                                    .ToListAsync();

            var homeroomRawList = await _dbContext.Entity<MsHomeroom>()
                                        .Include(h => h.GradePathwayClassroom)
                                        .ThenInclude(gpc => gpc.Classroom)
                                        .Include(h => h.Grade)
                                        .Where(x => levelGradeRawList.Select(y => y.Grade.Id).Any(y => y == x.IdGrade) &&
                                                    x.Semester == param.Semester)
                                        .Select(x => new
                                        {
                                            IdGrade = x.IdGrade,
                                            Homeroom = new ItemValueVm
                                            {
                                                Id = x.Id,
                                                Description = x.Grade.Code + " " + x.GradePathwayClassroom.Classroom.Code
                                            }
                                        })
                                        .OrderBy(x => x.Homeroom.Description)
                                        .ToListAsync(CancellationToken);

            // get list schedule day time
            var scheduleDayTimeRawList = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                                                .Include(esm => esm.ExtracurricularSession)
                                                .ThenInclude(es => es.Day)
                                                .Where(x => extracurricularDataList.Select(y => y.Extracurricular.Id).Any(y => y == x.IdExtracurricular))
                                                .OrderByDescending(x => x.ExtracurricularSession.Day.Code)
                                                .ThenBy(x => x.ExtracurricularSession.StartTime)
                                                .ThenBy(x => x.ExtracurricularSession.EndTime)
                                                .Select(x => new
                                                {
                                                    IdExtracurricular = x.IdExtracurricular,
                                                    Day = x.ExtracurricularSession.Day.Description,
                                                    StartTime = x.ExtracurricularSession.StartTime,
                                                    EndTime = x.ExtracurricularSession.EndTime,
                                                })
                                                .ToListAsync();

            List<GetMasterParticipantResult> resultList = new List<GetMasterParticipantResult>();

            foreach (var itemExtracurricular in extracurricularDataList)
            {
                // get list grade
                var levelGradeList = levelGradeRawList
                                                    .Where(x => x.IdExtracurricular == itemExtracurricular.Extracurricular.Id)
                                                    .Select(x => new LevelGrade
                                                    {
                                                        Level = new ItemValueVm
                                                        {
                                                            Id = x.Level.Id,
                                                            Description = x.Level.Description
                                                        },
                                                        Grade = new ItemValueVm
                                                        {
                                                            Id = x.Grade.Id,
                                                            Description = x.Grade.Description
                                                        },
                                                        HomeroomList = homeroomRawList
                                                                        .Where(y => y.IdGrade == x.Grade.Id)
                                                                        .Select(y => new ItemValueVm
                                                                        {
                                                                            Id = y.Homeroom.Id,
                                                                            Description = y.Homeroom.Description
                                                                        })
                                                                        .ToList()
                                                    })
                                                    .Distinct()
                                                    .ToList();

                // get list schedule day time
                var scheduleDayTimeList = scheduleDayTimeRawList
                                                    .Where(x => x.IdExtracurricular == itemExtracurricular.Extracurricular.Id)
                                                    .Select(x => new DayTimeSchedule
                                                    {
                                                        Day = x.Day,
                                                        StartTime = x.StartTime,
                                                        EndTime = x.EndTime,
                                                    })
                                                    .ToList();

                // get total participants
                int totalParticipant = _dbContext.Entity<MsExtracurricularParticipant>()
                                        .Where(x => x.IdExtracurricular == itemExtracurricular.Extracurricular.Id)
                                        .Select(x => x.IdStudent)
                                        .Distinct()
                                        .Count();

                // insert to body
                GetMasterParticipantResult body = new GetMasterParticipantResult
                {
                    Extracurricular = itemExtracurricular.Extracurricular,
                    ExtracurricularGroup = itemExtracurricular.ExtracurricularGroup,
                    Category = itemExtracurricular.Category,
                    Status = itemExtracurricular.Status,
                    AcademicYear = itemExtracurricular.AcademicYear,
                    Semester = itemExtracurricular.Semester,
                    LevelGradeList = levelGradeList,
                    ScheduleDayTimeList = scheduleDayTimeList,
                    StartDate = itemExtracurricular.StartDate,
                    EndDate = itemExtracurricular.EndDate,
                    //TotalSession = 0,
                    ParticipantMin = itemExtracurricular.ParticipantMin,
                    ParticipantMax = itemExtracurricular.ParticipantMax,
                    TotalParticipant = totalParticipant,
                    DefaultPrice = itemExtracurricular.DefaultPrice
                };

                resultList.Add(body);
            }

            var columns = new[] { "academicYear", "semester", "extracurricularName", "category", "minParticipant" };
            var aliasColumns = new Dictionary<string, string>
                {
                    { columns[0], "AcademicYear.Id" },
                    { columns[1], "Semester" },
                    { columns[2], "Extracurricular.Name" },
                    { columns[3], "Category" },
                    { columns[4], "ParticipantMin" }
                };

            IReadOnlyList<GetMasterParticipantResult> items;
            if (param.Return == CollectionType.Lov)
                items = resultList.AsQueryable().OrderByDynamic(param, aliasColumns).ToList();
            else
                items = resultList.AsQueryable().OrderByDynamic(param, aliasColumns).SetPagination(param).ToList();

            var resultCount = resultList.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(resultCount));
        }
    }
}
