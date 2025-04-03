using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Api.Student.FnStudent;
using System.Linq;
using BinusSchool.Common.Utils;
using System.Net.Http;
using BinusSchool.Common.Abstractions;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Data.Api.School.FnSchool;

namespace BinusSchool.Student.FnStudent.Achievement
{
    public class DownloadAchievementHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IAchievement _achievementService;
        private readonly IStorageManager _storageManager;
        private readonly ISchool _schoolService;
        private readonly IMachineDateTime _datetime;

        public DownloadAchievementHandler(IStudentDbContext dbContext, IAchievement achievementService, IStorageManager storageManager, ISchool schoolService, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _achievementService = achievementService;
            _storageManager = storageManager;
            _schoolService = schoolService;
            _datetime = datetime;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.AddMonths(1);

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }
        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<GetAchievementRequest>();

            #region GetData
            var getApiAchievement = await _achievementService.GetAchievement(param);
            var getAchievement = getApiAchievement.IsSuccess ? getApiAchievement.Payload.ToList() : null;
            var listAchievement = getAchievement.Where(e => e.Status == "Approved").ToList();

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                            .Where(e=>e.Id==param.IdAcademicYear)
                            .Select(e=>e.IdSchool)
                            .FirstOrDefaultAsync(CancellationToken);

            var result = await _schoolService.GetSchoolDetail(idSchool);
            var schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);
            byte[] logo = default;
            if (!string.IsNullOrEmpty(schoolResult.LogoUrl))
            {
                var blobNameLogo = schoolResult.LogoUrl;
                var blobContainerLogo = await _storageManager.GetOrCreateBlobContainer("school-logo", ct: CancellationToken);
                var blobClientLogo = blobContainerLogo.GetBlobClient(blobNameLogo);

                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClientLogo);

                using var client = new HttpClient();
                logo = await client.GetByteArrayAsync(sasUri.AbsoluteUri);
            }

            #endregion

            var title = "Achievement";
            var generateExcelByte = GenerateExcel(title, listAchievement, logo);
            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
            };
        }

        public byte[] GenerateExcel(string sheetTitle,List<GetAchievementResult> listMerit, byte[] logo)
        {
            var result = new byte[0];
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

                //Create style
                //header with bolder
                var headerBoldWithBoder = workbook.CreateFont();
                headerBoldWithBoder.IsBold = true;
                var headerStyleWithBoder = workbook.CreateCellStyle();
                headerStyleWithBoder.BorderBottom = BorderStyle.Thin;
                headerStyleWithBoder.BorderLeft = BorderStyle.Thin;
                headerStyleWithBoder.BorderRight = BorderStyle.Thin;
                headerStyleWithBoder.BorderTop = BorderStyle.Thin;
                headerStyleWithBoder.SetFont(headerBoldWithBoder);

                //header without bolder
                var headerBold = workbook.CreateFont();
                headerBold.IsBold = true;
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.SetFont(headerBold);

                //body
                var bodyBold = workbook.CreateFont();
                var bodyStyle = workbook.CreateCellStyle();
                bodyStyle.BorderBottom = BorderStyle.Thin;
                bodyStyle.BorderLeft = BorderStyle.Thin;
                bodyStyle.BorderRight = BorderStyle.Thin;
                bodyStyle.BorderTop = BorderStyle.Thin;
                bodyStyle.SetFont(bodyBold);

                List<string> listHeader = new List<string>()
                {
                    "Achievement Name",
                    "Achievement Category",
                    "Focus Area",
                    "Student Name",
                    "Date of Completion",
                    "Point",
                    "Approval Notes",
                    "Status",
                };


                if (logo != null)
                {
                    byte[] dataImg = logo;
                    int pictureIndex = workbook.AddPicture(dataImg, PictureType.PNG);
                    ICreationHelper helper = workbook.GetCreationHelper();
                    IDrawing drawing = excelSheet.CreateDrawingPatriarch();
                    IClientAnchor anchor = helper.CreateClientAnchor();
                    anchor.Col1 = 0;//0 index based column
                    anchor.Row1 = 0;//0 index based row
                    IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                    picture.Resize(1.0, 2.5);
                }

                var indexRow = 2;
                int startColumn = 3;
                IRow rowHeader = excelSheet.CreateRow(indexRow);
                var cellParticipant = rowHeader.CreateCell(startColumn);
                var merge = new NPOI.SS.Util.CellRangeAddress(2, 2, startColumn, startColumn + 3);
                cellParticipant.SetCellValue("SUMMARY DATA MERIT & ACHIEVEMENT");
                cellParticipant.CellStyle = headerStyle;
                indexRow ++;

                var indexcell = 0;
                rowHeader = excelSheet.CreateRow(indexRow);
                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue("Data Per");
                indexcell++;

                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue(_datetime.ServerTime.ToString("dd MMM yyyy HH:mm"));
                indexcell++;
                indexRow++;

                indexcell = 0;
                rowHeader = excelSheet.CreateRow(indexRow);
                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue("Academic Year");
                indexcell++;

                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue(listMerit.Select(e=>e.AcademicYear).FirstOrDefault());
                indexcell++;
                indexRow++;

                indexcell = 0;
                rowHeader = excelSheet.CreateRow(indexRow);
                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue("Semester");
                indexcell++;

                var semester = listMerit.Select(e => e.Semester.ToString()).Distinct().ToList();
                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue(semester.Count() == 2 ? "All" : semester.FirstOrDefault());
                indexcell++;
                indexRow++;

                indexRow += 1;
                indexcell = 0;
                rowHeader = excelSheet.CreateRow(indexRow);
                foreach (var item in listHeader)
                {
                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(item);
                    cellParticipant.CellStyle = headerStyleWithBoder;
                    indexcell++;
                }

                //value
                indexRow += 1;
                foreach (var itemData in listMerit)
                {
                    indexcell = 0;
                    rowHeader = excelSheet.CreateRow(indexRow);
                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.AchievementName);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.AchievementCategory);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.FocusArea);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(NameUtil.GenerateFullName(itemData.Student.IdStudent, itemData.Student.StudentName));
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(Convert.ToDateTime(itemData.DateOfCompletion).ToString("dd-MM-yy"));
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Point);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.ApprovalNote);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Status);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;
                    indexRow++;
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
