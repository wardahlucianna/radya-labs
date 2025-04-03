using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.Helper;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class UploadTransferEvidanceDocumentHandler : FunctionsHttpSingleHandler
    {
        private static readonly IDictionary<string, string[]> _allowedExtensions = new Dictionary<string, string[]>()
        {
            { "image", new[]{".png", ".jpg", ".jpeg"} },
            { "document", new[]{".pdf" } }
        };

        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        public UploadTransferEvidanceDocumentHandler(
            IConfiguration configuration,
            IMachineDateTime dateTime)
        {
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBodyForm<UploadTransferEvidanceDocumentRequest, UploadTransferEvidanceDocumentValidator>();

            try
            {
                var uploadFileHelper = new PaymentEvidanceDocumentBlobHelper(_dateTime, _configuration);
                
                var uploadedFileResult = new UploadFileHelperResult();

                var file = Request.Form.Files.FirstOrDefault();

                if (file is null || file.Length == 0)
                    throw new BadRequestException("Document file not found");

                var fileInfo = new FileInfo(file.FileName);
                if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension) && !_allowedExtensions["document"].Any(x => x == fileInfo.Extension))
                    throw new BadRequestException($"File extenstion is not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"]) + ", " + string.Join(", ", _allowedExtensions["document"])}");

                uploadedFileResult = await uploadFileHelper.UploadFileAsync(param.IdStudent, file);

                var result = new UploadTransferEvidanceDocumentResult
                {
                    IsSuccess = true,
                    FileName = uploadedFileResult.UploadedFileName,
                    OriginalFileName = file.FileName,
                    FileExtension = fileInfo.Extension,
                    FileSize = file.Length,
                    FileUrl = uploadedFileResult.UploadedFileUrl
                };

                return Request.CreateApiResult2(result as object);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
