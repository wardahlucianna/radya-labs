using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class ExportMasterSeachingDataTableToExcelHandler : FunctionsHttpSingleHandler
    {

        private readonly IConfiguration _configuration;
        private readonly GetMasterSearchingDataTableHandler _getMasterSearchingDataTableHandler;

        public ExportMasterSeachingDataTableToExcelHandler(
            IConfiguration configuration,
            GetMasterSearchingDataTableHandler getMasterSearchingDataTableHandler)
        {
            _configuration = configuration;
            _getMasterSearchingDataTableHandler = getMasterSearchingDataTableHandler;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<ExportMasterSeachingDataTableToExcelRequest>();

            var GetMasterSearchingDataTable = await _getMasterSearchingDataTableHandler.GetMasterSearchingDataTable(new GetMasterSearchingDataTableRequest
            {
                IdSchool = param.IdSchool,
                SchoolName = param.SchoolName,
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                IdGrade = param.IdGrade,
                IdHomeroom = param.IdHomeroom,
                IdLevel = param.IdLevel,
                IdStudentStatus = param.IdStudentStatus,
                FieldData = param.FieldData,
                SearchByFieldData = param.SearchByFieldData,
                Keyword = param.Keyword
            });

            if (GetMasterSearchingDataTable != null)
            {
                var generateExcelByte = GenerateExcel(GetMasterSearchingDataTable.Result_Head, GetMasterSearchingDataTable.Result_Body, param);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcelByte = GenerateBlankExcel(GetMasterSearchingDataTable.Result_Head);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                };
            }
        }

        public byte[] GenerateBlankExcel(List<string> head)
        {
            var result = new byte[0];

            return result;
        }

        public byte[] GenerateExcel(List<string> head, List<GetMasterSearchingDataTableResult_BodyVm> body, ExportMasterSeachingDataTableToExcelRequest param)
        {
            var result = new byte[0];
            //string[] fieldDataList = head;

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Master Searching Data");

                //Create style for header
                ICellStyle headerStyle = workbook.CreateCellStyle();

                //Set border style 
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                headerStyle.SetFont(font);

                //Create style for header
                ICellStyle dataStyle = workbook.CreateCellStyle();

                //Set border style 
                dataStyle.BorderBottom = BorderStyle.Thin;
                dataStyle.BorderLeft = BorderStyle.Thin;
                dataStyle.BorderRight = BorderStyle.Thin;
                dataStyle.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont Datafont = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                Datafont.FontName = "Arial";
                Datafont.FontHeightInPoints = 12;
                dataStyle.SetFont(Datafont);

                //header 
                IRow row = excelSheet.CreateRow(0);


                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                foreach (string field in head)
                {
                    var Judul = row.CreateCell(fieldCount);
                    Judul.SetCellValue(field);
                    row.Cells[fieldCount].CellStyle = headerStyle;
                    fieldCount++;
                }

                int w = 0;
                foreach (var item in body)
                {
                    row = excelSheet.CreateRow(row.RowNum + 1);

                    for (int i = 0; i < item.FieldData.Count(); i++)
                    {
                        if (head[i].ToLower() == "photo")
                        {
                            if (item.IsPhotoExist == true)
                            {
                                try
                                {
                                    byte[] file = DownloadFromBlob(item.FilePathPhoto);
                                    row.CreateCell(i);
                                    int pictureIndex = workbook.AddPicture(file, PictureType.PNG);
                                    ICreationHelper helper = workbook.GetCreationHelper();
                                    IDrawing drawing = excelSheet.CreateDrawingPatriarch();
                                    IClientAnchor anchor = helper.CreateClientAnchor();
                                    anchor.Col1 = i;
                                    anchor.Row1 = row.RowNum;

                                    int imgWid = 100;
                                    int imgHgt = 75;
                                    excelSheet.SetColumnWidth(i, imgWid * 32);
                                    row.Height = (short)(imgHgt * 16);

                                    IPicture picture = drawing.CreatePicture(anchor, pictureIndex);

                                    picture.Resize(1);
                                }
                                catch (Exception)
                                {
                                    row.CreateCell(i);
                                    row.Cells[i].CellStyle = dataStyle;
                                }
                            }
                            else
                            {
                                row.CreateCell(i);
                                row.Cells[i].CellStyle = dataStyle;
                            }
                        }
                        else
                        {
                            var value = string.IsNullOrEmpty(item.FieldData[i]) ? "-" : item.FieldData[i];
                            row.CreateCell(i).SetCellValue(value);
                            row.Cells[i].CellStyle = dataStyle;
                            //row.CreateCell(i).SetCellValue(item.FieldData[i]);
                        }
                    }
                }

                //Add Auto FIt
                for (int i = 1; i < head.Count(); i++)
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

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadFromBlob(string FilePathPhoto)
        {

            CloudStorageAccount mycloudStorageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AccountStorage"));
            var blobClient = mycloudStorageAccount.CreateCloudBlobClient();

            var result = FilePathPhoto.Substring(FilePathPhoto.LastIndexOf("studentphoto") + 13);

            CloudBlobContainer container = blobClient.GetContainerReference("studentphoto");
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(result);

            cloudBlockBlob.FetchAttributes();
            long fileByteLength = cloudBlockBlob.Properties.Length;
            byte[] fileContent = new byte[fileByteLength];
            for (int i = 0; i < fileByteLength; i++)
            {
                fileContent[i] = 0x20;
            }

            cloudBlockBlob.DownloadToByteArray(fileContent, 0);

            return fileContent;
        }

    }
}
