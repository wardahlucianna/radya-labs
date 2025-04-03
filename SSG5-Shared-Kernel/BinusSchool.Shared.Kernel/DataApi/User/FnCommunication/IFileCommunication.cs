using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model;

namespace BinusSchool.Data.Api.User.FnCommunication
{
    public interface IFileCommunication : IFnCommunication
    {
        [Delete("/communication/file/delete")]
        Task<ApiErrorResult> DeleteFile([Body] FileRequest body);
    }
}
