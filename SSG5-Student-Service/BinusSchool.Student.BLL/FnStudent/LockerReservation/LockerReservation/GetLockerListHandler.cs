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
    public class GetLockerListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public GetLockerListHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetLockerListRequest, GetLockerListValidator>();

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = param.Semester
                });

            getStudentMasterDataForHeaderReport = getStudentMasterDataForHeaderReport
                .Where(x => x.IdBuilding == param.IdBuilding)
                .Where(x => x.IdFloor == param.IdFloor)
                .ToList();

            var retLockerList = new GetLockerListResult();

            if (getStudentMasterDataForHeaderReport.Count() > 0)
            {
                if (param.SearchBy != null)
                {
                    if (param.SearchBy == 0)
                    {
                        getStudentMasterDataForHeaderReport = getStudentMasterDataForHeaderReport.Where(x => x.IdStudent != null && x.IdStudent.Contains(param.Keyword)).ToList();
                    }
                    if (param.SearchBy == 1)
                    {
                        getStudentMasterDataForHeaderReport = getStudentMasterDataForHeaderReport.Where(x => x.StudentName != null && x.StudentName.Contains(param.Keyword)).ToList();
                    }
                    if (param.SearchBy == 2)
                    {
                        getStudentMasterDataForHeaderReport = getStudentMasterDataForHeaderReport.Where(x => x.GradeName != null && x.GradeName.Contains(param.Keyword)).ToList();
                    }
                }

                var lockerList = getStudentMasterDataForHeaderReport
                    .Select(x => new GetLockerListResult_Detail
                    {
                        IdLocker = x.IdLocker,
                        LockerName = x.LockerName,
                        LockerPosition = x.LockerPosition,
                        LockerPositionName = x.LockerPositionName,
                        FloorName = x.FloorName,
                        BuildingName = x.BuildingName,
                        Status = x.Status == true ? "locked" : ((x.Status == false && x.IdStudentLockerReservation != null) ? "reserved" : "available"),
                        LockerReservation = x.IdStudentLockerReservation != null ? new GetLockerListResult_LockerReservation
                        {
                            IdStudentLockerReservation = x.IdStudentLockerReservation,
                            Student = new NameValueVm
                            {
                                Id = x.IdStudent,
                                Name = x.StudentName
                            },
                            Homeroom = x.HomeroomName
                        } : null
                    }).OrderBy(x => x.BuildingName).ThenBy(x => x.FloorName).ThenBy(x => x.LockerName)
                    .ToList();

                retLockerList = new GetLockerListResult()
                    {
                        UpperLocker = lockerList.Where(x => x.LockerPosition == true).ToList(),
                        LowerLocker = lockerList.Where(x => x.LockerPosition == false).ToList(),
                    };
            }

            return Request.CreateApiResult2(retLockerList as object);
        }
    }
}
