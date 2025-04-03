using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.File;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IFile : IFnDocument
    {
        [Multipart]
        [Post("/document/file/upload")]
        Task<ApiErrorResult<UploadFileResult>> UploadFile(StreamPart stream, string fileName);
        
        [Delete("/document/file/delete")]
        Task<ApiErrorResult> DeleteFile(string fileName);
    }
}