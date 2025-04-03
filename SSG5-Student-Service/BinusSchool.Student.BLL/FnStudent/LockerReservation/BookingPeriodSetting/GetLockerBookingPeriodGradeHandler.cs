using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerBookingPeriodGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLockerBookingPeriodGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLockerBookingPeriodGradeRequest>
                (nameof(GetLockerBookingPeriodGradeRequest.IdAcademicYear),
                nameof(GetLockerBookingPeriodGradeRequest.Semester));

            var predicate = PredicateBuilder.Create<MsGrade>
                (a => a.MsLevel.IdAcademicYear == param.IdAcademicYear);

            var getGrade = _dbContext.Entity<MsGrade>()
                .Include(a => a.MsLevel)
                .Where(predicate)
                .Distinct()
                .ToList();

            var resultList = new List<GetLockerBookingPeriodGradeResult>();

            foreach (var item in getGrade)
            {
                var lockerReservationPeriodFilter = PredicateBuilder.Create<MsLockerReservationPeriod>
                    (a => a.IdAcademicYear == param.IdAcademicYear
                    && a.Semester == param.Semester);

                var getLockerReservationPeriod = _dbContext.Entity<MsLockerReservationPeriod>()
                    .Include(a => a.Grade)
                        .ThenInclude(b => b.MsLevel)
                    .Where(lockerReservationPeriodFilter)
                    .ToList();

                var result = new GetLockerBookingPeriodGradeResult
                {
                    Grades = new List<GetLockerBookingPeriodGradeResult_Grade>
                    {
                        new GetLockerBookingPeriodGradeResult_Grade
                        {
                            Id = item.Id,
                            Code = item.Code,
                            Description = item.Description,
                            OrderNumber = item.OrderNumber,
                            HasBookingPeriod = getLockerReservationPeriod.Any(a => a.IdGrade == item.Id) ? true : false
                        }
                    },
                    IdLevel = item.IdLevel,
                    LevelCode = item.MsLevel.Code,
                    LevelDescription = item.MsLevel.Description
                };

                resultList.Add(result);
            }

            var items = resultList.GroupBy(a => new
            {
                a.IdLevel,
                a.LevelCode,
                a.LevelDescription
            })
                    .Select(b => new GetLockerBookingPeriodGradeResult
                    {
                        IdLevel = b.Key.IdLevel,
                        LevelCode = b.Key.LevelCode,
                        LevelDescription = b.Key.LevelDescription,
                        Grades = b.SelectMany(c => c.Grades)
                            .OrderBy(c => c.OrderNumber)
                            .ToList()
                    })
                    .OrderByDescending(a => a.LevelCode)
                    .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
