using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class DownloadExcelStudentBlockingByCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public DownloadExcelStudentBlockingByCategoryHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadExcelStudentBlockingByCategoryRequest>();
            
            #region Get data Header From Db
            var dataBlocking1 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category != "FEATURE" && x.BlockingType.IdSchool == param.IdSchool)
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Order)
                .ToListAsync(CancellationToken);

            var dataBlocking2 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category == "FEATURE" && x.BlockingType.IdSchool == param.IdSchool)
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Name)
                .ToListAsync(CancellationToken);

            var dataAccessBlocking = await _dbContext.Entity<MsUserBlocking>()
                .Include(x => x.BlockingCategory)
                .Where(x => x.IdUser == param.IdUser && x.BlockingCategory.IdSchool == param.IdSchool)
                .Select(x=> x.IdBlockingCategory)
                .ToListAsync(CancellationToken);

            dataBlocking1 = dataBlocking1.Where(x => dataAccessBlocking.Contains(x.IdBlockingCategory)).ToList();
            dataBlocking2 = dataBlocking2.Where(x => dataAccessBlocking.Contains(x.IdBlockingCategory)).ToList();

            var dataBlockings = dataBlocking1.Concat(dataBlocking2).OrderBy(x=>x.BlockingCategory.Name).Select(y=> new StudentBlockingByCategoryResult
            {
                BlockingCategory = y.BlockingCategory.Name,
                BlockingTypeId = y.BlockingType.Id,
                BlockingType = y.BlockingType.Name,
            }).ToList();

            var dataBlocking = dataBlocking1.Concat(dataBlocking2).OrderBy(x => x.BlockingCategory.Name).ToList();

            if (!string.IsNullOrEmpty(param.IdBlockingType))
            {
                dataBlockings = dataBlockings.Where(x => x.BlockingTypeId == param.IdBlockingType).ToList();
                dataBlocking = dataBlocking.Where(x => x.IdBlockingType == param.IdBlockingType).ToList();
            }
                
            #endregion

            #region data body
            var dataHomeroom = await (from hrs in _dbContext.Entity<MsHomeroomStudent>()
                                      join st in _dbContext.Entity<MsStudent>() on hrs.IdStudent equals st.Id
                                      join hr in _dbContext.Entity<MsHomeroom>() on hrs.IdHomeroom equals hr.Id
                                      join g in _dbContext.Entity<MsGrade>() on hr.IdGrade equals g.Id
                                      join Level in _dbContext.Entity<MsLevel>() on g.IdLevel equals Level.Id
                                      join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on hr.IdGradePathwayClassRoom equals gpc.Id
                                      join cr in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals cr.Id
                                      join blc in _dbContext.Entity<MsStudentBlocking>() on hrs.IdStudent equals blc.IdStudent
                                      where hrs.Semester.ToString() == param.Semester && blc.IsBlocked == true && hr.IdAcademicYear == param.IdAcademicYear 
                                      && dataAccessBlocking.Any(t => t == blc.IdBlockingCategory)
                                      select new
                                      {
                                          IdStudent = hrs.IdStudent,
                                          Semester = hrs.Semester,
                                          IdHomeRoom = hrs.IdHomeroom,
                                          Homeroom = g.Code + cr.Code,
                                          Student = string.IsNullOrEmpty(st.LastName) ? $"{st.FirstName} {st.MiddleName}" : $"{st.FirstName} {st.LastName}",
                                          IdLevel = Level.Id,
                                          IdGrade = g.Id
                                      }).ToListAsync();

            var query = (from hrs in dataHomeroom
                         select new
                         {
                             IdStudent = hrs.IdStudent,
                             StudentName = hrs.Student,
                             IdLevel = hrs.IdLevel,
                             IdGrade = hrs.IdGrade,
                             Semester = hrs.Semester.ToString(),
                             IdHomeRoom = hrs.IdHomeRoom,
                             Homeroom = hrs.Homeroom,
                         }).Distinct().OrderBy(x => x.Homeroom).ThenBy(x => x.StudentName);

            var responeStudentBlockings = new List<DownloadExcelStudentBlockingByCategoryResult>();

            foreach (var student in query)
            {
                var dataStudentBlockings = new DownloadExcelStudentBlockingByCategoryResult()
                {
                    StudentName = student.StudentName,
                    StudentId = student.IdStudent,
                    HomeRoom = student.Homeroom,
                    Details = new List<DetailDownloadExcelStudentBlockingByCategoryResult>()
                };

                var dataDetailStudentBlockings = new List<DetailDownloadExcelStudentBlockingByCategoryResult>();
                foreach (var item in dataBlocking)
                {
                    var dataDetailStudentBlocking = new DetailDownloadExcelStudentBlockingByCategoryResult();

                    dataDetailStudentBlocking.IsBlocked = item.BlockingCategory.StudentBlockings.Any(x => x.IdStudent == student.IdStudent && x.IdBlockingCategory == item.IdBlockingCategory && x.IdBlockingType == item.IdBlockingType && x.IsBlocked);
                    dataDetailStudentBlocking.ColumnName = $"{item.BlockingCategory.Name}|{item.BlockingType.Name}";

                    dataDetailStudentBlockings.Add(dataDetailStudentBlocking);
                }

                dataStudentBlockings.Details = dataDetailStudentBlockings;

                responeStudentBlockings.Add(dataStudentBlockings);
            }

            #endregion

            var title = "Student Blocking";

            var generateExcelByte = GenerateExcel(title, dataBlockings, responeStudentBlockings);

            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{title}.xlsx"
            };
        }

        public byte[] GenerateExcel(string sheetTitle, List<StudentBlockingByCategoryResult> dataSource, List<DownloadExcelStudentBlockingByCategoryResult> dataStudent)
        {
         
            var columnDummyitem = dataSource.GetRange(0, dataSource.Count)
                  .Select(x => new
                  {
                      ColumnName = $"{x.BlockingCategory} | {x.BlockingType}"
                  })
                  .ToList();

            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

                //Create style
                ICellStyle styleBody = workbook.CreateCellStyle();
                ICellStyle styleHeader = workbook.CreateCellStyle();

                //Set border style 
                styleBody.BorderBottom = BorderStyle.Thin;
                styleHeader.BorderBottom = BorderStyle.Thin;
                styleBody.BorderLeft = BorderStyle.Thin;
                styleHeader.BorderLeft = BorderStyle.Thin;
                styleBody.BorderRight = BorderStyle.Thin;
                styleHeader.BorderRight = BorderStyle.Thin;
                styleBody.BorderTop = BorderStyle.Thin;
                styleHeader.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont fontBody = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                fontBody.FontName = "Arial";
                fontBody.FontHeightInPoints = 10;
                styleBody.SetFont(fontBody);

                var fontHeader = workbook.CreateFont();
                fontHeader.FontName = "Arial";
                fontHeader.FontHeightInPoints = 10;
                fontHeader.IsBold = true;
                styleHeader.SetFont(fontHeader);

                //header 
                var fontBold = workbook.CreateFont();
                fontBold.IsBold = true;
                var boldStyle = workbook.CreateCellStyle();
                boldStyle.SetFont(fontBold);
                List<string> listHeaderKeyStatic = new List<string>();
                List<string> listHeaderKeyDynamic = new List<string>();

                //header information
                IRow rowHeader = excelSheet.CreateRow(0);
                var rowIndex = 1;
                var cellParticipant = rowHeader.CreateCell(0);
                //Column1
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Student Name");
                cellParticipant.CellStyle = styleHeader;
                listHeaderKeyStatic.Add("Student");
                excelSheet.AutoSizeColumn(0);

                //Column2
                cellParticipant = rowHeader.CreateCell(1);
                cellParticipant.SetCellValue("Student ID");
                cellParticipant.CellStyle = styleHeader;
                listHeaderKeyStatic.Add("StudentID");
                excelSheet.AutoSizeColumn(1);

                //Column3
                cellParticipant = rowHeader.CreateCell(2);
                cellParticipant.SetCellValue("Class/Homeroom");
                cellParticipant.CellStyle = styleHeader;
                listHeaderKeyStatic.Add("ClassHomeroom");
                excelSheet.AutoSizeColumn(2);

                //Dynamic Columns
                //
                var columnCounter = 2;
                foreach (var column in columnDummyitem)
                {
                    columnCounter += 1;
                    cellParticipant = rowHeader.CreateCell(columnCounter);
                    cellParticipant.SetCellValue(column.ColumnName.ToUpper());
                    listHeaderKeyDynamic.Add(column.ColumnName.Replace(" ", string.Empty));
                    cellParticipant.CellStyle = styleHeader;
                    excelSheet.AutoSizeColumn(columnCounter);
                }

                foreach (var itemBody in dataStudent) {
                    rowHeader = excelSheet.CreateRow(rowIndex);
                    var startColumn = 0;
                    
                    var cekDataDetailBlocking = itemBody.Details.Where(x => x.IsBlocked).ToList();
                    if (!cekDataDetailBlocking.Any())
                        continue;

                    foreach (var itemHeaderKey in listHeaderKeyStatic)
                    {
                        var value = string.Empty;
                        switch (itemHeaderKey)
                        {
                            case "Student":
                                value = itemBody.StudentName;
                                break;
                            case "StudentID":
                                value = itemBody.StudentId;
                                break;
                            case "ClassHomeroom":
                                value = itemBody.HomeRoom;
                                break;
                            default:
                                value = string.Empty;
                                break;
                        }

                        cellParticipant = rowHeader.CreateCell(startColumn);
                        cellParticipant.SetCellValue(value.ToString());
                        cellParticipant.CellStyle = styleBody;
                        excelSheet.AutoSizeColumn(startColumn);
                        startColumn++;                    
                    }

                    foreach (var itemHeaderKey in listHeaderKeyDynamic)
                    {
                        foreach (var itemDetail in itemBody.Details)
                        {
                            var value = string.Empty;
                            var columnName = itemDetail.ColumnName.Replace(" ", string.Empty);
                            if (itemHeaderKey == columnName)
                            {
                                if (itemDetail.IsBlocked)
                                    value = "X";

                                cellParticipant = rowHeader.CreateCell(startColumn);
                                cellParticipant.SetCellValue(value.ToString());
                                cellParticipant.CellStyle = styleBody;
                                excelSheet.AutoSizeColumn(startColumn);
                                startColumn++;
                            }
                        }
                    }
                    rowIndex++;
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }
    }
}
