using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class SaveLockerAllocationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveLockerAllocationHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveLockerAllocationRequest, SaveLockerAllocationValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getLocker = _dbContext.Entity<MsLocker>()
                    .Where(a => a.IdAcademicYear == param.IdAcademicYear
                        && a.Semester == param.Semester
                        && a.IdBuilding == param.IdBuilding
                        && a.IdFloor == param.IdFloor
                        && a.IsActive == true)
                    .ToList();

                var studentLockerReservations = _dbContext.Entity<TrStudentLockerReservation>()
                    .Where(a => a.IdAcademicYear == param.IdAcademicYear
                        && a.Semester == param.Semester
                        && getLocker.Select(a => a.Id).Contains(a.IdLocker))
                    .ToList();

                if (studentLockerReservations.Any())
                {
                    throw new Exception("Can't edit. Locker(s) have been reserved in this period.");
                }

                var getLockerAllocation = _dbContext.Entity<MsLockerAllocation>()
                    .Where(a => a.IdAcademicYear == param.IdAcademicYear
                        && a.Semester == param.Semester
                        && a.IdBuilding == param.IdBuilding
                        && a.IdFloor == param.IdFloor)
                    .ToList();

                if (getLockerAllocation != null)
                {
                    foreach (var items in getLockerAllocation)
                    {
                        items.IsActive = false;

                        _dbContext.Entity<MsLockerAllocation>().Update(items);
                    }
                }

                var getGrade = _dbContext.Entity<MsGrade>()
                    .Include(a => a.MsLevel)
                    .Where(a => a.MsLevel.IdAcademicYear == param.IdAcademicYear);

                foreach (var items in param.Grades)
                {
                    var getIdGrade = getGrade
                        .Where(a => a.Id == items)
                        .FirstOrDefault();

                    var insertLockerAllocation = new MsLockerAllocation
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = param.IdAcademicYear,
                        Semester = param.Semester,
                        IdLevel = $"{getIdGrade.IdLevel}",
                        IdGrade = items,
                        IdBuilding = param.IdBuilding,
                        IdFloor = param.IdFloor
                    };

                    _dbContext.Entity<MsLockerAllocation>().Add(insertLockerAllocation);
                }

                var countLocker = getLocker.Count();

                if (param.TotalLocker != countLocker)
                {
                    if (getLocker != null)
                    {
                        foreach (var items in getLocker)
                        {
                            items.IsActive = false;

                            _dbContext.Entity<MsLocker>().Update(items);
                        }

                        await _dbContext.SaveChangesAsync(CancellationToken);
                    }       

                    var getLockerPosition = _dbContext.Entity<LtLockerPosition>();

                    for (int i = 1; i <= param.TotalLocker; i++)
                    {
                        var getFloor = _dbContext.Entity<MsFloor>()
                            .Where(a => a.Id == param.IdFloor)
                            .FirstOrDefault();

                        var lockerNumber = i.ToString("D3");

                        var lockerPosition = i <= Math.Ceiling(param.TotalLocker / 2.0) ? "Upper" : "Lower";

                        var getIdLockerPosition = getLockerPosition
                            .Where(a => a.PositionName == lockerPosition)
                            .FirstOrDefault();

                        var insertNewLocker = new MsLocker
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdBuilding = param.IdBuilding,
                            IdFloor = param.IdFloor,
                            LockerName = $"{getFloor.FloorName + getFloor.LockerTowerCodeName}-{lockerNumber}",
                            IdLockerPosition = $"{getIdLockerPosition.Id}",
                            IsLocked = false
                        };

                        _dbContext.Entity<MsLocker>().Add(insertNewLocker);
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
