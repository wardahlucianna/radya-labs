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
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetListEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListEvidencesHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListEvidencesRequest>();

            var predicate = PredicateBuilder.Create<TrEvidences>(x => x.IdExperience == param.IdExperience);

            var query = _dbContext.Entity<TrEvidences>()
                .Include(x => x.TrEvidenceLearnings).ThenInclude(x => x.LearningOutcome)
                .Include(x => x.TrEvidencesAttachments)
                .Where(predicate)
                .Select(x => new
                {
                    Id = x.Id,
                    IdExperience = x.IdExperience,
                    DateIn = x.DateIn,
                    EvidencesType = x.EvidencesType,
                    EvidencesText = x.EvidencesValue,
                    EvidencesLink = x.Url,
                    Attachments = x.TrEvidencesAttachments != null ? x.TrEvidencesAttachments.Select(y => new ListEvidencesAttachment
                        {
                            Url = y.Url,
                            Filename = y.File,
                            Filetype = y.FileType,
                            Filesize = y.Size
                        }).ToList() : null,
                    LearningOutcome = x.TrEvidenceLearnings != null ? x.TrEvidenceLearnings.Select(y => new CodeWithIdVm
                        {
                            Id = y.IdLearningOutcome,
                            Code = y.IdLearningOutcome,
                            Description = y.LearningOutcome.LearningOutcomeName
                        }).ToList() : null,
                    CanEdit = param.Role == "STUDENT",
                    CanDelete = param.Role == "STUDENT"
                })
                .OrderByDescending(x => x.DateIn);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);


                items = result.Select(x => new GetListEvidencesResult
                {
                    Id = x.Id,
                    IdExperience = x.IdExperience,
                    DateIn = x.DateIn,
                    EvidencesType = x.EvidencesType,
                    EvidencesText = x.EvidencesText,
                    EvidencesLink = x.EvidencesLink,
                    Attachments = x.Attachments,
                    LearningOutcomes = x.LearningOutcome,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetListEvidencesResult
                {
                    Id = x.Id,
                    IdExperience = x.IdExperience,
                    DateIn = x.DateIn,
                    EvidencesType = x.EvidencesType,
                    EvidencesText = x.EvidencesText,
                    EvidencesLink = x.EvidencesLink,
                    Attachments = x.Attachments,
                    LearningOutcomes = x.LearningOutcome,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
