using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Employee.FnStaff.MasterSearching
{
    public class ExportMasterSearchingStaffResultToExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;

        private readonly ICurrentAcademicYear _getActiveAcademicYearService;
        private readonly ITeacherPositionInfo _getTeacherPositionByUserIDService;
        private readonly ITeacherByAssignment _getTeacherByAssignmentService;

        public ExportMasterSearchingStaffResultToExcelHandler(IEmployeeDbContext dbContext,
                                                    ICurrentAcademicYear GetActiveAcademicYearService,
                                                    ITeacherPositionInfo GetTeacherPositionByUserID,
                                                    ITeacherByAssignment GetTeacherByAssignment)
        {
            _dbContext = dbContext;
            _getActiveAcademicYearService = GetActiveAcademicYearService;
            _getTeacherPositionByUserIDService = GetTeacherPositionByUserID;
            _getTeacherByAssignmentService = GetTeacherByAssignment;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<ExportToExcelMasterSearchingStaffDataRequest>();

            //if (param.GetAll != true)
            //{

            //    var predicate = PredicateBuilder.False<GetMasterSearchingforStaffResult>();

            //    if (!string.IsNullOrEmpty(param.BinusianID))
            //    {
            //        predicate = predicate.Or(s => s.BinusianID.Contains(param.BinusianID));
            //    }
            //    if (!string.IsNullOrEmpty(param.StaffName))
            //    {
            //        predicate = predicate.Or(s => s.StaffName.Contains(param.StaffName));
            //    }
            //    if (!string.IsNullOrEmpty(param.Email))
            //    {
            //        predicate = predicate.Or(s => s.Email.Contains(param.Email));
            //    }
            //    if (!string.IsNullOrEmpty(param.Initial))
            //    {
            //        predicate = predicate.Or(s => s.Initial.Contains(param.Initial));
            //    }
            //    if (!string.IsNullOrEmpty(param.Position))
            //    {
            //        predicate = predicate.Or(s => s.Position.Contains(param.Position));
            //    }
            //    if (!string.IsNullOrEmpty(param.Department))
            //    {
            //        predicate = predicate.Or(s => s.Department.Contains(param.Department));
            //    }

            //    var query = _dbContext.Entity<MsStaff>()
            //        .Include(x => x.StaffJobInformation)
            //        .Where(x => x.IdSchool == param.SchoolId)
            //        .Where(x => x.IdDesignation == param.DesignationId)
            //        .Select(x => new GetMasterSearchingforStaffResult
            //        {
            //            BinusianID = x.IdBinusian,
            //            StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName) + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
            //            Email = x.BinusianEmailAddress,
            //            Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
            //            Category = x.Designation.DesignationDescription,
            //            Position = x.StaffJobInformation.PositionName,
            //            Department = x.StaffJobInformation.DepartmentName,
            //            SchoolLocation = x.IdSchool
            //        }).Where(predicate);

            //    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
            //    var generateExcelByte = GenerateExcel(items, param.FieldData);
            //    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //    {
            //        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
            //    };

            //    //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
            //}
            //else
            //{

            //    var query = _dbContext.Entity<MsStaff>()
            //        .Include(x => x.StaffJobInformation)
            //        .Where(x => x.IdSchool == param.SchoolId)
            //        .Where(x => x.IdDesignation == param.DesignationId)
            //        .Select(x => new GetMasterSearchingforStaffResult
            //        {
            //            BinusianID = x.IdBinusian,
            //            StaffName = (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName) + (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName),
            //            Email = x.BinusianEmailAddress,
            //            Initial = string.IsNullOrEmpty(x.ShortName) ? "-" : x.ShortName,
            //            Category = x.Designation.DesignationDescription,
            //            Position = x.StaffJobInformation.PositionName,
            //            Department = x.StaffJobInformation.DepartmentName,
            //            SchoolLocation = x.IdSchool
            //        });

            //var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
            //var generateExcelByte = GenerateExcel(items, param.FieldData);
            //return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //{
            //    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
            //};

            //}

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

                if (param.UserId.Equals("superadmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    var query = _dbContext.Entity<MsStaff>()
                    .Include(x => x.StaffJobInformation)
                    .Where(x => x.IdSchool == param.SchoolId)
                    .Where(x => x.IdDesignation == param.DesignationId)
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
                    var generateExcelByte = GenerateExcel(items, param.FieldData);
                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                    };
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
                                                        SchoolLocation = x.IdSchool
                                                    }).Where(predicate);

                                var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                var generateExcelByte = GenerateExcel(items, param.FieldData);
                                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                {
                                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                };
                            }
                            else
                            {
                                var generateExcelByte = GenerateBlankExcel(param.FieldData);
                                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                {
                                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                };
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
                                                        SchoolLocation = x.IdSchool
                                                    }).Where(predicate);

                                var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                var generateExcelByte = GenerateExcel(items, param.FieldData);
                                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                {
                                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                };
                            }
                            else
                            {
                                var generateExcelByte = GenerateBlankExcel(param.FieldData);
                                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                {
                                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                };
                            }

                        }
                        else
                        {
                            var generateExcelByte = GenerateBlankExcel(param.FieldData);
                            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                            };
                        }

                    }
                    else
                    {
                        var generateExcelByte = GenerateBlankExcel(param.FieldData);
                        return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                        };
                    }
                }

            }
            else
            {
                if (param.UserId.Equals("superadmin", StringComparison.CurrentCultureIgnoreCase))
                {
                    var query = _dbContext.Entity<MsStaff>()
                    .Include(x => x.StaffJobInformation)
                    .Where(x => x.IdSchool == param.SchoolId)
                    .Where(x => x.IdDesignation == param.DesignationId)
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
                    });

                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                    var generateExcelByte = GenerateExcel(items, param.FieldData);
                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                    };
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
                                                            SchoolLocation = x.IdSchool
                                                        });

                                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                    var generateExcelByte = GenerateExcel(items, param.FieldData);
                                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                    {
                                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                    };
                                }
                                else
                                {
                                    var generateExcelByte = GenerateBlankExcel(param.FieldData);
                                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                    {
                                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                    };
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
                                                            SchoolLocation = x.IdSchool
                                                        });

                                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                                    var generateExcelByte = GenerateExcel(items, param.FieldData);
                                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                    {
                                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                    };
                                }
                                else
                                {
                                    var generateExcelByte = GenerateBlankExcel(param.FieldData);
                                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                    {
                                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                    };
                                }

                            }
                            else
                            {
                                var generateExcelByte = GenerateBlankExcel(param.FieldData);
                                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                                {
                                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                                };
                            }
                        }
                        else
                        {
                            var generateExcelByte = GenerateBlankExcel(param.FieldData);
                            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                            };
                        }
                    }
                    else
                    {
                        var generateExcelByte = GenerateBlankExcel(param.FieldData);
                        return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                        };
                    }
                }
            }

        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string fieldData)
        {
            var result = new byte[0];
            string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Master Searching Data");

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(0);

                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                foreach (string field in fieldDataList)
                {
                    var Judul = row.CreateCell(fieldCount);
                    Judul.SetCellValue(field);
                    row.Cells[fieldCount].CellStyle = style;
                    fieldCount++;
                }

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcel(List<GetMasterSearchingforStaffResult> data, string fieldData)
        {
            var result = new byte[0];
            string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Master Searching Data");

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(0);

                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                foreach (string field in fieldDataList)
                {
                    var Judul = row.CreateCell(fieldCount);
                    Judul.SetCellValue(field);
                    row.Cells[fieldCount].CellStyle = style;
                    fieldCount++;
                }

                foreach (var item in data)
                {
                    row = excelSheet.CreateRow(row.RowNum + 1);

                    for (int i = 0; i < fieldCount; i++)
                    {
                        var value = (string)item.GetType().GetProperty(fieldDataList[i]).GetValue(item, null);
                        value = string.IsNullOrEmpty(value) ? "" : value;
                        row.CreateCell(i).SetCellValue(value);
                        row.Cells[i].CellStyle = style;
                    }
                }

                //Add Auto FIt
                for (int i = 0; i < fieldCount; i++)
                {
                    excelSheet.AutoSizeColumn(i);
                }

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
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
