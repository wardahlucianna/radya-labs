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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation
{
    public class SaveLockedLockerHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public SaveLockedLockerHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveLockedLockerRequest, SaveLockedLockerValidator>();

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester
            });

            getStudentMasterDataForHeaderReport = getStudentMasterDataForHeaderReport
                .Where(x => x.IdBuilding == param.IdBuilding)
                .Where(x => x.IdFloor == param.IdFloor)
                .ToList();

            var getVacantLocker = getStudentMasterDataForHeaderReport
                .Where(x => x.IdStudentLockerReservation == null)
                .Where(x => x.LockerPosition == (((param.UpperLocker ?? 0) != 0) ? true : x.LockerPosition) || x.LockerPosition == (((param.LowerLocker ?? 0) != 0) ? false : x.LockerPosition))
                .Select(x => x.IdLocker)
                .Distinct()
                .ToList();

            var countUpperLocker = getStudentMasterDataForHeaderReport.Where(x => x.LockerPosition == true).Count();
            var countLowerLocker = getStudentMasterDataForHeaderReport.Where(x => x.LockerPosition == false).Count();

            if (((param.UpperLocker ?? 0) != 0) && countUpperLocker < param.UpperLocker)
                throw new BadRequestException($"Total Locked Upper Locker Can not be Greater Than Vacant Locker");

            if (((param.LowerLocker ?? 0) != 0) && countLowerLocker < param.LowerLocker)
                throw new BadRequestException($"Total Locked Lower Locker Can not be Greater Than Vacant Locker");

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var lockedLocker = new List<MsLocker>();

                var lockedLockerList = _dbContext.Entity<MsLocker>()
                                .Include(x => x.LockerPosition)
                                .Where(x => getVacantLocker.Any(y => y == x.Id))
                                .OrderBy(x => x.LockerName)
                                .ToList();

                if(((param.UpperLocker ?? 0) != 0) || ((param.LowerLocker ?? 0) != 0))
                {
                    if (param.UpperLocker != 0)
                    {
                        var lockedLockerListData = lockedLockerList.Where(x => x.LockerPosition.LockerPosition == true).Take(param.UpperLocker ?? 0).OrderBy(x => x.LockerName).ToList();
                        lockedLocker.AddRange(lockedLockerListData);
                    }
                    if (param.LowerLocker != 0)
                    {
                        var lockedLockerListData = lockedLockerList.Where(x => x.LockerPosition.LockerPosition == false).Take(param.LowerLocker ?? 0).OrderBy(x => x.LockerName).ToList();
                        lockedLocker.AddRange(lockedLockerListData);
                    }
                }
                else
                {
                    lockedLocker.AddRange(lockedLockerList);
                }

                lockedLocker.ForEach(x => x.IsLocked = true);

                _dbContext.Entity<MsLocker>().UpdateRange(lockedLocker);

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
