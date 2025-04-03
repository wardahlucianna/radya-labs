using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using BinusSchool.Workflow.FnWorkflow.ApprovalHistory.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory
{
    public class UpdateTaskHistoryhandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public UpdateTaskHistoryhandler(IWorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateTaskHistoryRequest, UpdateTaskhistoryValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getdata = await _dbContext.Entity<MsApprovalHistory>().Where(p => p.Id == body.Id).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SchApprovalHistory"], "Id", body.Id));

                getdata.UserUp = body.UserID;
                getdata.Action = body.Action;

                _dbContext.Entity<MsApprovalHistory>().Update(getdata);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

        }
    }
}
