using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMasterPortfolio : IFnStudent
    {
        [Get("/student/master-data-portfolio")]
        Task<ApiErrorResult<IEnumerable<GetMasterPortfolioResult>>> GetMasterDataPortfolio(GetMasterPortfolioRequest query);

        [Get("/student/master-data-portfolio/detail/{id}")]
        Task<ApiErrorResult<GetMasterPortfolioDetailResult>> GetMasterDataPortfolioDetail(string id);

        [Post("/student/master-data-portfolio")]
        Task<ApiErrorResult> AddMasterDataPortfolio([Body] AddMasterPortfolioRequest body);

        [Put("/student/master-data-portfolio")]
        Task<ApiErrorResult> UpdateMasterDataPortfolio([Body] UpdateMasterPortfolioRequest body);

        [Delete("/student/master-data-portfolio")]
        Task<ApiErrorResult> DeleteMasterDataPortfolio([Body] IEnumerable<string> ids);
    }
}
