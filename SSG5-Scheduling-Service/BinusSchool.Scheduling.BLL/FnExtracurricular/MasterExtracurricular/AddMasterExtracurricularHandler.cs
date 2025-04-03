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
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class AddMasterExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;
        private readonly IUserRole _userRoleApi;

        public AddMasterExtracurricularHandler(ISchedulingDbContext dbContext,
            IUserRole userRoleApi)
        {
            _dbContext = dbContext;
            _userRoleApi = userRoleApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<AddMasterExtracurricularRequest, AddMasterExtracurricularValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                DateTime electivesStartDate = body.ElectivesStartDate;
                DateTime electivesEndDate = body.ElectivesEndDate;

                int days = (electivesEndDate - electivesStartDate).Days;

                var isExtracurricularGroupExist = await _dbContext.Entity<MsExtracurricularGroup>()
                    .Where(x => x.Id == body.IdExtracurricularGroup)
                    .SingleOrDefaultAsync(CancellationToken);
                if (isExtracurricularGroupExist == null)
                    throw new BadRequestException($"ExtracurricularGroup {body.IdExtracurricularGroup} not exists");

                var IdSchool = await _dbContext.Entity<MsAcademicYear>()
                 .Where(x => x.Id == body.IdAcademicYear)
                 .Select(a => a.IdSchool)
                 .SingleOrDefaultAsync(CancellationToken);

                #region Insert New Master Extracurricular
                var param = new MsExtracurricular
                {
                    Id = Guid.NewGuid().ToString(),
                    Semester = body.Semester,
                    Name = body.ExtracurricularName,
                    Description = body.ExtracurricularDescription,
                    ShowParentStudent = body.ShowParentStudent,
                    ShowAttendanceRC = body.IsShowAttendanceReportCard,
                    ShowScoreRC = body.IsShowScoreReportCard,
                    IdExtracurricularGroup = isExtracurricularGroupExist.Id,
                    IsRegularSchedule = body.IsRegularSchedule,
                    ElectivesStartDate = body.ElectivesStartDate,
                    ElectivesEndDate = body.ElectivesEndDate,
                    AttendanceStartDate = body.AttendanceStartDate,
                    AttendanceEndDate = body.AttendanceEndDate,
                    MinParticipant = body.ParticipantMin,
                    MaxParticipant = body.ParticipantMax,
                    ScoreStartDate = body.ScoringStartDate == null ? null : body.ScoringStartDate,
                    ScoreEndDate = body.ScoringEndDate == null ? null : body.ScoringEndDate,
                    Status = true,
                    Price = body.Price,
                    NeedObjective = body.NeedObjective,
                    IdExtracurricularType = body.IdExtracurricularType
                };
                _dbContext.Entity<MsExtracurricular>().Add(param);
                #endregion

                #region Mapping Extracurricular to Grade
                foreach (var IdGrade in body.GradeList)
                {
                    var paramGrade = new TrExtracurricularGradeMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGrade = IdGrade,
                        IdExtracurricular = param.Id
                    };
                    _dbContext.Entity<TrExtracurricularGradeMapping>().Add(paramGrade);
                }
                #endregion

                #region Mapping Extracurricular to SpvCoach 
                foreach (var SpvCoach in body.CoachSupervisorList)
                {
                    var paramSpvCoach = new MsExtracurricularSpvCoach
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBinusian = SpvCoach.IdUser,
                        IdExtracurricular = param.Id,
                        IsSpv = (SpvCoach.IdExtracurricularCoachStatus == "1" ? true : false),
                        IdExtracurricularCoachStatus = SpvCoach.IdExtracurricularCoachStatus
                    };
                    _dbContext.Entity<MsExtracurricularSpvCoach>().Add(paramSpvCoach);

                    try
                    {
                        var createRoleSES = await _userRoleApi.AddUserRoleByRoleCode(new AddUserRoleByRoleCodeRequest
                        {
                            IdUser = SpvCoach.IdUser,
                            RoleCode = "SES",
                            IdSchool = IdSchool
                        });
                    }
                    catch (Exception e)
                    {

                    }

                }
                #endregion

                #region Mapping Extracurricular to external Coach 
                foreach (var ExtCoach in body.ExternalCoachList)
                {
                    var paramExtCoach = new MsExtracurricularExtCoachMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricularExternalCoach = ExtCoach.IdExtracurricularExternalCoach,
                        IdExtracurricular = param.Id
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
                #endregion

                #region Add Score Component Mapping
                if (!string.IsNullOrEmpty(body.IdExtracurricularScoreCompCategory))
                {
                    var addExtracurricularScoreCompMapping = new MsExtracurricularScoreCompMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricular = param.Id,
                        IdExtracurricularScoreCompCategory = body.IdExtracurricularScoreCompCategory
                    };
                    _dbContext.Entity<MsExtracurricularScoreCompMapping>().Add(addExtracurricularScoreCompMapping);
                }
                #endregion

                #region Add Score Legend Mapping
                if (!string.IsNullOrEmpty(body.IdExtracurricularScoreLegendCategory))
                {
                    var addExtracurricularScoreLegendMapping = new MsExtracurricularScoreLegendMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricular = param.Id,
                        IdExtracurricularScoreLegendCategory = body.IdExtracurricularScoreLegendCategory
                    };
                    _dbContext.Entity<MsExtracurricularScoreLegendMapping>().Add(addExtracurricularScoreLegendMapping);
                }
                #endregion

                if (body.IsRegularSchedule == true && body.ScheduleList != null)
                {
                    #region Create New session
                    foreach (var item in body.ScheduleList)
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

                        //Extracurricular attendence
                        string myDay = item.Day.Description;
                        var myDayDayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), myDay); //convert string to DayOfWeek type

                        //var allDates = new List<DateTime>();
                        for (int i = 0; i <= days; i++)
                        {
                            var currDate = electivesStartDate.AddDays(i);
                            if (currDate.DayOfWeek == myDayDayOfWeek)
                            {
                                var paramGeneratedAtt = new TrExtracurricularGeneratedAtt
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdExtracurricular = param.Id,
                                    IdExtracurricularSession = paramSession.Id,
                                    Date = currDate,
                                    NewSession = false
                                };
                                _dbContext.Entity<TrExtracurricularGeneratedAtt>().Add(paramGeneratedAtt);
                                //allDates.Add(currDate);
                            }
                        }

                        var paramSessionMapping = new TrExtracurricularSessionMapping
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdExtracurricular = param.Id,
                            IdExtracurricularSession = paramSession.Id
                        };
                        _dbContext.Entity<TrExtracurricularSessionMapping>().Add(paramSessionMapping);
                    }
                    #endregion
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
