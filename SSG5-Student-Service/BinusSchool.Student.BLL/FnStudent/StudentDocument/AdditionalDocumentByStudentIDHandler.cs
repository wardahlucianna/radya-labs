using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDocument
{
    public class AdditionalDocumentByStudentIDHandler : FunctionsHttpCrudHandler
    {

        private readonly IStudentDbContext _dbContext;
        public AdditionalDocumentByStudentIDHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var param = Request.GetParams<AdditionalDocumentByStudentIDRequest>();

            if (param.Role != null && param.Role.Equals("student", StringComparison.CurrentCultureIgnoreCase))
            {

                var query = _dbContext.Entity<TrStudentDocument>()
                            .Include(x => x.Document)
                            .Where(x => x.Document.IdDocumentType != "1" && x.IdStudent == id && x.isStudentView == true)
                            .SearchByDynamic(param)
                            .Select(
                                        x => new GetDocumentByStudentResult
                                        {
                                            DocumentStudentID = x.Id,
                                            DocumentID = x.Document.Id,
                                            DocumentName = x.Document.DocumentName,
                                            DocumentType = x.Document.DocumentType.DocumentTypeName,
                                            FileSize = x.FileSize,
                                            FileName = x.FileName,
                                            IsStudentView = x.isStudentView,
                                            LastModified = (x.DateUp != null ? Convert.ToDateTime(x.DateUp) : Convert.ToDateTime(x.DateIn)),
                                            ModifiedBy = (x.UserUp != null ? x.UserUp : x.UserIn),
                                            Comment = x.Comment
                                        }
                                    );

                var items = await query.SetPagination(param).ToListAsync(CancellationToken);

                var count = query.Select(x => x.DocumentStudentID).Count();

                return Request.CreateApiResult2(query as object, param.CreatePaginationProperty(count));
            }
            else
            {
                var query = _dbContext.Entity<TrStudentDocument>()
                            .Include(x => x.Document)
                            .Where(x => x.Document.IdDocumentType != "1" && x.IdStudent == id)
                            .SearchByDynamic(param)
                            .Select(
                                        x => new GetDocumentByStudentResult
                                        {
                                            DocumentStudentID = x.Id,
                                            DocumentID = x.Document.Id,
                                            DocumentName = x.Document.DocumentName,
                                            DocumentType = x.Document.DocumentType.DocumentTypeName,
                                            FileSize = x.FileSize,
                                            FileName = x.FileName,
                                            IsStudentView = x.isStudentView,
                                            LastModified = (x.DateUp != null ? Convert.ToDateTime(x.DateUp) : Convert.ToDateTime(x.DateIn)),
                                            ModifiedBy = (x.UserUp != null ? x.UserUp : x.UserIn),
                                            Comment = x.Comment
                                        }
                                    );

                var items = await query.SetPagination(param).ToListAsync(CancellationToken);

                var count = query.Select(x => x.DocumentStudentID).Count();

                return Request.CreateApiResult2(query as object, param.CreatePaginationProperty(count));
            }
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
