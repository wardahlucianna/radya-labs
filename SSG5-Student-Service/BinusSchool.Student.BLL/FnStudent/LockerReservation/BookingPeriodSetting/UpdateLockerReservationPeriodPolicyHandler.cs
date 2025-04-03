using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class UpdateLockerReservationPeriodPolicyHandler : FunctionsHttpSingleHandler
    {
        private IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public UpdateLockerReservationPeriodPolicyHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<IEnumerable<UpdateLockerReservationPeriodPolicyRequest>, UpdateLockerReservationPeriodPolicyValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getLockerReservationPeriod = _dbContext.Entity<MsLockerReservationPeriod>()
                    .ToList();

                foreach (var lockerReservationPeriod in param)
                {
                    var getIdLockerReservationPeriod = getLockerReservationPeriod
                        .Where(a => a.Id == lockerReservationPeriod.IdLockerReservationPeriod)
                        .FirstOrDefault();

                    getIdLockerReservationPeriod.StartDate = lockerReservationPeriod.StartDate;
                    getIdLockerReservationPeriod.EndDate = lockerReservationPeriod.EndDate;
                    getIdLockerReservationPeriod.PolicyMessage = lockerReservationPeriod.PolicyMessage;

                    _dbContext.Entity<MsLockerReservationPeriod>().Update(getIdLockerReservationPeriod);
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
