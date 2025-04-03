using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class UpdateElectivesEntryPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public UpdateElectivesEntryPeriodHandler(ISchedulingDbContext dbContext,
        IUserRole userRoleApi,
        IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateElectivesEntryPeriodRequest, UpdateElectivesEntryPeriodValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                
                var ListUpdateElectivesDate = new List<MsExtracurricular>();

                var getElectives = await _dbContext.Entity<MsExtracurricular>()
                        .Where(x => body.Electives.Any(z => z == x.Id))
                        .ToListAsync(CancellationToken);

                //Get Attendance and Session Data and Days
                var getAttendanceEntry = new List<TrExtracurricularAttendanceEntry>();
                var getExtracurricularGeneratedAtt = new List<TrExtracurricularGeneratedAtt>();
                var getExtracurricularSession = new List<TrExtracurricularSessionMapping>();
                if (body.ElectivesStartDate != null && body.ElectivesEndDate != null && body.ElectivesStartDate <= body.ElectivesEndDate)
                {
                    getAttendanceEntry = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                        .Where(x => body.Electives.Contains(x.ExtracurricularGeneratedAtt.IdExtracurricular))
                        .ToListAsync(CancellationToken);

                    getExtracurricularGeneratedAtt = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                        .Where(x => body.Electives.Contains(x.IdExtracurricular))
                        .ToListAsync(CancellationToken);

                    getExtracurricularSession = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                        .Include(x => x.ExtracurricularSession)
                            .ThenInclude(x => x.Day)
                        .Where(x => body.Electives.Contains(x.IdExtracurricular))
                        .ToListAsync(CancellationToken);
                }

                foreach (var item in body.Electives)
                {
                    var isElectiveExist = getElectives.Where(x => x.Id == item).SingleOrDefault();
                    if(isElectiveExist != null)
                    {
                        if(body.AttendanceStartDate != null && body.AttendanceEndDate != null && body.AttendanceStartDate <= body.AttendanceEndDate)
                        {
                            isElectiveExist.AttendanceStartDate = (DateTime)body.AttendanceStartDate;
                            isElectiveExist.AttendanceEndDate = (DateTime)body.AttendanceEndDate;
                        }

                        if(body.ElectivesStartDate != null && body.ElectivesEndDate != null && body.ElectivesStartDate <= body.ElectivesEndDate)
                        {
                            var filterExtracurricularGeneratedAtt = getExtracurricularGeneratedAtt
                                .Where(x => x.IdExtracurricular == item)
                                .ToList();

                            var filterAttendanceEntry = getAttendanceEntry
                                .Where(x => filterExtracurricularGeneratedAtt.Select(y => y.Id).Contains(x.IdExtracurricularGeneratedAtt))
                                .ToList();

                            var filterExtracurricularSession = getExtracurricularSession
                                .Where(x => x.IdExtracurricular == item)
                                .ToList();

                            var checkSessionDays = new List<ItemValueVm>();

                            //Add Sessions
                            if (filterExtracurricularGeneratedAtt.Count() == 0 || filterExtracurricularGeneratedAtt.Select(x => x.Date).Min() >= (DateTime)body.ElectivesStartDate || filterExtracurricularGeneratedAtt.Select(x => x.Date).Max() <= (DateTime)body.ElectivesEndDate)
                            {
                                var listDayExtracurricularSessionMapping = filterExtracurricularSession
                                .Select(x => new ItemValueVm
                                {
                                    Description = x.ExtracurricularSession.Day.Description,
                                    Id = x.IdExtracurricularSession
                                }).Distinct().ToList();
                                checkSessionDays.AddRange(listDayExtracurricularSessionMapping);
                                checkSessionDays.Distinct();

                                DateTime electivesStartDate = (DateTime)body.ElectivesStartDate;
                                DateTime electivesEndDate = (DateTime)body.ElectivesEndDate;
                                int days = (electivesEndDate - electivesStartDate).Days;

                                for (int i = 0; i <= days; i++)
                                {
                                    var currDate = electivesStartDate.AddDays(i);
                                    if (checkSessionDays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Description)).ToList().Contains(currDate.DayOfWeek))
                                    {
                                        if (!filterExtracurricularGeneratedAtt.Select(x => x.Date).ToList().Contains(currDate))
                                        {
                                            var getExtracurricularGeneratedAttToAdd = new TrExtracurricularGeneratedAtt
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdExtracurricular = item,
                                                IdExtracurricularSession = checkSessionDays.Where(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Description) == currDate.DayOfWeek).Select(x => x.Id).FirstOrDefault(),
                                                Date = currDate,
                                                NewSession = false
                                            };
                                            _dbContext.Entity<TrExtracurricularGeneratedAtt>().Add(getExtracurricularGeneratedAttToAdd);
                                        }
                                    }
                                }
                            }

                            //Delete Session + check Attendance
                            var getExtracurricularGeneratedAttToDelete = filterExtracurricularGeneratedAtt //Delete when date is out of the elective date range + check attendance
                                .Where(x => ((DateTime)x.Date < body.ElectivesStartDate || (DateTime)x.Date > body.ElectivesEndDate) && (!filterAttendanceEntry.Select(x => x.ExtracurricularGeneratedAtt).Contains(x)))
                                .ToList();
                            if (getExtracurricularGeneratedAttToDelete.Count() > 0)
                            {
                                foreach (var itemDelete in getExtracurricularGeneratedAttToDelete)
                                {
                                    itemDelete.IsActive = false;
                                }
                                _dbContext.Entity<TrExtracurricularGeneratedAtt>().UpdateRange(getExtracurricularGeneratedAttToDelete);
                            }

                            isElectiveExist.ElectivesStartDate = (DateTime)body.ElectivesStartDate;
                            isElectiveExist.ElectivesEndDate = (DateTime)body.ElectivesEndDate;
                        }

                        if(body.ScoringStartDate != null && body.ScoringEndDate != null && body.ScoringStartDate <= body.ScoringEndDate)
                        {
                            isElectiveExist.ScoreStartDate = (DateTime)body.ScoringStartDate;
                            isElectiveExist.ScoreEndDate = (DateTime)body.ScoringEndDate;
                        }

                        ListUpdateElectivesDate.Add(isElectiveExist);
                    }
                }

                _dbContext.Entity<MsExtracurricular>().UpdateRange(ListUpdateElectivesDate);

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

