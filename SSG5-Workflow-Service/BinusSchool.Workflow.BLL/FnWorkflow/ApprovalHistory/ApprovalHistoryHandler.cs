using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using BinusSchool.Workflow.FnWorkflow.ApprovalHistory.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory
{
    public class ApprovalHistoryHandler : FunctionsHttpCrudHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        public ApprovalHistoryHandler(IWorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddApprovalHistoryRequest, AddApprovalHistoryValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var build = new MsApprovalHistory
            {
                Id = Guid.NewGuid().ToString(),
                IdDocument = body.IdDocument,
                IdFormState = body.IdFormState,
                IdUserAction = body.IdUserAction,
                Action = body.Action,
            };

            _dbContext.Entity<MsApprovalHistory>().Add(build);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateApprovalHistoryRequest, UpdateApprovalHistoryValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var getdata = await _dbContext.Entity<MsApprovalHistory>().Where(p => p.Id == body.Id).FirstOrDefaultAsync();
            if (getdata is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SchApprovalHistory"], "Id", body.Id));

            getdata.IdDocument = body.IdDocument;
            getdata.IdFormState = body.IdFormState;
            getdata.IdUserAction = body.IdUserAction;
            getdata.Action = body.Action;
            _dbContext.Entity<MsApprovalHistory>().Update(getdata);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
