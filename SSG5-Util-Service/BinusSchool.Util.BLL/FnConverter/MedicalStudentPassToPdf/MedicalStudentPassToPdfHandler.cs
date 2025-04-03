using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Util.FnConverter.MedicalStudentPassToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.MedicalStudentPassToPdf
{
    public class MedicalStudentPassToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _date;

        private const string _container = "medical-student-pass";

        public MedicalStudentPassToPdfHandler(IConverter converter, IConfiguration configuration, IMachineDateTime date)
        {
            _converter = converter;
            _configuration = configuration;
            _date = date;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = await Request.GetBody<MedicalStudentPassToPdfRequest>();

            var response = await MedicalStudentPassToPdf(request);

            return new JsonResult(response, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
        }

        public async Task<MedicalStudentPassToPdfResponse> MedicalStudentPassToPdf(MedicalStudentPassToPdfRequest request)
        {
            var response = new MedicalStudentPassToPdfResponse();

            // Convert String Html to Pdf Logic
            var pdfBytes = ConvertHtmlToPdf(request.Html);

            // Store Pdf file to Blob Azure Storage
            var blobUrl = await UploadPdfToBlobStorage(pdfBytes);

            response.Url = blobUrl;

            return response;
        }

        private byte[] ConvertHtmlToPdf(string html)
        {
            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Grayscale,
                    Orientation = Orientation.Portrait,
                    PaperSize = new PechkinPaperSize("80", "210"),
                    Margins = new MarginSettings
                    {
                        Top = 5,
                        Bottom = 5,
                        Left = 5,
                        Right = 5
                    }
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                    }
                }
            };

            return _converter.Convert(pdfDoc);
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            var connectionString = _configuration.GetConnectionString("Util:AccountStorage")
                                   ?? _configuration["ConnectionStrings:Util:AccountStorage"];

            return CloudStorageAccount.Parse(connectionString);
        }

        private async Task CreateContainerIfNotExistsAsync()
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_container);

            await blobContainer.CreateIfNotExistsAsync();
        }

        private async Task<string> UploadPdfToBlobStorage(byte[] pdfBytes)
        {
            await CreateContainerIfNotExistsAsync();

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_container);

            string uploadFileName = _date.ServerTime.ToString("yyyyMMdd_HHmmss") + ".pdf";

            CloudBlockBlob blob = container.GetBlockBlobReference(uploadFileName);

            await blob.UploadFromByteArrayAsync(pdfBytes, 0, pdfBytes.Length);

            // set content_type
            blob.Properties.ContentType = "application/pdf";
            blob.Properties.CacheControl = "max-age=5";

            await blob.SetPropertiesAsync();

            return blob.Uri.ToString();
        }

    }
}
