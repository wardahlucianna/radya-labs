using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;


namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class AddSessionExtracurricularAttendanceHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public AddSessionExtracurricularAttendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddSessionExtracurricularAttendanceRequest, AddSessionExtracurricularAttendanceValidator>();

            #region Validate Extracurricular Generate Att
            var isExculExist = await _dbContext.Entity<MsExtracurricular>()
                .Where(x => x.Id == body.IdExtracurricular)
                .SingleOrDefaultAsync(CancellationToken);
                
            if (isExculExist == null)
            {
                throw new BadRequestException($"Extracurricular with Id : {isExculExist} not exists");
            }

            var isExculGeneratedAttExist = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                .Where(x => x.Id == body.IdExtracurricular)
                .Where(x => x.Date.Date == body.Date.Date)
                .ToListAsync(CancellationToken);

            if (isExculGeneratedAttExist.Count() >= 1)
            {
                throw new BadRequestException($"Date Already Taken");
            }

            var isVanueTaken = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                .Where(x => x.Date.Date == body.Date.Date)
                .Where(x => x.ExtracurricularSession.IdVenue == body.Venue.Id)
                .ToListAsync(CancellationToken);

            if (isExculGeneratedAttExist.Count() >= 1)
            {
                throw new BadRequestException($"Venue Already Taken");
            }
            #endregion

            var dayCode = body.Date.DayOfWeek.ToString();
            var idDay = await _dbContext.Entity<LtDay>()
                .Where(x => x.Description.ToLower() == dayCode.ToLower())
                .Select(x => x.Id)
                .SingleOrDefaultAsync(CancellationToken);

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var paramSession = new MsExtracurricularSession();

                if (isExculExist.IsRegularSchedule == true)
                {
                    #region Create Extracurricular Session
                    paramSession = new MsExtracurricularSession()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdDay = idDay,
                        IdVenue = body.Venue.Id,
                        StartTime = TimeSpan.Parse(body.StartTime),
                        EndTime = TimeSpan.Parse(body.EndTime)
                    };
                    _dbContext.Entity<MsExtracurricularSession>().Add(paramSession);
                    #endregion

                    #region Create Extracurricular Session Mapping
                    var paramSessionMapping = new TrExtracurricularSessionMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricular = body.IdExtracurricular,
                        IdExtracurricularSession = paramSession.Id
                    };
                    _dbContext.Entity<TrExtracurricularSessionMapping>().Add(paramSessionMapping);
                    #endregion
                }

                #region Create Extracurricular Generated Att
                var paramGeneratedAtt = new TrExtracurricularGeneratedAtt
                {
                    Id = Guid.NewGuid().ToString(),
                    IdExtracurricular = body.IdExtracurricular,
                    IdExtracurricularSession = paramSession.Id,
                    Date = body.Date,
                    NewSession = true
                };
                _dbContext.Entity<TrExtracurricularGeneratedAtt>().Add(paramGeneratedAtt);
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
