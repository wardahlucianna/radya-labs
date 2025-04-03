using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class UpdateExtracurricularAttendanceHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public UpdateExtracurricularAttendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateExtracurricularAttendanceRequest, UpdateExtracurricularAttendanceValidator>();

            #region Create Temp List
            //var exculSessionList = await _dbContext.Entity<MsExtracurricularSession>()
            //    .ToListAsync(CancellationToken);

            //var exculGenerateAttList = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
            //    .Include(x => x.Extracurricular)
            //    .ToListAsync(CancellationToken);

            var exculGenerateAttEntryList = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                .Where(x => x.IdExtracurricularGeneratedAtt == body.IdGeneratedAttendance)
                .ToListAsync(CancellationToken);
            #endregion

            #region Validate Extracurricular Generate Att
            var exculGenerateAtt = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                .Include(x => x.Extracurricular)
                .Where(x => x.Id == body.IdGeneratedAttendance)
                .OrderByDescending(x => x.DateIn)
                .FirstOrDefaultAsync(CancellationToken);

            if (exculGenerateAtt == null)
            {
                throw new BadRequestException($"Extracurricular Generate Att with Id : {exculGenerateAtt.Id} not exists");
            }

            var excul = exculGenerateAtt.Extracurricular;
            if (!(excul.ElectivesStartDate.Date <= body.Date.Date && body.Date.Date <= excul.ElectivesEndDate.Date))
            {
                throw new BadRequestException($"Date : {body.Date} is Outside Electives Date");
            }

            var CheckDateExist = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                        .Where(x => x.IdExtracurricular == exculGenerateAtt.IdExtracurricular)
                        .Where(x => x.Date.Date == body.Date.Date)
                        .Where(x => x.Id != body.IdGeneratedAttendance)
                        .OrderByDescending(x => x.DateIn)
                        .FirstOrDefaultAsync(CancellationToken);
            if (CheckDateExist != null)
            {
                throw new BadRequestException($"Attendance with date {body.Date.Date} alredy exists");
            }

            var getDay = await _dbContext.Entity<LtDay>()
                .Where(x => x.Description.ToLower().Trim() == body.Date.DayOfWeek.ToString().ToLower().Trim())
                .SingleOrDefaultAsync(CancellationToken);

            #endregion

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                if (excul.IsRegularSchedule)
                {
                    var exculSession = await _dbContext.Entity<MsExtracurricularSession>()
                        .Where(x => x.Id == exculGenerateAtt.IdExtracurricularSession)
                        .OrderByDescending(x => x.DateIn)
                        .FirstOrDefaultAsync(CancellationToken);
                    if (exculSession != null)
                    {
                        if (exculGenerateAtt.Date.Date != body.Date.Date)
                        {
                            //Create News Session
                            var paramSession = new MsExtracurricularSession
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdDay = getDay.Id,
                                IdVenue = exculSession.IdVenue,
                                StartTime = exculSession.StartTime,
                                EndTime = exculSession.EndTime
                            };
                            _dbContext.Entity<MsExtracurricularSession>().Add(paramSession);

                            //Update Generate Att
                            exculGenerateAtt.Date = body.Date.Date;
                            exculGenerateAtt.NewSession = true;
                            exculGenerateAtt.IdExtracurricularSession = paramSession.Id;
                            _dbContext.Entity<TrExtracurricularGeneratedAtt>().Update(exculGenerateAtt);
                        }
                    }
                    else
                    {
                        throw new BadRequestException($"Extracurricular Session for GenerateAttId : {exculGenerateAtt.Id} not exists");
                    }

                }
                else
                {
                    #region Update Extracurricular Session (Unused)
                    /*var exculSession = await _dbContext.Entity<MsExtracurricularSession>()
                        .Where(x => x.Id == exculGenerateAtt.IdExtracurricularSession)
                        .OrderByDescending(x => x.DateIn)
                        .FirstOrDefaultAsync(CancellationToken);
                    if (exculSession != null)
                    {
                        exculSession.IdDay = getDay.Id;
                        //exculSession.IdVenue = body.Venue.Id;
                        //exculSession.StartTime = TimeSpan.Parse(body.StartTime);
                        //exculSession.EndTime = TimeSpan.Parse(body.EndTime);
                        _dbContext.Entity<MsExtracurricularSession>().Update(exculSession);

                        
                    }
                    else
                    {
                        throw new BadRequestException($"Extracurricular Session for GenerateAttId : {exculGenerateAtt.Id} not exists");
                    }*/
                    #endregion

                    //Update Generate Att
                    if (exculGenerateAtt.Date.Date != body.Date.Date)
                    {
                        exculGenerateAtt.Date = body.Date.Date;
                        _dbContext.Entity<TrExtracurricularGeneratedAtt>().Update(exculGenerateAtt);
                    }
                }

                #region Fill Attendence for Excul
                if (body.StatusAttendance != null)
                {
                    foreach (var student in body.StatusAttendance)
                    {
                        var isExculAttEntryExist = exculGenerateAttEntryList
                            .Where(x => x.IdStudent == student.IdStudent)
                            .SingleOrDefault();

                        if (isExculAttEntryExist != null)
                        {
                            if (isExculAttEntryExist.IdExtracurricularStatusAtt != student.IdStatusAttendance || isExculAttEntryExist.Reason != student.Reason)
                            {
                                isExculAttEntryExist.IdExtracurricularStatusAtt = student.IdStatusAttendance;
                                isExculAttEntryExist.Reason = student.Reason;

                                _dbContext.Entity<TrExtracurricularAttendanceEntry>().Update(isExculAttEntryExist);
                            }
                        }
                        else
                        {
                            var studentAtt = new TrExtracurricularAttendanceEntry
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricularGeneratedAtt = body.IdGeneratedAttendance,
                                IdExtracurricularStatusAtt = student.IdStatusAttendance,
                                IdStudent = student.IdStudent,
                                Reason = student.Reason,
                            };

                            _dbContext.Entity<TrExtracurricularAttendanceEntry>().Add(studentAtt);
                        }
                    }
                }
                #endregion

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
