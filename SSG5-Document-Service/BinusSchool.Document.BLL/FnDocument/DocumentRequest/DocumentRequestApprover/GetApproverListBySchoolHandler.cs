using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class GetApproverListBySchoolHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public GetApproverListBySchoolHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetApproverListBySchoolRequest>(
                            nameof(GetApproverListBySchoolRequest.IdSchool));

            var query = _dbContext.Entity<MsDocumentReqApprover>()
                            .Include(x => x.Staff)
                            .Where(x => x.IdSchool == param.IdSchool);

            var listApprover = query
                                .Where(x => (string.IsNullOrEmpty(param.IdBinusian) ? true : (x.IdBinusian == param.IdBinusian)))
                                .Join(_dbContext.Entity<MsUser>(),
                                        approver => approver.UserIn,
                                        user => user.Id,
                                        (approver, user) => new GetApproverListBySchoolResult
                                        {
                                            IdDocumentReqApprover = approver.Id,
                                            Approver = new NameValueVm
                                            {
                                                Id = approver.IdBinusian,
                                                Name = NameUtil.GenerateFullName(approver.Staff.FirstName, approver.Staff.LastName)
                                            },
                                            BinusianEmail = approver.Staff.BinusianEmailAddress,
                                            InsertedBy = new NameValueVm
                                            {
                                                Id = approver.UserIn,
                                                Name = user.DisplayName
                                            },
                                            InsertedTime = approver.DateIn,
                                            CanRemove = query.Count() <= 1 ? false : true
                                        })
                                .ToList();

            return Request.CreateApiResult2(listApprover as object);
        }
    }
}
