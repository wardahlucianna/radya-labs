using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularDetailHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetMasterExtracurricularDetailRequest.Id) };

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetMasterExtracurricularDetailHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMasterExtracurricularDetailRequest>(_requiredParams);

            var isExtracurricularExist = await _dbContext.Entity<MsExtracurricular>()
                    .Where(x => x.Id == param.Id)
                    .SingleOrDefaultAsync(CancellationToken);

            if (isExtracurricularExist == null)
                throw new BadRequestException($"Extracurricular not exists");

            var queryRaw = _dbContext.Entity<MsExtracurricular>()
                .Include(x => x.ExtracurricularSpvCoach)
                    .ThenInclude(y => y.ExtracurricularCoachStatus)
                .Include(x => x.ExtracurricularGroup)
                .Include(x => x.ExtracurricularGradeMappings)
                    .ThenInclude(y => y.Grade)
                    .ThenInclude(z => z.Level)
                    .ThenInclude(zz => zz.AcademicYear)
                .Include(x => x.ExtracurricularSessionMappings)
                    .ThenInclude(y => y.ExtracurricularSession)
                    .ThenInclude(z => z.Day)
                .Include(x => x.ExtracurricularSessionMappings)
                    .ThenInclude(y => y.ExtracurricularSession)
                    .ThenInclude(z => z.Venue)
                .Include(x => x.ExtracurricularExtCoachMappings)
                    .ThenInclude(y => y.ExtracurricularExternalCoach)
                .Include(x => x.ExtracurricularScoreCompMappings)
                    .ThenInclude(x => x.ExtracurricularScoreCompCategory)
                .Include(x => x.ExtracurricularScoreLegendMappings)
                    .ThenInclude(x => x.ExtracurricularScoreLegendCategory)
                .Include(x => x.ExtracurricularType)
                .Where(x => x.Id == param.Id);

            var extracurricularGradeMappingRawList = queryRaw
                                                        .SelectMany(y => y.ExtracurricularGradeMappings)
                                                        .Select(y => y.IdGrade)
                                                        .ToList();

            var extracurricularRule = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                                    .Include(ergm => ergm.ExtracurricularRule)
                                                    .Where(x => extracurricularGradeMappingRawList.Any(y => y == x.IdGrade) &&
                                                                x.ExtracurricularRule.Status == true
                                                            )
                                                    .FirstOrDefaultAsync(CancellationToken);

            bool inRegistrationPeriod = extracurricularRule == null ? false : (_dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.RegistrationStartDate && _dateTime.ServerTime <= extracurricularRule.ExtracurricularRule.RegistrationEndDate);

            var query = await queryRaw 
                .Select(x => new GetMasterExtracurricularDetailResult
                {
                    AcademicYear = x.ExtracurricularGradeMappings.Count != 0 ? 
                        new ItemValueVm { 
                            Id = x.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Id).FirstOrDefault(), 
                            Description = x.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Description).FirstOrDefault(),
                        } : null,
                    Level = x.ExtracurricularGradeMappings.Count != 0 ?
                        x.ExtracurricularGradeMappings.Select( y => 
                        new ItemValueVm
                        {
                            Id = y.Grade.Level.Id,
                            Description = y.Grade.Level.Description
                        }).ToList() : null,
                    Grade = x.ExtracurricularGradeMappings.Count != 0 ?
                        x.ExtracurricularGradeMappings.OrderBy(x => x.Grade.OrderNumber).ThenBy(x => x.Grade.Description).Select(y =>
                        new ItemValueVm
                        {
                            Id = y.Grade.Id,
                            Description = y.Grade.Description,
                        }).ToList() : null,
                    Semester = new ItemValueVm { Id = x.Semester.ToString(), Description = x.Semester.ToString() },
                    Id = x.Id,
                    Name = x.Name,
                    ExtracurricularGroup = new ItemValueVm { Id = x.ExtracurricularGroup.Id, Description = x.ExtracurricularGroup.Name },
                    Description = x.Description,
                    ShowAttendanceRC = x.ShowAttendanceRC,
                    ShowScoreRC = x.ShowScoreRC,
                    IsRegularSchedule = x.IsRegularSchedule,
                    ScheduleList = x.ExtracurricularSessionMappings.Select( y => new ScheduleExtracurricularDetail
                                { 
                                    Id = y.IdExtracurricularSession,
                                    Day = new ItemValueVm { Id = y.ExtracurricularSession.Day.Id, Description = y.ExtracurricularSession.Day.Description},
                                    Venue = new ItemValueVm { Id = y.ExtracurricularSession.Venue.Id, Description = y.ExtracurricularSession.Venue.Description },
                                    StartTime = y.ExtracurricularSession.StartTime,
                                    EndTime = y.ExtracurricularSession.EndTime,
                                }).ToList(),
                    ElectivesStartDate = x.ElectivesStartDate,
                    ElectivesEndDate = x.ElectivesEndDate,
                    AttendanceStartDate = x.AttendanceStartDate,
                    AttendanceEndDate = x.AttendanceEndDate,
                    MinParticipant = x.MinParticipant,
                    MaxParticipant = x.MaxParticipant,
                    ScoreStartDate = x.ScoreStartDate == null ? null : x.ScoreStartDate,
                    ScoreEndDate = x.ScoreEndDate == null ? null : x.ScoreEndDate,
                    Price = x.Price,
                    NeedObjective = x.NeedObjective,
                    ReviewDate = extracurricularRule == null ? null : extracurricularRule.ExtracurricularRule.ReviewDate,
                    InRegistrationPeriod = inRegistrationPeriod,
                    CoachSupervisorList = x.ExtracurricularSpvCoach.Select( y => new CoachSupervisorExtracurricularDetail
                               {
                                   Id = y.IdBinusian,
                                   Description = NameUtil.GenerateFullName(y.Staff.FirstName, y.Staff.LastName),
                                   IsSpv = (y.IsSpv || y.ExtracurricularCoachStatus.Code == "SPV"),
                                   IdExtracurricularCoachStatus = y.IdExtracurricularCoachStatus,
                                   ExtracurricularCoachStatusDesc = y.ExtracurricularCoachStatus.Description
                    }).ToList(),
                    ExternalCoachList = x.ExtracurricularExtCoachMappings.Select(y => new ExternalCoachExtracurricularDetail
                    {
                        Id = y.IdExtracurricularExternalCoach,
                        Description = y.ExtracurricularExternalCoach.Name                      
                    }).ToList(),
                    ScoreComponentCategory = x.ExtracurricularScoreCompMappings
                                                .Select(x => new ItemValueVm
                                                {
                                                    Id = x.IdExtracurricularScoreCompCategory,
                                                    Description = x.ExtracurricularScoreCompCategory.Description
                                                })
                                                .FirstOrDefault(),
                    ScoreLegendMapping = x.ExtracurricularScoreLegendMappings
                                            .Select(x => new ScoreLegendMappingExtracurricularDetail
                                            {
                                                Id = x.IdExtracurricularScoreLegendCategory,
                                                Description = x.ExtracurricularScoreLegendCategory.Description,
                                                IdExtracurricularScoreLegendCategory = x.IdExtracurricularScoreLegendCategory
                                            })
                                            .FirstOrDefault(),
                    //ScoreLegendMapping = x.ExtracurricularScoreLegendMappings
                    //                            .Select(x => new ItemValueVm
                    //                            {
                    //                                Id = x.IdExtracurricularScoreLegendCategory,
                    //                                Description = x.ExtracurricularScoreLegendCategory.Description
                    //                            })
                    //                            .FirstOrDefault(),
                    //IdExtracurricularScoreLegendCategory = x.ExtracurricularScoreLegendMappings
                    //    .Select(x => x.IdExtracurricularScoreLegendCategory)
                    //    .FirstOrDefault()
                    ExtracurricularType = new ItemValueVm
                    {
                        Id = x.IdExtracurricularType,
                        Description = x.ExtracurricularType.Description
                    },
                    ShowParentStudent = x.ShowParentStudent
                })
                .SingleOrDefaultAsync(CancellationToken);

            query.Level = query.Level.Distinct(new DataComparerItemValue()).ToList();

            return Request.CreateApiResult2(query as object);
        }

        private class DataComparerItemValue : IEqualityComparer<ItemValueVm>
        {
            public bool Equals(ItemValueVm x, ItemValueVm y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the data' properties are equal.
                return x.Id == y.Id && x.Description == y.Description;
            }

            // If Equals() returns true for a pair of objects
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(ItemValueVm data)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(data, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashDataId = data.Id == null ? 0 : data.Id.GetHashCode();

                //Get hash code for the Code field.
                int hashDataDesc = data.Description.GetHashCode();

                //Calculate the hash code for the data.
                return hashDataId ^ hashDataDesc;
            }
        }

    }
}
