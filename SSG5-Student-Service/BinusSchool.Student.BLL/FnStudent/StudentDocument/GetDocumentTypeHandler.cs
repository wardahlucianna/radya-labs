using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using BinusSchool.Domain.Extensions;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetDocumentTypeHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        public GetDocumentTypeHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = await Request.GetBody<CollectionRequest>();

            var query = await _dbContext.Entity<MsDocument>()
                        .Where(x => x.IdDocumentType == "2")
                        .SearchByDynamic(param)
                        .Select(
                            x => new GetDocumentTypeResult 
                                { 
                                    DocumentId = Convert.ToInt32(x.Id),
                                    DocumentTypeName = x.DocumentName
                                }
                                ).ToListAsync();

            return Request.CreateApiResult2(query as object);
        }
    }
}
