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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerReservationPeriodPolicyHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLockerReservationPeriodPolicyHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLockerReservationPeriodPolicyRequest>
                (nameof(GetLockerReservationPeriodPolicyRequest.IdAcademicYear),
                 nameof(GetLockerReservationPeriodPolicyRequest.Semester)
                );

            var predicate = PredicateBuilder.Create<MsLockerReservationPeriod>
                (a => a.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                && a.Semester == param.Semester);

            var query = _dbContext.Entity<MsLockerReservationPeriod>()
                .Include(a => a.Grade)
                    .ThenInclude(b => b.MsLevel)
                .Where(predicate)
                .Distinct()
                .ToList();

            var items = query.Select(a => new GetLockerReservationPeriodPolicyResult
            {
                IdLockerReservationPeriod = a.Id,
                Grade = new ItemValueVm
                {
                    Id = a.IdGrade,
                    Description = a.Grade.Description
                },
                GradeOrderNumber = a.Grade.OrderNumber,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                PolicyMessage = a.PolicyMessage
            })
                .OrderBy(a => a.GradeOrderNumber)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
