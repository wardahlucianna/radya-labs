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
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetLockerAllocationGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLockerAllocationGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLockerAllocationGradeRequest>
                (nameof(GetLockerAllocationGradeRequest.IdAcademicYear),
                nameof(GetLockerAllocationGradeRequest.Semester));

            var gradeFilter = PredicateBuilder.Create<MsGrade>
                (a => a.MsLevel.IdAcademicYear == param.IdAcademicYear);

            var getGrade = _dbContext.Entity<MsGrade>()
                .Include(a => a.MsLevel)
                .Where(gradeFilter)
                .Distinct()
                .ToList();

            var resultList = new List<GetLockerAllocationGradeResult>();

            foreach (var item in getGrade)
            {
                var lockerAllocationFilter = PredicateBuilder.Create<MsLockerAllocation>
                    (a => a.IdAcademicYear == param.IdAcademicYear
                    && a.Semester == param.Semester
                    && a.IdBuilding == param.IdBuilding
                    && a.IdFloor == param.IdFloor);

                var getLockerAllocation = _dbContext.Entity<MsLockerAllocation>()
                    .Include(a => a.Grade)
                        .ThenInclude(b => b.MsLevel)
                    .Where(lockerAllocationFilter)
                    .ToList();

                var result = new GetLockerAllocationGradeResult
                {
                    IdLevel = item.IdLevel,
                    LevelCode = item.MsLevel.Code,
                    LevelDesc = item.MsLevel.Description,
                    Grades = new List<GetLockerAllocationGradeResult_Grade>
                    {
                        new GetLockerAllocationGradeResult_Grade
                        {
                            IdGrade = item.Id,
                            GradeCode = item.Code,
                            GradeDesc = item.Description,
                            OrderNumber = item.OrderNumber,
                            HasAllocated = getLockerAllocation.Any(a => a.IdGrade == item.Id) ? true : false
                        }
                    }
                };

                resultList.Add(result);
            }

            var items = resultList.GroupBy(a => new
            {
                a.IdLevel,
                a.LevelCode,
                a.LevelDesc
            })
                .Select(b => new GetLockerAllocationGradeResult
                {
                    IdLevel = b.Key.IdLevel,
                    LevelCode = b.Key.LevelCode,
                    LevelDesc = b.Key.LevelDesc,
                    Grades = b.SelectMany(c => c.Grades)
                        .OrderBy(c => c.OrderNumber)
                        .ToList()
                })
                .OrderByDescending(a => a.LevelDesc)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
