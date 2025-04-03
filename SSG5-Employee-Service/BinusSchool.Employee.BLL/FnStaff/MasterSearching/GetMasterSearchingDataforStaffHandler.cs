using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Model.Employee.MasterSearching;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Employee.FnStaff.MasterSearching
{
    public class GetMasterSearchingDataforStaffHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;

        private readonly ICurrentAcademicYear _getActiveAcademicYearService;
        private readonly ITeacherPositionInfo _getTeacherPositionByUserIDService;
        private readonly ITeacherByAssignment _getTeacherByAssignmentService;

        public GetMasterSearchingDataforStaffHandler(IEmployeeDbContext dbContext,
                                                    ICurrentAcademicYear GetActiveAcademicYearService,
                                                    ITeacherPositionInfo GetTeacherPositionByUserID,
                                                    ITeacherByAssignment GetTeacherByAssignment)
        {
            _dbContext = dbContext;
            _getActiveAcademicYearService = GetActiveAcademicYearService;
            _getTeacherPositionByUserIDService = GetTeacherPositionByUserID;
            _getTeacherByAssignmentService = GetTeacherByAssignment;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = await Request.GetBody<GetMasterSearchingDataforStaffRequest>();

            var currAY = await _getActiveAcademicYearService.GetActiveAcademicYear(new GetActiveAcademicYearRequest { SchoolID = param.SchoolId });

            if (param.GetAll != true)
            {

                var predicate = PredicateBuilder.False<GetMasterSearchingforStaffResult>();

                if (!string.IsNullOrEmpty(param.BinusianID))
                {
                    predicate = predicate.Or(s => s.BinusianID.Contains(param.BinusianID));
                }
                if (!string.IsNullOrEmpty(param.StaffName))
                {
                    predicate = predicate.Or(s => s.StaffName.Contains(param.StaffName));
                }
                if (!string.IsNullOrEmpty(param.Email))
                {
                    predicate = predicate.Or(s => s.Email.Contains(param.Email));
                }
                if (!string.IsNullOrEmpty(param.Initial))
                {
                    predicate = predicate.Or(s => s.Initial.Contains(param.Initial));
                }
                if (!string.IsNullOrEmpty(param.Position))
                {
                    predicate = predicate.Or(s => s.Position.Contains(param.Position));
                }
                if (!string.IsNullOrEmpty(param.Department))
                {
                    predicate = predicate.Or(s => s.Department.Contains(param.Department));
                }


                /*
                 Date: 2 July 2022
                Modified By: Fikri
                Confirmation : Yohanes Damenta
                Reason: Bypass first for display data, because trnonteachingload still emtpy
                 */
                //if (param.UserId.Equals("superadmin", StringComparison.CurrentCultureIgnoreCase))
                if (true)
                {
                    var query = _dbContext.Entity<MsStaff>()
                    .Include(x => x.StaffJobInformation)
                    .Where(x => x.IdSchool == param.SchoolId)
                    //.Where(x => x.IdDesignation == param.DesignationId)
                    .Where(x => x.IdDesignation == (param.DesignationId == 0 ? x.IdDesignation : param.DesignationId))
                    .Select(x => new GetMasterSearchingforStaffResult
                    {
                        BinusianID = x.IdBinusian,
                        StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                        Email = x.BinusianEmailAddress,
                        Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                        Category = x.Designation.DesignationDescription,
                        Position = x.StaffJobInformation.PositionName,
                        Department = x.StaffJobInformation.DepartmentName,
                        SchoolLocation = x.IdSchool
                    }).Where(predicate);

                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                    var count = await query.CountAsync(CancellationToken);
                    //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                    return Request.CreateApiResult2(items as object);
                }
                else
                {
                    var position = await _getTeacherPositionByUserIDService.GetTeacherPositionByUserID(new GetTeacherPositionByUserIDRequest { UserId = param.UserId });

                    if (position.IsSuccess)
                    {
                        var UserPosition = position.Payload.PositionShortName;

                        if (UserPosition.Equals("hod", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var positionData = JsonConvert.DeserializeObject<HOD>(position.Payload.Data);
                            var teacherAssignmentService = await _getTeacherByAssignmentService.GetTeacherByDepartment(new GetTeacherByDepartmentRequest { AcademicYearId = currAY.Payload.AcademicYearId, Department = positionData.Department.description });

                            if (teacherAssignmentService.IsSuccess)
                            {
                                List<string> teacherList = teacherAssignmentService.Payload.Select(x => x.BinusianId).ToList();

                                var query = _dbContext.Entity<MsStaff>()
                                                    .Include(x => x.StaffJobInformation)
                                                    .Where(x => x.IdSchool == param.SchoolId)
                                                    .Where(x => x.IdDesignation == param.DesignationId)
                                                    .Where(x => teacherList.Contains(x.IdBinusian))
                                                    .Select(x => new GetMasterSearchingforStaffResult
                                                    {
                                                        BinusianID = x.IdBinusian,
                                                        StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                                                        Email = x.BinusianEmailAddress,
                                                        Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                                                        Category = x.Designation.DesignationDescription,
                                                        Position = x.StaffJobInformation.PositionName,
                                                        Department = x.StaffJobInformation.DepartmentName,
                                                        SchoolLocation = param.SchoolName
                                                    }).Where(predicate);

                                var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                var count = await query.CountAsync(CancellationToken);
                                //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                                return Request.CreateApiResult2(items as object);
                            }
                            else
                            {
                                var item = new List<GetMasterSearchingforStaffResult>();
                                var count = item.Count;
                                //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                                return Request.CreateApiResult2(item as object);
                            }

                        }
                        else if (UserPosition.Equals("lh", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var positionData = JsonConvert.DeserializeObject<LH>(position.Payload.Data);
                            var teacherAssignmentService = await _getTeacherByAssignmentService.GetTeacherByGrade(new GetTeacherByGradeRequest { AcademicYearId = currAY.Payload.AcademicYearId, Grade = positionData.Grade.description });

                            if (teacherAssignmentService.IsSuccess)
                            {
                                List<string> teacherList = teacherAssignmentService.Payload.Select(x => x.BinusianId).ToList();

                                var query = _dbContext.Entity<MsStaff>()
                                                    .Include(x => x.StaffJobInformation)
                                                    .Where(x => x.IdSchool == param.SchoolId)
                                                    .Where(x => x.IdDesignation == param.DesignationId)
                                                    .Where(x => teacherList.Contains(x.IdBinusian))
                                                    .Select(x => new GetMasterSearchingforStaffResult
                                                    {
                                                        BinusianID = x.IdBinusian,
                                                        StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                                                        Email = x.BinusianEmailAddress,
                                                        Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                                                        Category = x.Designation.DesignationDescription,
                                                        Position = x.StaffJobInformation.PositionName,
                                                        Department = x.StaffJobInformation.DepartmentName,
                                                        SchoolLocation = param.SchoolName
                                                    }).Where(predicate);

                                var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                var count = await query.CountAsync(CancellationToken);
                                //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                                return Request.CreateApiResult2(items as object);
                            }
                            else
                            {
                                var item = new List<GetMasterSearchingforStaffResult>();
                                var count = item.Count;
                                //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                                return Request.CreateApiResult2(item as object);
                            }

                        }
                        else
                        {
                            var item = new List<GetMasterSearchingforStaffResult>();
                            var count = item.Count;
                            //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                            return Request.CreateApiResult2(item as object);
                        }

                    }
                    else
                    {
                        var item = new List<GetMasterSearchingforStaffResult>();

                        var count = item.Count;

                        //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                        return Request.CreateApiResult2(item as object);
                    }
                }

            }
            else
            {

                //param.GetAll = false;

                //var query = _dbContext.Entity<MsStaff>()
                //    .Include(x => x.StaffJobInformation)
                //    .Where(x => x.IdSchool == param.SchoolId)
                //    .Where(x => x.IdDesignation == param.DesignationId)
                //    .Select(x => new GetMasterSearchingforStaffResult
                //    {
                //        BinusianID = x.IdBinusian,
                //        StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName) + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                //        Email = x.BinusianEmailAddress,
                //        Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                //        Category = "",
                //        Position = x.StaffJobInformation.PositionName,
                //        Department = x.StaffJobInformation.DepartmentName,
                //        SchoolLocation = x.IdSchool
                //    });
                //var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                //var count = await query.CountAsync(CancellationToken);

                //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));

                param.GetAll = false;
                /*
                 Date: 2 July 2022
                Modified By: Fikri
                Confirmation : Yohanes Damenta
                Reason: Bypass first for display data, because trnonteachingload still emtpy
                 */
                //if (param.UserId.Equals("superadmin", StringComparison.CurrentCultureIgnoreCase))
                if (true)
                {
                    var query = _dbContext.Entity<MsStaff>()
                    .Include(x => x.StaffJobInformation)
                    .Where(x => x.IdSchool == param.SchoolId)
                    //.Where(x => x.IdDesignation == param.DesignationId)
                    .Where(x => x.IdDesignation == (param.DesignationId == 0 ? x.IdDesignation : param.DesignationId))
                    .Select(x => new GetMasterSearchingforStaffResult
                    {
                        BinusianID = x.IdBinusian,
                        StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                        Email = x.BinusianEmailAddress,
                        Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                        Category = x.Designation.DesignationDescription,
                        Position = x.StaffJobInformation.PositionName,
                        Department = x.StaffJobInformation.DepartmentName,
                        SchoolLocation = param.SchoolName
                    });

                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                    var count = await query.CountAsync(CancellationToken);
                    //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                    return Request.CreateApiResult2(items as object);
                }
                else
                {
                    var position = await _getTeacherPositionByUserIDService.GetTeacherPositionByUserID(new GetTeacherPositionByUserIDRequest { UserId = param.UserId });

                    if (position.IsSuccess)
                    {
                        var UserPosition = position.Payload.PositionShortName;
                        if (!string.IsNullOrEmpty(UserPosition))
                        {
                            if (UserPosition.Equals("hod", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var positionData = JsonConvert.DeserializeObject<HOD>(position.Payload.Data);
                                var teacherAssignmentService = await _getTeacherByAssignmentService.GetTeacherByDepartment(new GetTeacherByDepartmentRequest { AcademicYearId = currAY.Payload.AcademicYearId, Department = positionData.Department.description });

                                if (teacherAssignmentService.IsSuccess)
                                {
                                    List<string> teacherList = teacherAssignmentService.Payload.Select(x => x.BinusianId).ToList();

                                    var query = _dbContext.Entity<MsStaff>()
                                                        .Include(x => x.StaffJobInformation)
                                                        .Where(x => x.IdSchool == param.SchoolId)
                                                        .Where(x => x.IdDesignation == param.DesignationId)
                                                        .Where(x => teacherList.Contains(x.IdBinusian))
                                                        .Select(x => new GetMasterSearchingforStaffResult
                                                        {
                                                            BinusianID = x.IdBinusian,
                                                            StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                                                            Email = x.BinusianEmailAddress,
                                                            Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                                                            Category = x.Designation.DesignationDescription,
                                                            Position = x.StaffJobInformation.PositionName,
                                                            Department = x.StaffJobInformation.DepartmentName,
                                                            SchoolLocation = param.SchoolName
                                                        });

                                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                    var count = await query.CountAsync(CancellationToken);
                                    //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                                    return Request.CreateApiResult2(items as object);
                                }
                                else
                                {
                                    var item = new List<GetMasterSearchingforStaffResult>();
                                    var count = item.Count;
                                    //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                                    return Request.CreateApiResult2(item as object);
                                }
                            }
                            else if (UserPosition.Equals("lh", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var positionData = JsonConvert.DeserializeObject<LH>(position.Payload.Data);
                                var teacherAssignmentService = await _getTeacherByAssignmentService.GetTeacherByGrade(new GetTeacherByGradeRequest { AcademicYearId = currAY.Payload.AcademicYearId, Grade = positionData.Grade.description });

                                if (teacherAssignmentService.IsSuccess)
                                {
                                    List<string> teacherList = teacherAssignmentService.Payload.Select(x => x.BinusianId).ToList();

                                    var query = _dbContext.Entity<MsStaff>()
                                                        .Include(x => x.StaffJobInformation)
                                                        .Where(x => x.IdSchool == param.SchoolId)
                                                        .Where(x => x.IdDesignation == param.DesignationId)
                                                        .Where(x => teacherList.Contains(x.IdBinusian))
                                                        .Select(x => new GetMasterSearchingforStaffResult
                                                        {
                                                            BinusianID = x.IdBinusian,
                                                            StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
                                                            Email = x.BinusianEmailAddress,
                                                            Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
                                                            Category = x.Designation.DesignationDescription,
                                                            Position = x.StaffJobInformation.PositionName,
                                                            Department = x.StaffJobInformation.DepartmentName,
                                                            SchoolLocation = param.SchoolName
                                                        });

                                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                    var count = await query.CountAsync(CancellationToken);
                                    //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                                    return Request.CreateApiResult2(items as object);
                                }
                                else
                                {
                                    var item = new List<GetMasterSearchingforStaffResult>();
                                    var count = item.Count;
                                    //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                                    return Request.CreateApiResult2(item as object);
                                }

                            }
                            else
                            {
                                var item = new List<GetMasterSearchingforStaffResult>();
                                var count = item.Count;
                                //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                                return Request.CreateApiResult2(item as object);
                            }
                        }
                        else
                        {
                            var item = new List<GetMasterSearchingforStaffResult>();
                            var count = item.Count;
                            //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                            return Request.CreateApiResult2(item as object);
                        }
                    }
                    else
                    {
                        var item = new List<GetMasterSearchingforStaffResult>();

                        var count = item.Count;

                        //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                        return Request.CreateApiResult2(item as object);
                    }
                }
            }

        }

        internal class Department
        {
            public string id { get; set; }
            public string description { get; set; }
        }

        internal class HOD
        {
            public Department Department { get; set; }
        }

        internal class Level
        {
            public string id { get; set; }
            public string description { get; set; }
        }

        internal class Grade
        {
            public string id { get; set; }
            public string description { get; set; }
        }

        internal class Streaming
        {
            public string id { get; set; }
            public string description { get; set; }
        }

        internal class Classroom
        {
            public string id { get; set; }
            public string description { get; set; }
        }

        internal class LH
        {
            public Level Level { get; set; }
            public Grade Grade { get; set; }
            public Streaming Streaming { get; set; }
            public Classroom Classroom { get; set; }
        }


    }
}
