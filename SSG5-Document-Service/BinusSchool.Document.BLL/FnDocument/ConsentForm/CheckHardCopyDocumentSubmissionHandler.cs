using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.ConsentForm;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;

namespace BinusSchool.Document.FnDocument.ConsentForm
{
    public class CheckHardCopyDocumentSubmissionHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public CheckHardCopyDocumentSubmissionHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CheckHardCopyDocumentSubmissionRequest>(
                        nameof(CheckHardCopyDocumentSubmissionRequest.IdSchool),
                        nameof(CheckHardCopyDocumentSubmissionRequest.IdAcademicYear),
                        nameof(CheckHardCopyDocumentSubmissionRequest.Semester),
                        nameof(CheckHardCopyDocumentSubmissionRequest.IdStudent));

            var result = new CheckHardCopyDocumentSubmissionResult();

            var isNeedHardCopy = _dbContext.Entity<MsBLPSetting>()
                                    .Where(x => x.IdSchool == param.IdSchool)
                                    .Select(x => x.NeedHardCopy)
                                    .FirstOrDefault();

            if (!isNeedHardCopy)
            {
                result = new CheckHardCopyDocumentSubmissionResult
                {
                    IsDone = true,
                    NeedHardCopy = isNeedHardCopy,
                    HardCopySubmissionDate = null
                };
            }

            // Check hardcopy submission date
            var getSubmissionDate = _dbContext.Entity<TrBLPGroupStudent>()
                                        .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                    x.IdStudent == param.IdStudent &&
                                                    x.Semester == param.Semester)
                                        .Select(x => x.HardCopySubmissionDate)
                                        .FirstOrDefault();

            result = new CheckHardCopyDocumentSubmissionResult
            {
                IsDone = getSubmissionDate == null ? false : true,
                NeedHardCopy = isNeedHardCopy,
                HardCopySubmissionDate = getSubmissionDate
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
