using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionStudentStatusSpecialHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetStudentUnderAttentionStudentStatusSpecialHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var studentStatusSpecials = await _context.Entity<LtStudentStatusSpecial>()
                .ToListAsync(CancellationToken);

            var response = new List<GetStudentUnderAttentionStudentStatusSpecialResponse>();

            response = studentStatusSpecials
                .Select(a => new GetStudentUnderAttentionStudentStatusSpecialResponse
                {
                    SpecialStudentStatus = new ItemValueVm
                    {
                        Id = a.IdStudentStatusSpecial.ToString(),
                        Description = a.LongDesc,
                    },
                    Remarks = a.Remarks,
                }).ToList();

            return Request.CreateApiResult2(response as object);
        }
    }
}
