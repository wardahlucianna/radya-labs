using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using BinusSchool.Common.Model.Enums;
using System.Collections.Generic;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class UpdateMasterExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IUserRole _userRoleApi;


        public UpdateMasterExtracurricularHandler(ISchedulingDbContext dbContext,
            IMachineDateTime dateTime,
            IUserRole userRoleApi)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _userRoleApi = userRoleApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateMasterExtracurricularRequest, UpdateMasterExtracurricularValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);


                var isExtracurricularExist = await _dbContext.Entity<MsExtracurricular>()
                                        .Include(x => x.ExtracurricularGroup)
                                        .Include(x => x.ExtracurricularGradeMappings)
                                            .ThenInclude(x => x.Grade)
                                            .ThenInclude(x => x.Level)
                                        .Where(x => x.Id == body.IdExtracurricular)
                                        .FirstOrDefaultAsync(CancellationToken);

                if (isExtracurricularExist == null)
                    throw new BadRequestException($"Extracurricular with Id : {body.IdExtracurricular} not exists");


                var IdSchool = isExtracurricularExist.ExtracurricularGradeMappings.First().Grade.Level.IdAcademicYear;

                if (body.UpdateAll)
                {
                    var extracurricularSpvCoach = await _dbContext.Entity<MsExtracurricularSpvCoach>()
                           .Include(x => x.ExtracurricularCoachStatus)
                           .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                           .ToListAsync(CancellationToken);

                    var isExtracurricularAttendanceEntriesExist = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                                                       .Include(x => x.ExtracurricularGeneratedAtt)
                                                       .Where(x => x.ExtracurricularGeneratedAtt.IdExtracurricular == body.IdExtracurricular)
                                                       .ToListAsync(CancellationToken);

                    if (isExtracurricularAttendanceEntriesExist.Count() > 0)
                    {
                        // Extracurricular Group
                        var isExtracurricularGroupExist = await _dbContext.Entity<MsExtracurricularGroup>()
                                .Where(x => x.Id == body.IdExtracurricularGroup)
                                .FirstOrDefaultAsync(CancellationToken);
                        if (isExtracurricularGroupExist == null)
                            throw new BadRequestException($"ExtracurricularGroup {body.IdExtracurricularGroup} not exists");

                        isExtracurricularExist.Name = body.ExtracurricularName;
                        isExtracurricularExist.Description = body.ExtracurricularDescription;
                        isExtracurricularExist.ShowParentStudent = body.ShowParentStudent;
                        isExtracurricularExist.ShowAttendanceRC = body.IsShowAttendanceReportCard;
                        isExtracurricularExist.ShowScoreRC = body.IsShowScoreReportCard;
                        isExtracurricularExist.MinParticipant = body.ParticipantMin;
                        isExtracurricularExist.MaxParticipant = body.ParticipantMax;
                        isExtracurricularExist.ScoreStartDate = body.ScoringStartDate == null ? null : body.ScoringStartDate;
                        isExtracurricularExist.ScoreEndDate = body.ScoringEndDate == null ? null : body.ScoringEndDate;
                        isExtracurricularExist.Price = body.Price == null ? isExtracurricularExist.Price : body.Price.Value;
                        isExtracurricularExist.NeedObjective = body.NeedObjective;
                        isExtracurricularExist.IdExtracurricularType = body.IdExtracurricularType;
                        isExtracurricularExist.IdExtracurricularGroup = isExtracurricularGroupExist.Id;

                        #region Update Extracurricular to SpvCoach 
                        var getSpvCoachToDelete = extracurricularSpvCoach
                            .Where(ex => body.CoachSupervisorList.All(ex2 => ex2.IdUser != ex.IdBinusian))
                            .Select(x => x.IdBinusian).ToList();

                        var getSpvCoachToAdd = body.CoachSupervisorList
                            .Where(ex => extracurricularSpvCoach.All(ex2 => ex2.IdBinusian != ex.IdUser))
                            .Select(x => new { IdBinusian = x.IdUser, IsCoach = (x.IdExtracurricularCoachStatus != "1"), IdExtracurricularCoachStatus = x.IdExtracurricularCoachStatus }).ToList();

                        #region Unused Code
                        //var getSpvCoachAlmostEqual = extracurricularSpvCoach
                        //   .Where(ex => !body.CoachSupervisorList.All(ex2 => ex2.IdUser == ex.IdBinusian && ex2.IdExtracurricularCoachStatus != ex.IdExtracurricularCoachStatus))
                        //   .ToList();
                        #endregion

                        var getSpvCoachAlmostEqual = extracurricularSpvCoach
                           .Where(ex => !getSpvCoachToDelete.Contains(ex.IdBinusian) && !body.CoachSupervisorList.All(ex2 => ex2.IdUser == ex.IdBinusian && ex2.IdExtracurricularCoachStatus != ex.IdExtracurricularCoachStatus))
                           .ToList();

                        // Delete spvCoach
                        foreach (var SpvCoach in getSpvCoachToDelete)
                        {
                            var deleteSpvCoach = extracurricularSpvCoach.Where(x => x.IdBinusian == SpvCoach).SingleOrDefault();

                            if (deleteSpvCoach != null)
                                deleteSpvCoach.IsActive = false;
                            _dbContext.Entity<MsExtracurricularSpvCoach>().Update(deleteSpvCoach);
                        }

                        // Add spvCoach
                        foreach (var SpvCoach in getSpvCoachToAdd)
                        {
                            var paramSpvCoach = new MsExtracurricularSpvCoach
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdBinusian = SpvCoach.IdBinusian,
                                IdExtracurricular = isExtracurricularExist.Id,
                                IsSpv = (SpvCoach.IdExtracurricularCoachStatus == "1"),
                                IdExtracurricularCoachStatus = SpvCoach.IdExtracurricularCoachStatus
                            };
                            _dbContext.Entity<MsExtracurricularSpvCoach>().Add(paramSpvCoach);

                            try
                            {
                                var createRoleSES = await _userRoleApi.AddUserRoleByRoleCode(new AddUserRoleByRoleCodeRequest
                                {
                                    IdUser = SpvCoach.IdBinusian,
                                    RoleCode = "SES",
                                    IdSchool = IdSchool
                                });
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        // Update spvCoach
                        foreach (var SpvCoach in getSpvCoachAlmostEqual)
                        {
                            var newDataSpvCoach = body.CoachSupervisorList
                                .Where(x => x.IdUser == SpvCoach.IdBinusian)
                                .FirstOrDefault();

                            SpvCoach.IsSpv = (newDataSpvCoach.IdExtracurricularCoachStatus == "1");
                            SpvCoach.IdExtracurricularCoachStatus = newDataSpvCoach.IdExtracurricularCoachStatus;
                            _dbContext.Entity<MsExtracurricularSpvCoach>().Update(SpvCoach);
                        }
                        #endregion

                        #region Update Extracurricular to ExtCoach 

                        //Check ExtracurricularExternalCoach
                        var extracurricularExternalCoache = await _dbContext.Entity<MsExtracurricularExternalCoach>()
                            .Where(x => x.IdSchool == isExtracurricularExist.ExtracurricularGroup.IdSchool)
                            .Select(x => x.Id)
                            .ToListAsync(CancellationToken);

                        var paramExternalCoachList = body.ExternalCoachList.Select(x => x.IdExtracurricularExternalCoach).ToList();

                        if (paramExternalCoachList.Count() > 0)
                        {
                            var extracurricularExtCoach = await _dbContext.Entity<MsExtracurricularExtCoachMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .ToListAsync(CancellationToken);

                            var getExtCoachToDelete = extracurricularExtCoach
                               .Where(ex => body.ExternalCoachList.All(ex2 => ex2.IdExtracurricularExternalCoach != ex.IdExtracurricularExternalCoach))
                               .Select(x => x.IdExtracurricularExternalCoach).ToList();

                            var getExtCoachToAdd = body.ExternalCoachList
                                .Where(ex => extracurricularExtCoach.All(ex2 => ex2.IdExtracurricularExternalCoach != ex.IdExtracurricularExternalCoach))
                                .Select(x => new { IdExtracurricularExternalCoach = x.IdExtracurricularExternalCoach }).ToList();

                            // Delete ExtCoach
                            foreach (var ExtCoach in getExtCoachToDelete)
                            {
                                var deleteExtCoach = extracurricularExtCoach.Where(x => x.IdExtracurricularExternalCoach == ExtCoach).SingleOrDefault();

                                if (deleteExtCoach != null)
                                    deleteExtCoach.IsActive = false;
                                _dbContext.Entity<MsExtracurricularExtCoachMapping>().Update(deleteExtCoach);
                            }

                            // Add spvCoach
                            foreach (var ExtCoach in getExtCoachToAdd)
                            {
                                var paramExtCoach = new MsExtracurricularExtCoachMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricularExternalCoach = ExtCoach.IdExtracurricularExternalCoach,
                                    IdExtracurricular = isExtracurricularExist.Id
                                };
                                _dbContext.Entity<MsExtracurricularExtCoachMapping>().Add(paramExtCoach);

                                try
                                {
                                    var createRoleEC = await _userRoleApi.AddUserRoleByRoleCode(new AddUserRoleByRoleCodeRequest
                                    {
                                        IdUser = ExtCoach.IdExtracurricularExternalCoach,
                                        RoleCode = "EC",
                                        IdSchool = IdSchool
                                    });
                                }
                                catch (Exception e)
                                {

                                }
                            }

                            // Update spvCoach
                            //foreach (var SpvCoach in getSpvCoachAlmostEqual)
                            //{
                            //    var updateSpvCoach = extracurricularSpvCoach.Where(x => x.Id == SpvCoach.IdBinusian).SingleOrDefault();
                            //    updateSpvCoach.IsSpv = false;
                            //    updateSpvCoach.IdExtracurricularCoachStatus = SpvCoach.IdExtracurricularCoachStatus;
                            //    _dbContext.Entity<MsExtracurricularSpvCoach>().Update(updateSpvCoach);
                            //}
                        }
                        #endregion

                        #region Update Score Component Mapping
                        var updateExtracurricularScoreCompMapping = await _dbContext.Entity<MsExtracurricularScoreCompMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .FirstOrDefaultAsync(CancellationToken);

                        if (!string.IsNullOrEmpty(body.IdExtracurricularScoreCompCategory))
                        {
                            if (updateExtracurricularScoreCompMapping != null)
                            {
                                updateExtracurricularScoreCompMapping.IdExtracurricularScoreCompCategory = body.IdExtracurricularScoreCompCategory;

                                _dbContext.Entity<MsExtracurricularScoreCompMapping>().Update(updateExtracurricularScoreCompMapping);
                            }
                            else
                            {
                                var addExtracurricularScoreCompMapping = new MsExtracurricularScoreCompMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricular = body.IdExtracurricular,
                                    IdExtracurricularScoreCompCategory = body.IdExtracurricularScoreCompCategory
                                };
                                _dbContext.Entity<MsExtracurricularScoreCompMapping>().Add(addExtracurricularScoreCompMapping);
                            }
                        }
                        else
                        {
                            if (updateExtracurricularScoreCompMapping != null)
                            {
                                _dbContext.Entity<MsExtracurricularScoreCompMapping>().Remove(updateExtracurricularScoreCompMapping);
                            }
                        }

                        #endregion

                        #region Update Score Legend Mapping
                        var updateExtracurricularScoreLegendMapping = await _dbContext.Entity<MsExtracurricularScoreLegendMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .FirstOrDefaultAsync(CancellationToken);

                        if (!string.IsNullOrEmpty(body.IdExtracurricularScoreLegendCategory))
                        {
                            if (updateExtracurricularScoreLegendMapping != null)
                            {
                                updateExtracurricularScoreLegendMapping.IdExtracurricularScoreLegendCategory = body.IdExtracurricularScoreLegendCategory;

                                _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Update(updateExtracurricularScoreLegendMapping);
                            }
                            else
                            {
                                var addExtracurricularScoreLegendMapping = new MsExtracurricularScoreLegendMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricular = body.IdExtracurricular,
                                    IdExtracurricularScoreLegendCategory = body.IdExtracurricularScoreLegendCategory
                                };
                                _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Add(addExtracurricularScoreLegendMapping);
                            }
                        }
                        else
                        {
                            if (updateExtracurricularScoreLegendMapping != null)
                            {
                                _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Remove(updateExtracurricularScoreLegendMapping);
                            }
                        }

                        #endregion

                        #region Update Delete Set Session
                        if (body.IsRegularSchedule == true && body.ScheduleList != null)
                        {
                            var getExtracurricularAttendanceEntry = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                                .Where(x => x.ExtracurricularGeneratedAtt.IdExtracurricular == body.IdExtracurricular)
                                .ToListAsync(CancellationToken);

                            var extracurricularGeneratedAtt = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                                .ToListAsync(CancellationToken);

                            var extracurricularFilteredByAttendance = extracurricularGeneratedAtt // Filter extracurricular session that can't be deleted
                                .Where(x => getExtracurricularAttendanceEntry.Select(x => x.IdExtracurricularGeneratedAtt).ToList().Contains(x.Id))
                                .ToList();

                            var extracurricularSessionMapping = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                                .Include(x => x.ExtracurricularSession.Day)
                                .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                                .ToListAsync(CancellationToken);

                            var getExtracurricularSessionMappingToDelete = extracurricularSessionMapping //DeleteDay
                               .Where(ex => !body.ScheduleList.Any(ex2 => ex2.Day.Id == ex.ExtracurricularSession.IdDay))
                               .ToList();

                            var getExtracurricularSessionMappingToAdd = body.ScheduleList //AddDay
                                .Where(ex => !extracurricularSessionMapping.Any(ex2 => ex2.ExtracurricularSession.IdDay == ex.Day.Id))
                                .ToList();

                            var checkSessionDays = new List<ItemValueVm>();

                            if (getExtracurricularSessionMappingToAdd.Count() > 0)
                            {
                                foreach (var item in getExtracurricularSessionMappingToAdd)
                                {
                                    var paramSession = new MsExtracurricularSession
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdDay = item.Day.Id,
                                        IdVenue = item.Venue.Id,
                                        StartTime = TimeSpan.Parse(item.StartTime),
                                        EndTime = TimeSpan.Parse(item.EndTime)
                                    };
                                    _dbContext.Entity<MsExtracurricularSession>().Add(paramSession);

                                    var paramSessionMapping = new TrExtracurricularSessionMapping
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdExtracurricular = body.IdExtracurricular,
                                        IdExtracurricularSession = paramSession.Id
                                    };
                                    _dbContext.Entity<TrExtracurricularSessionMapping>().Add(paramSessionMapping);

                                    var sessionData = new ItemValueVm
                                    {
                                        Id = paramSession.Id,
                                        Description = item.Day.Description
                                    };
                                    checkSessionDays.Add(sessionData);
                                }
                            }

                            var getExtracurricularSessionMappingToUpdate = extracurricularSessionMapping //Update Time / Venue
                               .Where(ex => body.ScheduleList.Any(ex2 => ex2.Day.Id == ex.ExtracurricularSession.IdDay))
                               .ToList();

                            if (getExtracurricularSessionMappingToUpdate.Count() > 0)
                            {
                                var extracurricularSessionToUpdate = await _dbContext.Entity<MsExtracurricularSession>()
                                    .Where(x => getExtracurricularSessionMappingToUpdate.Select(x => x.IdExtracurricularSession).ToList().Contains(x.Id))
                                    .ToListAsync(CancellationToken);
                                foreach (var item in body.ScheduleList)
                                {
                                    var dataExtracurricularSessionToUpdate = extracurricularSessionToUpdate.Where(x => x.IdDay == item.Day.Id).ToList();
                                    foreach (var itemUpdate in dataExtracurricularSessionToUpdate)
                                    {
                                        if (itemUpdate.IdVenue != item.Venue.Id || itemUpdate.StartTime != TimeSpan.Parse(item.StartTime) || itemUpdate.EndTime != TimeSpan.Parse(item.EndTime))
                                        {
                                            itemUpdate.StartTime = TimeSpan.Parse(item.StartTime);
                                            itemUpdate.EndTime = TimeSpan.Parse(item.EndTime);
                                            itemUpdate.IdVenue = item.Venue.Id;
                                            _dbContext.Entity<MsExtracurricularSession>().Update(itemUpdate);
                                        }
                                    }
                                }
                            }

                            var getExtracurricularGeneratedAttToDelete = extracurricularGeneratedAtt //Delete when date is out of the elective date range + check attendance
                                .Where(x => ((DateTime)x.Date < body.ElectivesStartDate || (DateTime)x.Date > body.ElectivesEndDate) && (!extracurricularFilteredByAttendance.Contains(x)))
                                .ToList();

                            var addDeletedDayExtracurricularGeneratedAtt = extracurricularGeneratedAtt //Delete when session is deleted
                                .Where(x => getExtracurricularSessionMappingToDelete.Select(x => x.IdExtracurricularSession).Contains(x.IdExtracurricularSession))
                                .ToList();

                            var getAttendanceEntryToDelete = getExtracurricularAttendanceEntry //Delete attendance when session is deleted
                                .Where(x => addDeletedDayExtracurricularGeneratedAtt.Select(y => y.Id).Contains(x.IdExtracurricularGeneratedAtt))
                                .ToList();

                            getExtracurricularGeneratedAttToDelete.AddRange(addDeletedDayExtracurricularGeneratedAtt);

                            //Add when date is in the elective date range / add according to SessionToAdd
                            if (getExtracurricularSessionMappingToAdd.Count() > 0 || extracurricularGeneratedAtt.Count() == 0 || extracurricularGeneratedAtt.Select(x => x.Date).Min() >= body.ElectivesStartDate || extracurricularGeneratedAtt.Select(x => x.Date).Max() <= body.ElectivesEndDate)
                            {
                                var listDayExtracurricularSessionMapping = extracurricularSessionMapping
                                .Select(x => new ItemValueVm
                                {
                                    Description = x.ExtracurricularSession.Day.Description,
                                    Id = x.IdExtracurricularSession
                                }).Distinct().ToList();
                                checkSessionDays.AddRange(listDayExtracurricularSessionMapping);
                                checkSessionDays.Distinct();

                                DateTime electivesStartDate = body.ElectivesStartDate;
                                DateTime electivesEndDate = body.ElectivesEndDate;
                                int days = (electivesEndDate - electivesStartDate).Days;

                                for (int i = 0; i <= days; i++)
                                {
                                    var currDate = electivesStartDate.AddDays(i);
                                    if (checkSessionDays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Description)).ToList().Contains(currDate.DayOfWeek))
                                    {
                                        if (!extracurricularGeneratedAtt.Select(x => x.Date).ToList().Contains(currDate))
                                        {
                                            var getExtracurricularGeneratedAttToAdd = new TrExtracurricularGeneratedAtt
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdExtracurricular = body.IdExtracurricular,
                                                IdExtracurricularSession = checkSessionDays.Where(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Description) == currDate.DayOfWeek).Select(x => x.Id).FirstOrDefault(),
                                                Date = currDate,
                                                NewSession = false
                                            };
                                            _dbContext.Entity<TrExtracurricularGeneratedAtt>().Add(getExtracurricularGeneratedAttToAdd);
                                        }
                                    }
                                }
                            }

                            //Delete Session and TrExtracurricularGeneratedAtt and TrExtracurricularAttendanceEntry
                            if (getExtracurricularGeneratedAttToDelete.Count() > 0)
                            {
                                foreach (var item in getExtracurricularGeneratedAttToDelete)
                                {
                                    item.IsActive = false;
                                }
                                _dbContext.Entity<TrExtracurricularGeneratedAtt>().UpdateRange(getExtracurricularGeneratedAttToDelete);
                            }

                            if (getExtracurricularSessionMappingToDelete.Count() > 0)
                            {
                                var extracurricularSessionToDelete = await _dbContext.Entity<MsExtracurricularSession>()
                                    .Where(x => getExtracurricularSessionMappingToDelete.Select(x => x.IdExtracurricularSession).ToList().Contains(x.Id))
                                    .ToListAsync(CancellationToken);
                                foreach (var item in getExtracurricularSessionMappingToDelete)
                                {
                                    item.IsActive = false;
                                }
                                foreach (var item in extracurricularSessionToDelete)
                                {
                                    item.IsActive = false;
                                }
                                _dbContext.Entity<MsExtracurricularSession>().UpdateRange(extracurricularSessionToDelete);
                                _dbContext.Entity<TrExtracurricularSessionMapping>().UpdateRange(getExtracurricularSessionMappingToDelete);
                            }

                            if(getAttendanceEntryToDelete.Count() > 0)
                            {
                                foreach(var item in getAttendanceEntryToDelete)
                                {
                                    item.IsActive = false;
                                }
                                _dbContext.Entity<TrExtracurricularAttendanceEntry>().UpdateRange(getAttendanceEntryToDelete);
                            }

                        }
                        #endregion

                            _dbContext.Entity<MsExtracurricular>().Update(isExtracurricularExist);
                    }
                    else
                    {
                        var queryRaw = _dbContext.Entity<MsExtracurricular>()
                                    .Include(x => x.ExtracurricularGradeMappings)
                                        .ThenInclude(y => y.Grade)
                                        .ThenInclude(z => z.Level)
                                        .ThenInclude(zz => zz.AcademicYear)
                                    .Where(x => x.Id == body.IdExtracurricular);

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

                        //var checkpricecanchanges =  _dbContext.Entity<TrExtracurricularGradeMapping>()
                        //                            .Include(x => x.Extracurricular)
                        //                            .Where(a => a.Extracurricular.Id == body.IdExtracurricular
                        //                            && a.Extracurricular.Price > 0)                                             
                        //                            .Join( _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                        //                                    .Include(x => x.ExtracurricularRule)
                        //                                    .Where(b => b.ExtracurricularRule.RegistrationStartDate < _dateTime.ServerTime &&
                        //                                     (b.ExtracurricularRule.ReviewDate != null ? (b.ExtracurricularRule.ReviewDate > _dateTime.ServerTime) : true) )
                        //                                    .ToList(), e => e.IdGrade, er => er.IdGrade, (r,er) => new { r, er})
                        //                            .Select(c => new { 
                        //                                        c.r.IdExtracurricular
                        //                            })
                        //                            .ToList();

                        // cant change price if extracurricular in registration period
                        bool inRegistrationPeriod = extracurricularRule == null ? false : (_dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.RegistrationStartDate && _dateTime.ServerTime <= extracurricularRule.ExtracurricularRule.RegistrationEndDate);

                        if (inRegistrationPeriod && body.Price != null)
                        {
                            throw new BadRequestException("Cannot change price extracurricular");
                        }

                        //body.GradeList.se
                        var isExtracurricularGroupExist = await _dbContext.Entity<MsExtracurricularGroup>()
                                .Where(x => x.Id == body.IdExtracurricularGroup)
                                .FirstOrDefaultAsync(CancellationToken);
                        if (isExtracurricularGroupExist == null)
                            throw new BadRequestException($"ExtracurricularGroup {body.IdExtracurricularGroup} not exists");

                        DateTime electivesStartDate = body.ElectivesStartDate;
                        DateTime electivesEndDate = body.ElectivesEndDate;
                        int days = (electivesEndDate - electivesStartDate).Days;

                        isExtracurricularExist.Name = body.ExtracurricularName;
                        isExtracurricularExist.Description = body.ExtracurricularDescription;
                        isExtracurricularExist.ShowParentStudent = body.ShowParentStudent;
                        isExtracurricularExist.ShowAttendanceRC = body.IsShowAttendanceReportCard;
                        isExtracurricularExist.ShowScoreRC = body.IsShowScoreReportCard;
                        isExtracurricularExist.IdExtracurricularGroup = isExtracurricularGroupExist.Id;
                        isExtracurricularExist.IsRegularSchedule = body.IsRegularSchedule;
                        isExtracurricularExist.ElectivesStartDate = body.ElectivesStartDate;
                        isExtracurricularExist.ElectivesEndDate = body.ElectivesEndDate;
                        isExtracurricularExist.AttendanceStartDate = body.AttendanceStartDate;
                        isExtracurricularExist.AttendanceEndDate = body.AttendanceEndDate;
                        isExtracurricularExist.MinParticipant = body.ParticipantMin;
                        isExtracurricularExist.MaxParticipant = body.ParticipantMax;
                        isExtracurricularExist.ScoreStartDate = body.ScoringStartDate == null ? null : body.ScoringStartDate;
                        isExtracurricularExist.ScoreEndDate = body.ScoringEndDate == null ? null : body.ScoringEndDate;
                        //isExtracurricularExist.Status = true;
                        isExtracurricularExist.Price = body.Price == null ? isExtracurricularExist.Price : body.Price.Value;
                        isExtracurricularExist.NeedObjective = body.NeedObjective;
                        isExtracurricularExist.IdExtracurricularType = body.IdExtracurricularType;

                        _dbContext.Entity<MsExtracurricular>().Update(isExtracurricularExist);

                        #region Update Mapping Extracurricular to Grade
                        // only update grade mapping when not in registration period
                        if (!inRegistrationPeriod)
                        {
                            var extracurricularGradeMapping = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .ToListAsync(CancellationToken);

                            var getGradeToDelete = extracurricularGradeMapping
                                .Where(ex => body.GradeList.All(ex2 => ex2 != ex.IdGrade))
                                .Select(x => x.IdGrade).ToList();

                            var getGradeToAdd = body.GradeList
                                .Where(ex => extracurricularGradeMapping.All(ex2 => ex2.IdGrade != ex))
                                .Select(x => x).ToList();

                            // delete Mapping Extracurricular to Grade
                            foreach (var grade in getGradeToDelete)
                            {
                                var deleteGrade = extracurricularGradeMapping.Where(x => x.IdGrade == grade).SingleOrDefault();

                                if (deleteGrade != null)
                                    deleteGrade.IsActive = false;
                                _dbContext.Entity<TrExtracurricularGradeMapping>().Update(deleteGrade);
                            }

                            // Add Mapping Extracurricular to Grade
                            foreach (var grade in getGradeToAdd)
                            {
                                var paramGrade = new TrExtracurricularGradeMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdGrade = grade,
                                    IdExtracurricular = isExtracurricularExist.Id
                                };
                                _dbContext.Entity<TrExtracurricularGradeMapping>().Add(paramGrade);
                            }
                        }
                        #endregion

                        #region Update Extracurricular to SpvCoach 
                        var getSpvCoachToDelete = extracurricularSpvCoach
                           .Where(ex => body.CoachSupervisorList.All(ex2 => ex2.IdUser != ex.IdBinusian))
                           .Select(x => x.IdBinusian).ToList();

                        var getSpvCoachToAdd = body.CoachSupervisorList
                            .Where(ex => extracurricularSpvCoach.All(ex2 => ex2.IdBinusian != ex.IdUser))
                            .Select(x => new { IdBinusian = x.IdUser, IsCoach = (x.IdExtracurricularCoachStatus != "1"), IdExtracurricularCoachStatus = x.IdExtracurricularCoachStatus }).ToList();

                        #region unused code
                        /*var getSpvCoachAlmostEqual = extracurricularSpvCoach
                           .Where(ex => !body.CoachSupervisorList.All(ex2 => ex2.IdUser == ex.IdBinusian && ex2.IdExtracurricularCoachStatus != ex.IdExtracurricularCoachStatus))
                           .ToList();*/
                        #endregion

                        var getSpvCoachAlmostEqual = extracurricularSpvCoach
                           .Where(ex => !getSpvCoachToDelete.Contains(ex.IdBinusian) && !body.CoachSupervisorList.All(ex2 => ex2.IdUser == ex.IdBinusian && ex2.IdExtracurricularCoachStatus != ex.IdExtracurricularCoachStatus))
                           .ToList();

                        // Delete spvCoach
                        foreach (var SpvCoach in getSpvCoachToDelete)
                        {
                            var deleteSpvCoach = extracurricularSpvCoach.Where(x => x.IdBinusian == SpvCoach).SingleOrDefault();

                            if (deleteSpvCoach != null)
                                deleteSpvCoach.IsActive = false;
                            _dbContext.Entity<MsExtracurricularSpvCoach>().Update(deleteSpvCoach);
                        }

                        // Add spvCoach
                        foreach (var SpvCoach in getSpvCoachToAdd)
                        {
                            var paramSpvCoach = new MsExtracurricularSpvCoach
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdBinusian = SpvCoach.IdBinusian,
                                IdExtracurricular = isExtracurricularExist.Id,
                                IsSpv = (SpvCoach.IdExtracurricularCoachStatus == "1"),
                                IdExtracurricularCoachStatus = SpvCoach.IdExtracurricularCoachStatus
                            };
                            _dbContext.Entity<MsExtracurricularSpvCoach>().Add(paramSpvCoach);

                            try
                            {
                                var createRoleSES = await _userRoleApi.AddUserRoleByRoleCode(new AddUserRoleByRoleCodeRequest
                                {
                                    IdUser = SpvCoach.IdBinusian,
                                    RoleCode = "SES",
                                    IdSchool = IdSchool
                                });
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        // Update spvCoach
                        foreach (var SpvCoach in getSpvCoachAlmostEqual)
                        {
                            var newDataSpvCoach = body.CoachSupervisorList
                                .Where(x => x.IdUser == SpvCoach.IdBinusian)
                                .FirstOrDefault();

                            SpvCoach.IsSpv = (newDataSpvCoach.IdExtracurricularCoachStatus == "1");
                            SpvCoach.IdExtracurricularCoachStatus = newDataSpvCoach.IdExtracurricularCoachStatus;
                            _dbContext.Entity<MsExtracurricularSpvCoach>().Update(SpvCoach);
                        }
                        #endregion

                        #region Update Extracurricular to ExtCoach 

                        //Check ExtracurricularExternalCoach
                        var extracurricularExternalCoache = await _dbContext.Entity<MsExtracurricularExternalCoach>()
                            .Where(x => x.IdSchool == isExtracurricularExist.ExtracurricularGroup.IdSchool)
                            .Select(x => x.Id)
                            .ToListAsync(CancellationToken);

                        var paramExternalCoachList = body.ExternalCoachList.Select(x => x.IdExtracurricularExternalCoach).ToList();

                        if (paramExternalCoachList.Count() > 0)
                        {
                            var extracurricularExtCoach = await _dbContext.Entity<MsExtracurricularExtCoachMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .ToListAsync(CancellationToken);

                            var getExtCoachToDelete = extracurricularExtCoach
                               .Where(ex => body.ExternalCoachList.All(ex2 => ex2.IdExtracurricularExternalCoach != ex.IdExtracurricularExternalCoach))
                               .Select(x => x.IdExtracurricularExternalCoach).ToList();

                            var getExtCoachToAdd = body.ExternalCoachList
                                .Where(ex => extracurricularExtCoach.All(ex2 => ex2.IdExtracurricularExternalCoach != ex.IdExtracurricularExternalCoach))
                                .Select(x => new { IdExtracurricularExternalCoach = x.IdExtracurricularExternalCoach }).ToList();

                            // Delete ExtCoach
                            foreach (var ExtCoach in getExtCoachToDelete)
                            {
                                var deleteExtCoach = extracurricularExtCoach.Where(x => x.IdExtracurricularExternalCoach == ExtCoach).SingleOrDefault();

                                if (deleteExtCoach != null)
                                    deleteExtCoach.IsActive = false;
                                _dbContext.Entity<MsExtracurricularExtCoachMapping>().Update(deleteExtCoach);
                            }

                            // Add spvCoach
                            foreach (var ExtCoach in getExtCoachToAdd)
                            {
                                var paramExtCoach = new MsExtracurricularExtCoachMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricularExternalCoach = ExtCoach.IdExtracurricularExternalCoach,
                                    IdExtracurricular = isExtracurricularExist.Id
                                };
                                _dbContext.Entity<MsExtracurricularExtCoachMapping>().Add(paramExtCoach);

                                try
                                {
                                    var createRoleEC = await _userRoleApi.AddUserRoleByRoleCode(new AddUserRoleByRoleCodeRequest
                                    {
                                        IdUser = ExtCoach.IdExtracurricularExternalCoach,
                                        RoleCode = "EC",
                                        IdSchool = IdSchool
                                    });
                                }
                                catch (Exception e)
                                {

                                }
                            }

                            // Update spvCoach
                            //foreach (var SpvCoach in getSpvCoachAlmostEqual)
                            //{
                            //    var updateSpvCoach = extracurricularSpvCoach.Where(x => x.Id == SpvCoach.IdBinusian).SingleOrDefault();
                            //    updateSpvCoach.IsSpv = !SpvCoach.IsSpv;
                            //    updateSpvCoach.IdExtracurricularCoachStatus = SpvCoach.IdExtracurricularCoachStatus;
                            //    _dbContext.Entity<MsExtracurricularSpvCoach>().Update(updateSpvCoach);
                            //}
                        }

                        #endregion

                        #region Update Score Component Mapping
                        var updateExtracurricularScoreCompMapping = await _dbContext.Entity<MsExtracurricularScoreCompMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .FirstOrDefaultAsync(CancellationToken);

                        if (!string.IsNullOrEmpty(body.IdExtracurricularScoreCompCategory))
                        {
                            if (updateExtracurricularScoreCompMapping != null)
                            {
                                updateExtracurricularScoreCompMapping.IdExtracurricularScoreCompCategory = body.IdExtracurricularScoreCompCategory;

                                _dbContext.Entity<MsExtracurricularScoreCompMapping>().Update(updateExtracurricularScoreCompMapping);
                            }
                            else
                            {
                                var addExtracurricularScoreCompMapping = new MsExtracurricularScoreCompMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricular = body.IdExtracurricular,
                                    IdExtracurricularScoreCompCategory = body.IdExtracurricularScoreCompCategory
                                };
                                _dbContext.Entity<MsExtracurricularScoreCompMapping>().Add(addExtracurricularScoreCompMapping);
                            }
                        }
                        else
                        {
                            if (updateExtracurricularScoreCompMapping != null)
                            {
                                _dbContext.Entity<MsExtracurricularScoreCompMapping>().Remove(updateExtracurricularScoreCompMapping);
                            }
                        }

                        #endregion

                        #region Update Score Legend Mapping
                        var updateExtracurricularScoreLegendMapping = await _dbContext.Entity<MsExtracurricularScoreLegendMapping>()
                            .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                            .FirstOrDefaultAsync(CancellationToken);

                        if (!string.IsNullOrEmpty(body.IdExtracurricularScoreLegendCategory))
                        {
                            if (updateExtracurricularScoreLegendMapping != null)
                            {
                                updateExtracurricularScoreLegendMapping.IdExtracurricularScoreLegendCategory = body.IdExtracurricularScoreLegendCategory;

                                _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Update(updateExtracurricularScoreLegendMapping);
                            }
                            else
                            {
                                var addExtracurricularScoreLegendMapping = new MsExtracurricularScoreLegendMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricular = body.IdExtracurricular,
                                    IdExtracurricularScoreLegendCategory = body.IdExtracurricularScoreLegendCategory
                                };
                                _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Add(addExtracurricularScoreLegendMapping);
                            }
                        }
                        else
                        {
                            if (updateExtracurricularScoreLegendMapping != null)
                            {
                                _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Remove(updateExtracurricularScoreLegendMapping);
                            }
                        }

                        #endregion

                        #region Set Delete, Add and Update Session

                        if (body.IsRegularSchedule == true && body.ScheduleList != null)
                        {

                            //Check MsExtracurricularSession From TrExtracurricularSessionMapping
                            var extracurricularSessionMapping = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                                .Include(x => x.ExtracurricularSession.Day)
                                .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                                .ToListAsync(CancellationToken);

                            var getExtracurricularSessionMappingToDelete = extracurricularSessionMapping //DeleteDay
                                .Where(ex => !body.ScheduleList.Any(ex2 => ex2.Day.Id == ex.ExtracurricularSession.IdDay))
                                .ToList();

                            var getExtracurricularSessionMappingToAdd = body.ScheduleList //AddDay
                                .Where(ex => !extracurricularSessionMapping.Any(ex2 => ex2.ExtracurricularSession.IdDay == ex.Day.Id))
                                .ToList();

                            var checkSessionDays = new List<ItemValueVm>();

                            if (getExtracurricularSessionMappingToAdd.Count() > 0)
                            {
                                foreach (var item in getExtracurricularSessionMappingToAdd)
                                {
                                    var paramSession = new MsExtracurricularSession
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdDay = item.Day.Id,
                                        IdVenue = item.Venue.Id,
                                        StartTime = TimeSpan.Parse(item.StartTime),
                                        EndTime = TimeSpan.Parse(item.EndTime)
                                    };
                                    _dbContext.Entity<MsExtracurricularSession>().Add(paramSession);

                                    var paramSessionMapping = new TrExtracurricularSessionMapping
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdExtracurricular = body.IdExtracurricular,
                                        IdExtracurricularSession = paramSession.Id
                                    };
                                    _dbContext.Entity<TrExtracurricularSessionMapping>().Add(paramSessionMapping);

                                    var sessionData = new ItemValueVm
                                    {
                                        Id = paramSession.Id,
                                        Description = item.Day.Description
                                    };
                                    checkSessionDays.Add(sessionData);
                                }
                            }

                            var getExtracurricularSessionMappingToUpdate = extracurricularSessionMapping //Update Time / Venue
                               .Where(ex => body.ScheduleList.Any(ex2 => ex2.Day.Id == ex.ExtracurricularSession.IdDay) )
                               .ToList();

                            if (getExtracurricularSessionMappingToUpdate.Count() > 0)
                            {
                                var extracurricularSessionToUpdate = await _dbContext.Entity<MsExtracurricularSession>()
                                    .Where(x => getExtracurricularSessionMappingToUpdate.Select(x => x.IdExtracurricularSession).ToList().Contains(x.Id))
                                    .ToListAsync(CancellationToken);
                                foreach (var item in body.ScheduleList)
                                {
                                    var dataExtracurricularSessionToUpdate = extracurricularSessionToUpdate.Where(x => x.IdDay == item.Day.Id).ToList();
                                    foreach (var itemUpdate in dataExtracurricularSessionToUpdate)
                                    {
                                        if(itemUpdate.IdVenue != item.Venue.Id || itemUpdate.StartTime != TimeSpan.Parse(item.StartTime) || itemUpdate.EndTime != TimeSpan.Parse(item.EndTime))
                                        {
                                            itemUpdate.StartTime = TimeSpan.Parse(item.StartTime);
                                            itemUpdate.EndTime = TimeSpan.Parse(item.EndTime);
                                            itemUpdate.IdVenue = item.Venue.Id;
                                            _dbContext.Entity<MsExtracurricularSession>().Update(itemUpdate);
                                        }
                                    }
                                }
                            }


                            //Check TrExtracurricularGeneratedAtt Change Elective Date
                            var extracurricularGeneratedAtt = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                .Include(x => x.ExtracurricularSession)
                                    .ThenInclude(x => x.Day)
                                .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                                .ToListAsync(CancellationToken);

                            var getExtracurricularGeneratedAttToDelete = extracurricularGeneratedAtt //Delete when date is out of the elective date range / delete according to SessionToDelete
                                .Where(x => (DateTime)x.Date < body.ElectivesStartDate || (DateTime)x.Date > body.ElectivesEndDate || getExtracurricularSessionMappingToDelete.Select(x => x.IdExtracurricularSession).Contains(x.IdExtracurricularSession))
                                .ToList();

                            //Add when date is in the elective date range / add according to SessionToAdd
                            if (getExtracurricularSessionMappingToAdd.Count() > 0|| extracurricularGeneratedAtt.Count() == 0 || extracurricularGeneratedAtt.Select(x => x.Date).Min() >= body.ElectivesStartDate || extracurricularGeneratedAtt.Select(x => x.Date).Max() <= body.ElectivesEndDate)
                            {
                                var listDayExtracurricularSessionMapping = extracurricularSessionMapping
                                .Select(x => new ItemValueVm
                                {
                                    Description = x.ExtracurricularSession.Day.Description,
                                    Id = x.IdExtracurricularSession
                                }).Distinct().ToList();
                                checkSessionDays.AddRange(listDayExtracurricularSessionMapping);
                                checkSessionDays.Distinct();

                                for (int i = 0; i <= days; i++)
                                {
                                    var currDate = electivesStartDate.AddDays(i);
                                    if (checkSessionDays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Description)).ToList().Contains(currDate.DayOfWeek))
                                    {
                                        if (!extracurricularGeneratedAtt.Select(x => x.Date).ToList().Contains(currDate))
                                        {
                                            var getExtracurricularGeneratedAttToAdd = new TrExtracurricularGeneratedAtt
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdExtracurricular = body.IdExtracurricular,
                                                IdExtracurricularSession = checkSessionDays.Where(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Description) == currDate.DayOfWeek).Select(x => x.Id).FirstOrDefault(),
                                                Date = currDate,
                                                NewSession = false
                                            };
                                            _dbContext.Entity<TrExtracurricularGeneratedAtt>().Add(getExtracurricularGeneratedAttToAdd);
                                        }
                                    }
                                }
                            }

                            //Delete Session and TrExtracurricularGeneratedAtt
                            if (getExtracurricularGeneratedAttToDelete.Count() > 0)
                            {
                                foreach (var item in getExtracurricularGeneratedAttToDelete)
                                {
                                    item.IsActive = false;
                                }
                                _dbContext.Entity<TrExtracurricularGeneratedAtt>().UpdateRange(getExtracurricularGeneratedAttToDelete);
                            }

                            if (getExtracurricularSessionMappingToDelete.Count() > 0)
                            {
                                var extracurricularSessionToDelete = await _dbContext.Entity<MsExtracurricularSession>()
                                    .Where(x => getExtracurricularSessionMappingToDelete.Select(x => x.IdExtracurricularSession).ToList().Contains(x.Id))
                                    .ToListAsync(CancellationToken);
                                foreach (var item in getExtracurricularSessionMappingToDelete)
                                {
                                    item.IsActive = false;
                                }
                                foreach (var item in extracurricularSessionToDelete)
                                {
                                    item.IsActive = false;
                                }
                                _dbContext.Entity<MsExtracurricularSession>().UpdateRange(extracurricularSessionToDelete);
                                _dbContext.Entity<TrExtracurricularSessionMapping>().UpdateRange(getExtracurricularSessionMappingToDelete);
                            }

                        }
                        #endregion
                    }

                }
                else
                {
                    var idGradeExtracurricularList = isExtracurricularExist
                                                        .ExtracurricularGradeMappings.Select(x => x.IdGrade).ToList();

                    //var extracurricularRule = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                    //                                    .Include(ergm => ergm.ExtracurricularRule)
                    //                                    .Where(x => idGradeExtracurricularList.Any(y => y == x.IdGrade) &&
                    //                                                x.ExtracurricularRule.Status == true
                    //                                            )
                    //                                    .FirstOrDefaultAsync(CancellationToken);

                    //if (extracurricularRule.ExtracurricularRule.ReviewDate != null &&
                    //    _dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.ReviewDate &&
                    //    extracurricularRule.ExtracurricularRule.ReviewDate.Value.AddHours(10) > _dateTime.ServerTime)
                    //    throw new BadRequestException(string.Format("Extracurricular status can only be changed 10 hours after the review date. Please wait for {0} hours.", Convert.ToInt32((extracurricularRule.ExtracurricularRule.ReviewDate.Value.AddHours(10) - _dateTime.ServerTime).TotalHours)));

                    isExtracurricularExist.Status = body.Status;
                    _dbContext.Entity<MsExtracurricular>().Update(isExtracurricularExist);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
