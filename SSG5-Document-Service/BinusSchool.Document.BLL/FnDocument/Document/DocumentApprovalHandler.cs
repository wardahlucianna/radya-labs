using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Api.Workflow.FnWorkflow;
using BinusSchool.Data.Model.Document.FnDocument.Document;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Document.FnDocument.Document.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.Document
{
    public class DocumentApprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMetadata _metadataService;
        // private readonly IRole _roleService;
        private readonly IUser _userService;
        private readonly IApproval _approvalServices;
        private readonly IApprovalHistory _approvalHistoryServices;
        private IDbContextTransaction _transaction;

        public DocumentApprovalHandler(IDocumentDbContext dbContext,
            IMetadata metadataService,
            IUser userService,
            IApproval approvalServices,
            IApprovalHistory approvalHistoryServices)
        {
            _dbContext = dbContext;
            _metadataService = metadataService;
            _userService = userService;
            _approvalServices = approvalServices;
            _approvalHistoryServices = approvalHistoryServices;
            
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var model = await Request.ValidateBody<ApprovalDocumentRequest, DocumentApprovalValidation>();

            if (model.Action != ApprovalAction.Approved && model.Action != ApprovalAction.Reject)
                throw new NotFoundException(message: "Action for this Proccess Not Match", Localizer);

            var document = await _dbContext.Entity<MsFormDoc>()
                                     .Include(p => p.Form)
                                     .Include(p => p.Form).ThenInclude(p => p.DocCategory)
                                     .FirstOrDefaultAsync(p => p.Id == model.IdDocuemnt);

            if (document == null) throw new NotFoundException(message: "Data Not Found", Localizer);

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            //masuk mekanisme approval send notifikasi dan submit ke approval task
            //1.get approval dari approval transition
            var req = new GetInApproveRequest
            {
                IdFromState = model.IdFormState,
                Action = (int)model.Action
            };
            var getInApprove = await _approvalServices.GetInApprove(req);

            if (!getInApprove.IsSuccess)
                throw new BadRequestException(getInApprove.Message);

            //var getInApprove = await _dbContext.Entity<ApprovalTransition>()
            //                                .Include(p => p.FromState).ThenInclude(p => p.FromStateApprovalTransition)
            //                                .Include(p => p.ToState).ThenInclude(p => p.ToStateApprovalTransition)
            //                                .Where(p => p.Action == model.Action && p.IdFromState == model.IdFormState)
            //                                .FirstOrDefaultAsync();

            if (getInApprove.Payload.ToStateType != StateTypeConstant.End && !string.IsNullOrWhiteSpace(getInApprove.Payload.ToStateIdRoleAction))
            {

                var getUser = await _userService.GetUserByRoleAndSchoolWithoutValidateStaff(new GetUserBySchoolAndRoleRequest
                {
                    IdRole = getInApprove.Payload.ToStateIdRoleAction,
                    IdSchool = document.Form.IdSchool,
                });

                if (!getUser.IsSuccess)
                    throw new BadRequestException(getUser.Message);


                //kalo semislakan setelah aktion approve atau reject masih ada approval selanjutnya atau action selanjutnya 
                //maka akan di assign ke user yg berlaku setelah nya
                if (getUser.Payload.Any())
                {
                    foreach (var item in getUser.Payload)
                    {
                        var addHistoryApproval = await _approvalHistoryServices.AddApprovalHistory(new AddApprovalHistoryRequest
                        {
                            IdUserAction = item.Id,
                            IdFormState = getInApprove.Payload.IdToState,
                            IdDocument = document.Id,
                            Action = ApprovalStatus.Draft,
                        });

                        if (!addHistoryApproval.IsSuccess)
                            throw new BadRequestException(addHistoryApproval.Message);
                    }

                }

                //add notification for send email adn signalR
            }

            document.Status = getInApprove.Payload.Status;
            document.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsFormDoc>().Update(document);

            //update task approval
            var UpdateHistory = await _approvalHistoryServices.UpdateTaskApprovalHistory(new UpdateTaskHistoryRequest
            {
                Id = model.IdApprovalTask,
                Action = model.Action == ApprovalAction.Approved ? ApprovalStatus.Approved : ApprovalStatus.Reject,
                UserID = AuthInfo.UserId,
            });

            if (!UpdateHistory.IsSuccess)
                throw new BadRequestException(UpdateHistory.Message);

            //jika user malukan approve docuemnt
            if (model.Action == ApprovalAction.Approved)
            {
                //Send email dengan format email approve
                //dan spesial case ketika aproove


                //kalo masih ada approval selanjutnya maka add dahulu yg menandakan bahwa user yg approve sudah melakukan approve
                //await AddToHistoryApprovalTable(getInApprove.Payload, document, model, ApprovalStatus.Approved);

                if (getInApprove.Payload.ToStateType == StateTypeConstant.Process && getInApprove.Payload.Status!= ApprovalStatus.Approved)
                {
                    await AddToHistoryApprovalTable(getInApprove.Payload, document, model,null,true,1);
                    await AddToHistoryApprovalTable(getInApprove.Payload, document, model, ApprovalStatus.Approved, false,0);
                }
                else
                {
                    await AddToHistoryApprovalTable(getInApprove.Payload, document, model,null,false,0);
                }
            }
            //jika user malukan reject docuemnt
            else if (model.Action == ApprovalAction.Reject)
            {
                //Send email dengan format email Reject
                //dan spesial case ketika aproove
                await AddToHistoryApprovalTable(getInApprove.Payload, document, model,null,false,0);
            }


            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        /// <summary>
        /// class untuk add history 
        /// </summary>
        /// <param name="inApproveModel"></param>
        /// <param name="document"></param>
        /// <param name="model"></param>
        /// <param name="isHaveNexstep"></param>
        /// <returns></returns>
        private async Task AddToHistoryApprovalTable(GetInApproveResult inApproveModel,
                                            MsFormDoc document,
                                            ApprovalDocumentRequest model,
                                            ApprovalStatus? status,
                                            bool isRole,
                                            int order) 
        {
            //add histori change table or approve
            var historyChange = new HMsFormDocChange();
            historyChange.Id = Guid.NewGuid().ToString();
            historyChange.IdFormDoc = document.Id;
            historyChange.JsonDocumentValueOld = document.JsonDocumentValue;
            historyChange.JsonFormElementOld = document.JsonFormElement;
            historyChange.JsonDocumentValueNew = document.JsonDocumentValue;
            historyChange.JsonFormElementNew = document.JsonFormElement;
            historyChange.Status = status.HasValue ? status.Value : inApproveModel.Status;
            historyChange.UserIn = AuthInfo.UserId;
            historyChange.Order = order;
            historyChange.IdUserExecutor = isRole ? inApproveModel.ToStateIdRoleAction : AuthInfo.UserId;


            if (inApproveModel.FromStateType == StateTypeConstant.Start)
            {
                var hisApprovalInHisChange = new HMsFormDocChangeApproval();
                hisApprovalInHisChange.Id = Guid.NewGuid().ToString();
                hisApprovalInHisChange.IdState = inApproveModel.IdFromState;
                hisApprovalInHisChange.IdUserActionBy = AuthInfo.UserId;
                hisApprovalInHisChange.IdRoleActionBy = inApproveModel.FromStateIdRoleAction;
                hisApprovalInHisChange.IdFormDocument = document.Id;
                hisApprovalInHisChange.Action = model.Action == ApprovalAction.Approved ? ApprovalStatus.Approved : ApprovalStatus.Reject;
                hisApprovalInHisChange.Comment = model.Comment;
                hisApprovalInHisChange.ActionDate = DateTimeUtil.ServerTime;
                hisApprovalInHisChange.IdForRoleActionNext = inApproveModel.ToStateIdRoleAction;
                hisApprovalInHisChange.UserIn = AuthInfo.UserId;
                hisApprovalInHisChange.IdsFormDocChange = historyChange.Id;
                _dbContext.Entity<HMsFormDocChangeApproval>().Add(hisApprovalInHisChange);
            }
            else
            {
                //ambil dari history sebelum nya dan masukan ke yg baru
                var getHisDocument = await _dbContext.Entity<HMsFormDocChange>()
                    .Include(p => p.DocChangeApprovals)
                    .OrderByDescending(p => p.DateIn)
                    .Where(p => p.IdFormDoc == document.Id)
                    .FirstOrDefaultAsync();

                if (getHisDocument.Status != ApprovalStatus.Reject)
                {
                    var listHisApprove = new List<HMsFormDocChangeApproval>();
                    foreach (var item in getHisDocument.DocChangeApprovals.OrderBy(p => p.ActionDate))
                    {
                        item.Id = Guid.NewGuid().ToString();
                        item.IdsFormDocChange = historyChange.Id;
                        listHisApprove.Add(item);
                    }

                    listHisApprove.Add(new HMsFormDocChangeApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdState = inApproveModel.IdFromState,
                        IdUserActionBy = AuthInfo.UserId,
                        IdRoleActionBy = inApproveModel.FromStateIdRoleAction,
                        IdFormDocument = document.Id,
                        Action = model.Action == ApprovalAction.Approved ? ApprovalStatus.Approved : ApprovalStatus.Reject,
                        ActionDate = DateTimeUtil.ServerTime,
                        Comment = model.Comment,
                        IdForRoleActionNext = inApproveModel.ToStateIdRoleAction,
                        UserIn = AuthInfo.UserId,
                        IdsFormDocChange = historyChange.Id,
                    });

                    _dbContext.Entity<HMsFormDocChangeApproval>().AddRange(listHisApprove);
                }
            }

            _dbContext.Entity<HMsFormDocChange>().Add(historyChange);
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
    }
}
