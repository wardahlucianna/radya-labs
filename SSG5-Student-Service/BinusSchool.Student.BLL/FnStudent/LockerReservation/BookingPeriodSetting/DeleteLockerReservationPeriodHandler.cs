using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class DeleteLockerReservationPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteLockerReservationPeriodHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteLockerReservationPeriodRequest, DeleteLockerReservationPeriodValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getLockerReservationPeriod = _dbContext.Entity<MsLockerReservationPeriod>()
                    .ToList();

                foreach (var item in param.IdLockerReservationPeriod)
                {
                    var getIdLockerReservationPeriod = getLockerReservationPeriod
                        .Where(a => a.Id == item)
                        .FirstOrDefault();

                    getIdLockerReservationPeriod.IsActive = false;

                    _dbContext.Entity<MsLockerReservationPeriod>().Update(getIdLockerReservationPeriod);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2(code: HttpStatusCode.NoContent);
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
