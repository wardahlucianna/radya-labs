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
    public class UpdateLockerReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public UpdateLockerReservationHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateLockerReservationRequest, UpdateLockerReservationValidator>();

            var getLocker = _dbContext.Entity<MsLocker>()
                               .Where(x => x.Id == param.IdLocker)
                               .FirstOrDefault();

            if (getLocker != null)
                throw new BadRequestException($"Locker Not Found");

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = getLocker.IdAcademicYear,
                Semester = getLocker.Semester
            });

            var checkStudentReservation = getStudentMasterDataForHeaderReport.Where(x => x.IdStudent == param.IdStudent).ToList();

            if (checkStudentReservation != null)
                throw new BadRequestException($"Student Already Reserve Locker");

            var getLockerData = getStudentMasterDataForHeaderReport
                .Where(x => x.IdLocker == param.IdLocker)
                .FirstOrDefault();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);


                var lockerReservation = new TrStudentLockerReservation();
                if (getLockerData.IdStudentLockerReservation != null)
                {
                    lockerReservation = new TrStudentLockerReservation() { 
                        IdLocker = param.IdLocker,
                        IdStudent = param.IdStudent,
                        IdGrade = param.IdGrade,
                        IdHomeroom = param.IdHomeroom,
                        IdReserver = param.IdReserver,
                        IsAgree = true,
                        Notes = param.Notes
                    };
                }

                _dbContext.Entity<TrStudentLockerReservation>().Update(lockerReservation);

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

            var retLockerList = new GetLockerListResult();

            return Request.CreateApiResult2(retLockerList as object);
        }
    }
}
