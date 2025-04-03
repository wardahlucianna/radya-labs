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
    public class GetMasterParticipantHandlerV2 : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetMasterParticipantHandlerV2(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetMasterParticipantRequestV2, GetMasterParticipantValidatorV2>();

            var extracurricularDataList = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                                    .Include(egm => egm.Extracurricular)
                                    .ThenInclude(e => e.ExtracurricularGroup)
                                    .Include(egm => egm.Grade)
                                    .ThenInclude(g => g.Level)
                                    .ThenInclude(l => l.AcademicYear)
                                    .Where(x => x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
                                                x.Extracurricular.Semester == (param.Semester != null ? param.Semester : x.Extracurricular.Semester) &&
                                                (param.IdLevelList == null ? true == true : (param.IdLevelList.Contains(x.Grade.IdLevel))) &&
                                                (param.IdGradeList == null ? true == true : (param.IdGradeList.Contains(x.IdGrade))) &&
                                                //(string.IsNullOrEmpty(param.IdLevel) ? true == true : x.Grade.Level.Id == param.IdLevel) &&
                                                //(string.IsNullOrEmpty(param.IdGrade) ? true == true : x.Grade.Id == param.IdGrade) &&
                                                (string.IsNullOrEmpty(param.IdExtracurricular) ? true == true : x.IdExtracurricular == param.IdExtracurricular) &&
                                                x.Extracurricular.Status == (param.Status != null ? param.Status : x.Extracurricular.Status)
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
                                    .OrderBy(x => x.ExtracurricularGroup.Name)
                                    .ThenBy(x => x.Semester)
                                    .ThenBy(x => x.Extracurricular.Name)
                                    .ThenBy(x => x.StartDate)
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
                                                        },
                                                        GradeOrderNo = x.Grade.OrderNumber
                                                    })
                                                    .Distinct()
                                                    .OrderBy(x => x.GradeOrderNo)
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
                                                    IdDay = x.ExtracurricularSession.Day.Id
                                                })
                                                .OrderBy(x => x.IdDay)
                                                .ToListAsync();

            List<GetMasterParticipantResultV2> resultList = new List<GetMasterParticipantResultV2>();

            foreach (var itemExtracurricular in extracurricularDataList)
            {
                // get list grade
                var levelGradeList = levelGradeRawList
                                                    .Where(x => x.IdExtracurricular == itemExtracurricular.Extracurricular.Id)
                                                    .Select(x => new LevelGradeV2
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
                                                                        .ToList(),
                                                        GradeOrderNo = x.GradeOrderNo
                                                    })
                                                    .Distinct()
                                                    .ToList();

                // get list schedule day time
                var scheduleDayTimeList = scheduleDayTimeRawList
                                                    .Where(x => x.IdExtracurricular == itemExtracurricular.Extracurricular.Id)
                                                    .Select(x => new DayTimeScheduleV2
                                                    {
                                                        DayName = x.Day,
                                                        IdDay = x.IdDay,
                                                        StartTime = x.StartTime,
                                                        EndTime = x.EndTime
                                                    })
                                                    .ToList();

                // get total participants
                int totalParticipant = _dbContext.Entity<MsExtracurricularParticipant>()
                                        .Where(x => x.IdExtracurricular == itemExtracurricular.Extracurricular.Id)
                                        .Select(x => x.IdStudent)
                                        .Distinct()
                                        .Count();

                // insert to body
                GetMasterParticipantResultV2 body = new GetMasterParticipantResultV2
                {
                    Extracurricular = itemExtracurricular.Extracurricular,
                    ExtracurricularGroup = itemExtracurricular.ExtracurricularGroup,
                    Category = itemExtracurricular.Category,
                    Status = itemExtracurricular.Status,
                    AcademicYear = itemExtracurricular.AcademicYear,
                    Semester = itemExtracurricular.Semester,
                    LevelGradeList = levelGradeList,
                    GradeOrderNo = levelGradeList.FirstOrDefault().GradeOrderNo,
                    ScheduleDayTimeList = scheduleDayTimeList.Count() == 0 ? null : scheduleDayTimeList,
                    IdDay = scheduleDayTimeList.Count() == 0 ? null : scheduleDayTimeList.FirstOrDefault().IdDay,
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

            var columns = new[] { "electiveGroup", "semester", "extracurricularName", "levelGradeList", "scheduleDayTimeList", "totalParticipant", "participantMin", "status" };
            //var columns = new[] { "academicYear", "semester", "extracurricularName", "category", "minParticipant", "electiveGroup", "totalParticipant" };
            var aliasColumns = new Dictionary<string, string>
                {
                    { columns[0], "ExtracurricularGroup.Name" },
                    { columns[1], "Semester" },
                    { columns[2], "Extracurricular.Name" },
                    { columns[3], "GradeOrderNo" },
                    { columns[4], "IdDay" },
                    { columns[5], "TotalParticipant" },
                    { columns[6], "ParticipantMin" },
                    { columns[7], "Status" }
                };

            IReadOnlyList<GetMasterParticipantResultV2> items;
            if (param.Return == CollectionType.Lov)
                items = resultList.AsQueryable().OrderByDynamic(param, aliasColumns).ToList();
            else
                items = resultList.AsQueryable().OrderByDynamic(param, aliasColumns).SetPagination(param).ToList();

            var resultCount = resultList.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(resultCount));
        }
    }
}
