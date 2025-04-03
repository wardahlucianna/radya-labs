using System;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Util.FnConverter.HtmlToPdf;
using BinusSchool.Util.FnConverter.HtmlToPdf.Validator;
using Microsoft.AspNetCore.Mvc;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.HtmlToPdf
{
    public class ConvertHtmlToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;

        public ConvertHtmlToPdfHandler(IConverter converter)
        {
            _converter = converter;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var body = await Request.ValidateBody<ConvertHtmlToPdfRequest, ConvertHtmlToPdfValidator>();
            
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Outline = false
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = body.HtmlString,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        // HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
            };
            var bytes = _converter.Convert(doc);

            return new FileContentResult(bytes, "application/pdf")
            {
                FileDownloadName = body.FileName
            };
        }
    }
}