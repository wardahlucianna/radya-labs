using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
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
    public class SaveLockerReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public SaveLockerReservationHandler(
            IStudentDbContext dbContext,
            IMachineDateTime dateTime,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveLockerReservationRequest, SaveLockerReservationValidator>();

            var userLogin = AuthInfo.UserId;

            var getLocker = _dbContext.Entity<MsLocker>()
                .Where(x => x.Id == param.IdLocker)
                .FirstOrDefault();

            if (getLocker == null)
                throw new BadRequestException("Locker Not Found");

            //Check Reservation Period
            var getLockerReservationPeriod = _dbContext.Entity<MsLockerReservationPeriod>()
                .Where(x => 
                    x.IdAcademicYear == getLocker.IdAcademicYear &&
                    x.Semester == getLocker.Semester &&
                    x.IdGrade == param.IdGrade)
                .Where(x => x.StartDate <= _dateTime.ServerTime && x.EndDate >= _dateTime.ServerTime)
                .FirstOrDefault();

            if (getLockerReservationPeriod == null)
                throw new BadRequestException("Not In Reservation Period");

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = getLocker.IdAcademicYear,
                Semester = getLocker.Semester
            });

            var checkStudentReservation = getStudentMasterDataForHeaderReport.Where(x => x.IdStudent == param.IdStudent).ToList();

            if (checkStudentReservation.Count() > 0)
                throw new BadRequestException("Students Have Reserved Lockers");

            var getLockerData = getStudentMasterDataForHeaderReport
                .Where(x => x.IdLocker == param.IdLocker)
                .FirstOrDefault();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var lockerReservation = new TrStudentLockerReservation();

                if (getLockerData.IdStudentLockerReservation == null)
                {
                    if (param.IdStudent == param.IdReserver && getLockerData.Status == true)
                    {
                        throw new BadRequestException("Locker has been Locked");
                    }
                    else
                    {
                        if (getLockerData.Status == true)
                        {
                            getLocker.IsLocked = false;
                            _dbContext.Entity<MsLocker>().Update(getLocker);
                        }

                        lockerReservation = new TrStudentLockerReservation()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLocker = param.IdLocker,
                            IdStudent = param.IdStudent,
                            IdGrade = param.IdGrade,
                            IdHomeroom = param.IdHomeroom,
                            IdAcademicYear = getLocker.IdAcademicYear,
                            Semester = getLocker.Semester,
                            IdReserver = param.IdReserver,
                            IsAgree = param.IsAgree,
                            Notes = param.Notes
                        };

                        _dbContext.Entity<TrStudentLockerReservation>().Add(lockerReservation);
                    }
                }
                else
                {
                     throw new BadRequestException("Locker has been reserved");
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
