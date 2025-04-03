using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Parent
{
    public class GetParentGenerateAccountHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetParentGenerateAccountHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;           
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetParentGenerateAccountRequest>(nameof(GetParentGenerateAccountRequest.IdStudent));

            var studentParents = await _dbContext.Entity<MsStudentParent>()
                                .Include(x => x.Parent)                                
                                .Where(x => x.IdStudent == param.IdStudent)
                                .Where(x => x.Parent.AliveStatus == 1)
                                .Select(a => new {
                                    a.Parent.Id,
                                    a.Student.PersonalEmailAddress,
                                    DisplayName = (string.IsNullOrEmpty(a.Parent.FirstName.Trim()) ? "" : a.Parent.FirstName.Trim()) + (string.IsNullOrEmpty(a.Parent.LastName.Trim()) ? "" : (" " + a.Parent.LastName.Trim())),
                                    a.Parent.IdParentRole                                  
                                })
                                .ToListAsync();

                           
            return Request.CreateApiResult2(studentParents.Where(x => x.PersonalEmailAddress != null || x.PersonalEmailAddress.Trim() != "")
                                        .OrderByDescending(a => a.IdParentRole)
                                        .Select(a => new GetParentGenerateAccountResult(){
                                            IdBinusian = a.Id,
                                            DisplayName = a.DisplayName,
                                            Email = a.PersonalEmailAddress,
                                            Role = a.IdParentRole
                                        }).First() as object);                       
           
        }
    }
}
