using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetAllLockerAllocationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetAllLockerAllocationHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllLockerAllocationRequest>
                (nameof(GetAllLockerAllocationRequest.IdAcademicYear),
                nameof(GetAllLockerAllocationRequest.Semester));

            var getLockerAllocation = _dbContext.Entity<MsLockerAllocation>()
                .Join(_dbContext.Entity<MsLocker>(),
                    la => new { la.IdAcademicYear, la.Semester, la.IdBuilding, la.IdFloor },
                    l => new { l.IdAcademicYear, l.Semester, l.IdBuilding, l.IdFloor },
                    (la, l) => new { la, l })
                .Where(a => a.la.IdAcademicYear == param.IdAcademicYear
                    && a.la.Semester == param.Semester
                    && a.la.IsActive == true
                    && a.l.IdAcademicYear == param.IdAcademicYear
                    && a.l.Semester == param.Semester
                    && a.l.IsActive == true)
                .Select(a => new
                {
                    IdAcademicYear = a.la.IdAcademicYear,
                    AcademicYearCode = a.la.AcademicYear.Code,
                    Semester = a.la.Semester,
                    IdBuilding = a.la.IdBuilding,
                    BuildingDesc = a.la.Building.Description,
                    IdFloor = a.la.IdFloor,
                    FloorDesc = a.la.Floor.FloorName,
                    IdGrade = a.la.IdGrade,
                    GradeDesc = a.la.Grade.Description,
                    OrderNumber = a.la.Grade.OrderNumber
                })
                .Distinct()
                .ToList();

            var items = getLockerAllocation.GroupBy(a => new
            {
                a.IdAcademicYear,
                a.AcademicYearCode,
                a.Semester,
                a.IdBuilding,
                a.BuildingDesc,
                a.IdFloor,
                a.FloorDesc,
                a.IdGrade,
                a.GradeDesc,
                a.OrderNumber
            })
                .Select(b => new
                {
                    IdAcademicYear = b.Key.IdAcademicYear,
                    AcademicYearCode = b.Key.AcademicYearCode,
                    Semester = b.Key.Semester,
                    IdBuilding = b.Key.IdBuilding,
                    BuildingDesc = b.Key.BuildingDesc,
                    IdFloor = b.Key.IdFloor,
                    FloorDesc = b.Key.FloorDesc,
                    IdGrade = b.Key.IdGrade,
                    GradeDesc = b.Key.GradeDesc,
                    OrderNumber = b.Key.OrderNumber,
                    TotalLocker = _dbContext.Entity<MsLocker>()
                        .Count(l => l.IdAcademicYear == b.Key.IdAcademicYear
                            && l.Semester == b.Key.Semester
                            && l.IdBuilding == b.Key.IdBuilding
                            && l.IdFloor == b.Key.IdFloor)
                })
                .GroupBy(c => new
                {
                    c.IdAcademicYear,
                    c.AcademicYearCode,
                    c.Semester,
                    c.IdBuilding,
                    c.BuildingDesc,
                    c.IdFloor,
                    c.FloorDesc,
                    c.TotalLocker
                })
                .Select(d => new GetAllLockerAllocationResult
                {
                    AcademicYear = new ItemValueVm
                    {
                        Id = d.Key.IdAcademicYear,
                        Description = d.Key.AcademicYearCode
                    },

                    Semester = d.Key.Semester,

                    Building = new ItemValueVm
                    {
                        Id = d.Key.IdBuilding,
                        Description = d.Key.BuildingDesc
                    },

                    Floor = new ItemValueVm
                    {
                        Id = d.Key.IdFloor,
                        Description = d.Key.FloorDesc
                    },

                    Grades = d.Select(b => new GetAllLockerAllocationResult_Grade
                    {
                        Id = b.IdGrade,
                        Description = b.GradeDesc,
                        OrderNumber = b.OrderNumber
                    })
                        .OrderBy(b => b.OrderNumber)
                        .ToList(),

                    TotalLocker = d.Key.TotalLocker
                })
                .OrderByDescending(a => a.AcademicYear.Description)
                    .ThenBy(a => a.Building.Description)
                    .ThenBy(a => a.Floor.Description)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
