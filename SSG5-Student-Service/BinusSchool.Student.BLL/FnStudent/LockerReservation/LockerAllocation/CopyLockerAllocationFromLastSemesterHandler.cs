using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class CopyLockerAllocationFromLastSemesterHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public CopyLockerAllocationFromLastSemesterHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CopyLockerAllocationFromLastSemesterRequest, CopyLockerAllocationFromLastSemesterValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getLocker = _dbContext.Entity<MsLocker>()
                    .Where(a => a.IdAcademicYear == param.IdAcademicYear
                        && a.Semester == param.Semester
                        && a.IsActive == true)
                    .ToList();

                var getStudentLockerReservation = _dbContext.Entity<TrStudentLockerReservation>()
                    .Where(a => a.IdAcademicYear == param.IdAcademicYear
                        && a.Semester == param.Semester
                        && getLocker.Select(a => a.Id).Contains(a.IdLocker))
                    .ToList();

                if (getStudentLockerReservation.Any())
                {
                    throw new Exception("Can't copy. Locker(s) have been reserved in this period.");
                }

                var deleteLockerAllocation = _dbContext.Entity<MsLockerAllocation>()
                        .Where(a => a.IdAcademicYear == param.IdAcademicYear
                            && a.Semester == param.Semester)
                        .ToList();

                if (deleteLockerAllocation != null)
                {
                    foreach (var items in deleteLockerAllocation)
                    {
                        items.IsActive = false;

                        _dbContext.Entity<MsLockerAllocation>().Update(items);
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                if (getLocker != null)
                {
                    foreach (var items in getLocker)
                    {
                        items.IsActive = false;

                        _dbContext.Entity<MsLocker>().Update(items);
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                if (param.Semester == 1)
                {
                    var getAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                        .Where(a => a.Id == param.IdAcademicYear)
                        .FirstOrDefaultAsync(CancellationToken);

                    var getPrevAcademicYear = int.Parse(getAcademicYear.Code) - 1;

                    var prevAcademicYear = getPrevAcademicYear.ToString();

                    var getLockerAllocation = _dbContext.Entity<MsLockerAllocation>()
                        .Include(a => a.AcademicYear)
                        .Where(a => a.Semester == 2
                            && a.AcademicYear.Code == prevAcademicYear);

                    foreach (var items in getLockerAllocation)
                    {
                        var copyLockerAllocation = new MsLockerAllocation
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdLevel = items.IdLevel,
                            IdGrade = items.IdGrade,
                            IdBuilding = items.IdBuilding,
                            IdFloor = items.IdFloor
                        };

                        _dbContext.Entity<MsLockerAllocation>().Add(copyLockerAllocation);
                    }

                    var getPrevLocker = _dbContext.Entity<MsLocker>()
                        .Include(a => a.AcademicYear)
                        .Where(a => a.Semester == 2
                            && a.AcademicYear.Code == prevAcademicYear);

                    foreach (var items in getPrevLocker)
                    {
                        var copyLocker = new MsLocker
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdBuilding = items.IdBuilding,
                            IdFloor = items.IdFloor,
                            LockerName = items.LockerName,
                            IdLockerPosition = items.IdLockerPosition,
                            IsLocked = items.IsLocked
                        };

                        _dbContext.Entity<MsLocker>().Add(copyLocker);
                    }
                }
                else if (param.Semester == 2)
                {
                    var getLockerAllocation = _dbContext.Entity<MsLockerAllocation>()
                        .Include(a => a.AcademicYear)
                        .Where(a => a.IdAcademicYear == param.IdAcademicYear
                            && a.Semester == 1);

                    foreach (var items in getLockerAllocation)
                    {
                        var copyLockerAllocation = new MsLockerAllocation
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdLevel = items.IdLevel,
                            IdGrade = items.IdGrade,
                            IdBuilding = items.IdBuilding,
                            IdFloor = items.IdFloor
                        };

                        _dbContext.Entity<MsLockerAllocation>().Add(copyLockerAllocation);
                    }

                    var getPrevLocker = _dbContext.Entity<MsLocker>()
                        .Include(a => a.AcademicYear)
                        .Where(a => a.IdAcademicYear == param.IdAcademicYear
                            && a.Semester == 1);

                    foreach (var items in getPrevLocker)
                    {
                        var copyLocker = new MsLocker
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdBuilding = items.IdBuilding,
                            IdFloor = items.IdFloor,
                            LockerName = items.LockerName,
                            IdLockerPosition = items.IdLockerPosition,
                            IsLocked = items.IsLocked
                        };

                        _dbContext.Entity<MsLocker>().Add(copyLocker);
                    }
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
