using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetStudentAYAndGradeHistoryListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "AcademicYear", "Grade" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetStudentAYAndGradeHistoryListHandler(
           IDocumentDbContext dbContext,
           IMachineDateTime dateTime
           )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAYAndGradeHistoryListRequest>(
                            nameof(GetStudentAYAndGradeHistoryListRequest.IdStudent));

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like((x.Homeroom.Grade.Description +       
                            x.Homeroom.GradePathwayClassroom.Classroom.Description + " (" +
                            x.Homeroom.Grade.Level.AcademicYear.Description + ")"
                    ), param.SearchPattern())
                    );

            var query = _dbContext.Entity<MsHomeroomStudent>()
                            .Include(x => x.Homeroom)
                                .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                                .ThenInclude(x => x.AcademicYear)
                            .Include(x => x.Homeroom)
                                .ThenInclude(x => x.GradePathwayClassroom)
                                .ThenInclude(x => x.Classroom)
                            .Where(x => x.IdStudent == param.IdStudent)
                            .Select(x => new
                            {
                                AcademicYear = new
                                {
                                    Id = x.Homeroom.Grade.Level.AcademicYear.Id,
                                    Description = x.Homeroom.Grade.Level.AcademicYear.Description,
                                    Code = x.Homeroom.Grade.Level.AcademicYear.Code
                                },
                                Grade = new
                                {
                                    Id = x.Homeroom.Grade.Id,
                                    Description = x.Homeroom.Grade.Description,
                                    Code = x.Homeroom.Grade.Code
                                },
                                HomeroomName = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Description
                            })
                            .Distinct()
                            .ToList();

            query = param.OrderBy switch
            {
                "AcademicYear" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.AcademicYear.Code).ToList()
                    : query.OrderByDescending(x => x.AcademicYear.Code).ToList(),
                "Grade" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Grade.Code).ToList()
                    : query.OrderByDescending(x => x.Grade.Code).ToList(),
                _ => query.OrderByDescending(x => x.AcademicYear.Code).ToList()
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Grade.Id,
                        Description = $"{x.HomeroomName} ({x.AcademicYear.Description})"
                    })
                    .ToList();
            else
                items = query
                    .SetPagination(param)
                    .Select(x => new GetStudentAYAndGradeHistoryListResult
                    {
                        AcademicYear = new ItemValueVm
                        {
                            Id = x.AcademicYear.Id,
                            Description = x.AcademicYear.Description
                        },
                        Grade = new ItemValueVm
                        {
                            Id = x.Grade.Id,
                            Description = x.Grade.Description
                        },
                        HomeroomName = x.HomeroomName
                    })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Grade.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
