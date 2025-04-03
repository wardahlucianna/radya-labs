using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using System.Text.RegularExpressions;
using BinusSchool.Domain.Extensions;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class GetListStudentByAySmtLvlGrdHrmHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[] { "idStudent", "name", "homeroom" });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "idStudent" },
            { _columns.Value[1], "name" },
            { _columns.Value[2], "homeroom" }
        });

        private readonly IStudentDbContext _dbContext;
        public GetListStudentByAySmtLvlGrdHrmHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListStudentByAySmtLvlGrdHrmRequest>(
                    nameof(GetListStudentByAySmtLvlGrdHrmRequest.IdAcademicYear),
                    nameof(GetListStudentByAySmtLvlGrdHrmRequest.Semester)
                    );

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);
            if (param.Semester != 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);

            param.OrderBy ??= _columns.Value[1];

            var query = _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Student)
                                .Include(x => x.Homeroom).
                                    ThenInclude(x => x.Grade).
                                    ThenInclude(x => x.MsLevel).
                                    ThenInclude(x => x.MsAcademicYear).
                                    ThenInclude(x => x.MsSchool)
               .Where(predicate)
               .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;
            var count = 0;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(mH => new ItemValueVm()
                    {
                        Id = mH.IdStudent,
                        Description = NameUtil.GenerateFullName(mH.Student.FirstName, mH.Student.MiddleName, mH.Student.LastName)
                    })
                    .ToListAsync(CancellationToken);

                count = items.Count();
            }
            else
            {
                var results = await query
                    .If(string.IsNullOrWhiteSpace(param.Search), x => x.SetPagination(param))
                    .GroupBy(x => new
                    {
                        x.IdStudent,
                        x.Student.FirstName,
                        x.Student.MiddleName,
                        x.Student.LastName
                    })
                    //.If(string.IsNullOrWhiteSpace(param.Search), x => x.SetPagination(param))
                    .Select(x => new GetListStudentByAySmtLvlGrdHrmResult
                    {
                        Id = x.Key.IdStudent,
                        Student = new NameValueVm
                        {
                            Id = x.Key.IdStudent,
                            Name = Regex.Replace(NameUtil.GenerateFullName(x.Key.FirstName, x.Key.MiddleName, x.Key.LastName), @"\s+", " ")
                        }
                    }).ToListAsync(CancellationToken);

                var datas = new List<GetListStudentByAySmtLvlGrdHrmResult>();

                if (!string.IsNullOrWhiteSpace(param.Search))
                    datas = results
                        .Where(x => x.Student.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                    || x.Student.Id.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                    || $"{x.Student.Id} {x.Student.Name}".Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                        .SetPagination(param)
                        .ToList();
                else
                    datas = results;

                var studentId = datas.Select(x => x.Student.Id).ToList();

                foreach (var item in datas)
                {
                    item.Homeroom = query
                        .Where(x => x.IdStudent == item.Student.Id)
                        .Select(x => new ItemValueVm
                        {
                            Id = x.IdHomeroom,
                            Description = Regex.Replace(x.Homeroom.Grade.Description + " " + x.Homeroom.MsGradePathwayClassroom.Classroom.Code, @"\s+", " ")
                        })
                        .FirstOrDefault();
                }

                items = datas;

                count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : string.IsNullOrWhiteSpace(param.Search) ? await query.GroupBy(x => new
                {
                    x.IdStudent,
                    x.Student.FirstName,
                    x.Student.MiddleName,
                    x.Student.LastName
                }).Select(x => x.Key).CountAsync(CancellationToken) : results.Where(x => x.Student.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                   || x.Student.Id.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                   || $"{x.Student.Id} {x.Student.Name}".Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                .Count();
            }


            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
