using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation
{
    public class UpdateLockedLockerHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public UpdateLockedLockerHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateLockedLockerRequest, UpdateLockedLockerValidator>();

            var getLocker = _dbContext.Entity<MsLocker>()
                               .Where(x => x.Id == param.IdLocker)
                               .FirstOrDefault();

            if (getLocker == null)
                throw new BadRequestException($"Locker Not Found");

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = getLocker.IdAcademicYear,
                Semester = getLocker.Semester
            });

            var getLockerData = getStudentMasterDataForHeaderReport
                .Where(x => x.IdLocker == param.IdLocker)
                .Where(x => x.IdBuilding == getLocker.IdBuilding)
                .Where(x => x.IdFloor == getLocker.IdFloor)
                .FirstOrDefault();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (param.LockedLocker)
                {
                    if (getLocker.IsLocked == true)
                    {
                        throw new BadRequestException($"Locker Already Locked");
                    }
                    else if (getLocker.IsLocked == false && getLockerData.IdStudentLockerReservation != null)
                    {
                        throw new BadRequestException($"Locker Already Reserve");
                    }
                    else if (getLocker.IsLocked == false && getLockerData.IdStudentLockerReservation == null)
                    {
                        getLocker.IsLocked = true;
                    }

                }
                else
                {
                    if (getLocker.IsLocked == true)
                    {
                        getLocker.IsLocked = false;
                    }
                    else
                    {
                        throw new BadRequestException($"Locker Already UnLocked");
                    }
                }

                _dbContext.Entity<MsLocker>().Update(getLocker);

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
