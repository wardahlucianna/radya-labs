using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class EditJoinDateStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;

        public EditJoinDateStudentParticipantHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<EditJoinDateStudentParticipantRequest, EditJoinDateValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var selectedData = await _dbContext.Entity<MsExtracurricularParticipant>()
                    .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                    .ToListAsync(CancellationToken);

                foreach (var data in selectedData)
                {
                    var paramData = param.EditRequestsList.FirstOrDefault(e => e.IdStudent == data.IdStudent);

                    if (paramData != null)
                    {
                        data.JoinDate = paramData.JoinDate;
                    }
                }

                _dbContext.Entity<MsExtracurricularParticipant>().UpdateRange(selectedData);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
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

            return Request.CreateApiResult2();
        }
    }
}
