using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class DownloadExcelMeritDemeritTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly ISchool _schoolService;
        private readonly IStorageManager _storageManager;

        public DownloadExcelMeritDemeritTeacherHandler(IStudentDbContext EntryMeritDemetitDbContext, IMachineDateTime datetime, ISchool schoolService, IStorageManager storageManager)
        {
            _dbContext = EntryMeritDemetitDbContext;
            _datetime = datetime;
            _schoolService = schoolService;
            _storageManager = storageManager;
        }


        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<GetDownloadTeacherMeritDemeritTeacherRequest>();

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                           .Where(e => e.Id == param.IdAcademicYear)
                           .FirstOrDefaultAsync(CancellationToken);

            var idSchool = GetAcademicYear.IdSchool;
            var academicYear = GetAcademicYear.Description;

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

            var title = "MeritDemeritTeacher";
            var generateExcelByte = GenerateExcel(title, param, logo, academicYear);
            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
            };
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.AddMonths(1);

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }

        public byte[] GenerateExcel(string sheetTitle, GetDownloadTeacherMeritDemeritTeacherRequest param, byte[] logo, string academicYear)
        {

            var query = (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         join User in _dbContext.Entity<MsUser>() on Student.Id equals User.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join StudentPoint in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals StudentPoint.IdHomeroomStudent into JoinedStudentPoint
                         from StudentPoint in JoinedStudentPoint.DefaultIfEmpty()
                         join LevelOfInteraction in _dbContext.Entity<MsLevelOfInteraction>() on StudentPoint.IdLevelOfInteraction equals LevelOfInteraction.Id into JoinedLevelOfInteraction
                         from LevelOfInteraction in JoinedLevelOfInteraction.DefaultIfEmpty()
                         join SanctionMapping in _dbContext.Entity<MsSanctionMapping>() on StudentPoint.IdSanctionMapping equals SanctionMapping.Id into JoinedSanctionMapping
                         from SanctionMapping in JoinedSanctionMapping.DefaultIfEmpty()
                         where Level.IdAcademicYear == param.IdAcademicYear && Level.Id == param.IdLevel && Grade.Id == param.IdGrade && Homeroom.Semester == param.Semester
                         select new
                         {
                             AcademicYear = AcademicYear.Description,
                             Semester = Homeroom.Semester.ToString(),
                             Level = Level.Description,
                             Grade = Grade.Description,
                             Homeroom = (Grade.Code) + (Classroom.Code),
                             IdStudent = Student.Id,
                             NameStudent = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                             Demerit = StudentPoint != null ? StudentPoint.DemeritPoint : 0,
                             Merit = StudentPoint != null ? StudentPoint.MeritPoint : 0,
                             LevelOfInfraction = LevelOfInteraction.IdParentLevelOfInteraction == null ? LevelOfInteraction != null ? LevelOfInteraction.NameLevelOfInteraction : string.Empty : LevelOfInteraction.Parent.NameLevelOfInteraction + LevelOfInteraction.NameLevelOfInteraction,
                             LastUpdate = StudentPoint.DateUp == null ? StudentPoint.DateIn.GetValueOrDefault() : StudentPoint.DateUp.GetValueOrDefault(),
                             Sanction = SanctionMapping != null ? SanctionMapping.SanctionName : string.Empty,
                             IdHomeroom = HomeroomStudent.IdHomeroom
                         });


            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdStudent.Contains(param.Search) || x.NameStudent.Contains(param.Search));

            var trStudentPoint = query.ToList();

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
                    "Academic Year",
                    "Semester",
                    "Level",
                    "Grade",
                    "Homeroom",
                    "Student ID",
                    "Student Name",
                    "Merit",
                    "Accountability Points",
                    "Level of Infraction",
                    "Sanction",
                    "Last Date",
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
                cellParticipant.SetCellValue("SUMMARY DATA MERIT /ACCOUNTABILITY POINTS");
                cellParticipant.CellStyle = headerStyle;
                indexRow++;

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
                cellParticipant.SetCellValue(
                                                trStudentPoint.Any()
                                                ? trStudentPoint.Select(e => e.AcademicYear).FirstOrDefault()
                                                : academicYear
                                            );
                indexcell++;
                indexRow++;

                indexcell = 0;
                rowHeader = excelSheet.CreateRow(indexRow);
                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue("Semester");
                indexcell++;

                cellParticipant = rowHeader.CreateCell(indexcell);
                cellParticipant.SetCellValue(
                                                trStudentPoint.Any()
                                                ? trStudentPoint.Select(e => e.Semester).FirstOrDefault()
                                                : param.Semester.ToString()
                                            );

                indexRow += 2;
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
                foreach (var itemData in trStudentPoint)
                {
                    indexcell = 0;
                    rowHeader = excelSheet.CreateRow(indexRow);
                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.AcademicYear);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Semester);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Level);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Grade);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Homeroom);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.IdStudent);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.NameStudent);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Merit);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Demerit);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.LevelOfInfraction);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.Sanction);
                    cellParticipant.CellStyle = bodyStyle;
                    indexcell++;

                    cellParticipant = rowHeader.CreateCell(indexcell);
                    cellParticipant.SetCellValue(itemData.LastUpdate == null ? "" : itemData.LastUpdate.Date == DateTime.MinValue.Date ? string.Empty : Convert.ToDateTime(itemData.LastUpdate).ToString("dd MMM yyyy | hh:mm"));
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
