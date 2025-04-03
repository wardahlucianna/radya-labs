using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation
{
    public class GetListLockerReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public GetListLockerReservationHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetListLockerReservationRequest, GetListLockerReservationValidator>();

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester
            });

            var lockerReservationList = getStudentMasterDataForHeaderReport
                .Select(x => new { Id = x.IdFloor + "#" + x.IdBuilding, Desc = $"{x.FloorName} - {x.BuildingName}" })
                .Distinct()
                .OrderBy(x => x.Desc)
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Desc
                })
                .ToList();

            return Request.CreateApiResult2(lockerReservationList as object);
        }
    }
}
