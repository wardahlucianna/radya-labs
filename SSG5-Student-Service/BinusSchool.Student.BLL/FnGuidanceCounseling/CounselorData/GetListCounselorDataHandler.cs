using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnGuidanceCounseling.CounselorData.Validator;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData
{
    public class GetListCounselorDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListCounselorDataHandler(IStudentDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        private string GetGrade(string idCounselor, List<MsCounselorGrade> CounselorGrades)
        {
            var counselorGrade = CounselorGrades.Where(x => x.IdCounselor == idCounselor).ToList();
            if(counselorGrade == null) return "-";

            return string.Join(", ", counselorGrade.OrderBy(x=>x.Grade.OrderNumber).Select(x => x.Grade.Code));
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCounselorDataRequest>();
            string[] columns = { "AcademicYearCode", "Grades", "GCPhoto", "CounselorName", "OfficeLocation", "ExtentionNumber", "ConselorEmail", "OtherInformation" };

            var queryJoin = (from c in _dbContext.Entity<MsCounselor>()
                        join cp_ in _dbContext.Entity<MsCounselorPhoto>() on c.Id equals cp_.IdCounselor into _cp
                        from cp in _cp.DefaultIfEmpty()
                        join u in _dbContext.Entity<MsUser>() on c.IdUser equals u.Id
                        join ay in _dbContext.Entity<MsAcademicYear>() on c.IdAcademicYear equals ay.Id
                        join s in _dbContext.Entity<MsStaff>() on u.Id equals s.IdBinusian
                        where c.IdAcademicYear == param.IdAcadyear
                        select new {
                            Id = c.Id,
                            IdAcademicYear = c.IdAcademicYear,
                            AcademicYearCode = ay.Code,
                            Grades = "",
                            GCPhoto = cp.Url,
                            CounselorName = !string.IsNullOrEmpty(s.FirstName) ? s.FirstName == s.LastName ? s.FirstName : s.FirstName + " " + s.LastName : s.LastName,
                            OfficerLocation = c.OfficerLocation,
                            ExtensionNumber = c.ExtensionNumber,
                            CounselorEmail = u.Email,
                            OtherInformation = c.OtherInformation,
                            AcademicYear = new AcademicYearObject()
                            {
                                Id = ay.Id,
                                Code = ay.Code,
                                Description = ay.Description
                            },
                            DateIn = c.DateIn
                        }).AsQueryable();

            //filter
            // if (!string.IsNullOrEmpty(param.IdAcadyear))
            //     queryJoin = queryJoin.Where(x => x.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.Search))
            {
                queryJoin = queryJoin.Where(x => EF.Functions.Like(x.CounselorName, param.SearchPattern()));
            }

            //ordering
            queryJoin = queryJoin.OrderByDescending(x => x.DateIn);

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                switch (param.OrderBy.ToLower())
                {
                    case "academicyearcode":
                        queryJoin = param.OrderType == OrderType.Desc
                            ? queryJoin.OrderByDescending(x => x.AcademicYear.Code)
                            : queryJoin.OrderBy(x => x.AcademicYear.Code);
                        break;
                    case "counselorname":
                        queryJoin = param.OrderType == OrderType.Desc
                            ? queryJoin.OrderByDescending(x => x.CounselorName)
                            : queryJoin.OrderBy(x => x.CounselorName);
                        break;
                    case "officerlocation":
                        queryJoin = param.OrderType == OrderType.Desc
                            ? queryJoin.OrderByDescending(x => x.OfficerLocation)
                            : queryJoin.OrderBy(x => x.OfficerLocation);
                        break;
                    case "extensionnumber":
                        queryJoin = param.OrderType == OrderType.Desc
                            ? queryJoin.OrderByDescending(x => x.ExtensionNumber)
                            : queryJoin.OrderBy(x => x.ExtensionNumber);
                        break;
                    case "counseloremail":
                        queryJoin = param.OrderType == OrderType.Desc
                            ? queryJoin.OrderByDescending(x => x.CounselorEmail)
                            : queryJoin.OrderBy(x => x.CounselorEmail);
                        break;
                    case "OtherInformation":
                        queryJoin = param.OrderType == OrderType.Desc
                            ? queryJoin.OrderByDescending(x => x.OtherInformation)
                            : queryJoin.OrderBy(x => x.OtherInformation);
                        break;
                };
            }

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
            {
                var result = await queryJoin
                            .ToListAsync(CancellationToken);

                var councelorGrade = await _dbContext.Entity<MsCounselorGrade>().Include(x => x.Grade).Where(x => result.Select(y => y.Id).ToList().Contains(x.IdCounselor)).ToListAsync();

                items = result
                        .Select(x => new GetCounselorDataResult()
                        {
                            Id = x.Id,
                            AcademicYearCode = x.AcademicYearCode,
                            Grades = GetGrade(x.Id, councelorGrade),
                            GCPhoto = x.GCPhoto,
                            CounselorName = x.CounselorName,
                            OfficerLocation = x.OfficerLocation,
                            ExtensionNumber = x.ExtensionNumber,
                            CounselorEmail = x.CounselorEmail,
                            OtherInformation = x.OtherInformation,
                            AcademicYear = x.AcademicYear
                        })
                        // .OrderByDynamic(param)
                        .ToList();
            }

            else
            {
                var result = await queryJoin
                        .SetPagination(param)
                        .ToListAsync(CancellationToken);

                var councelorGrade = await _dbContext.Entity<MsCounselorGrade>().Include(x => x.Grade).Where(x => result.Select(y => y.Id).ToList().Contains(x.IdCounselor)).ToListAsync();

                items = result
                        .Select(x => new GetCounselorDataResult()
                        {
                            Id = x.Id,
                            AcademicYearCode = x.AcademicYearCode,
                            Grades = GetGrade(x.Id, councelorGrade),
                            GCPhoto = x.GCPhoto,
                            CounselorName = x.CounselorName,
                            OfficerLocation = x.OfficerLocation,
                            ExtensionNumber = x.ExtensionNumber,
                            CounselorEmail = x.CounselorEmail,
                            OtherInformation = x.OtherInformation,
                            AcademicYear = x.AcademicYear
                        })
                        // .OrderByDynamic(param)
                        .ToList();
            }
            
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryJoin.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
