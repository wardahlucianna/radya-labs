using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class GetListExemplaryCharacterSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListExemplaryCharacterSummaryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //private static readonly IDictionary<string, string[]> _fileTypes = new Dictionary<string, string[]>()
        //{
        //    { "image", new[]{ ".png", ".jpg", ".jpeg" } },
        //    { "video", new[]{ ".mkv", ".mp4", ".webm", ".mov", ".wmv" } }
        //};

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetListExemplaryCharacterSummaryRequest, GetListExemplaryCharacterSummaryValidator>();

            var columns = new[] { "exemplaryTitle", "academicYear", "description", "exemplaryDate", "likesCount" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Exemplary.Name" },
                { columns[1], "AcademicYear.Description" },
                { columns[2], "Exemplary.Description" },
                { columns[3], "Exemplary.ExemplaryDate" },
                { columns[4], "Exemplary.LikesCount" }
            };

            var predicate = PredicateBuilder.True<TrExemplary>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.AcademicYear.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Title, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    );

            var resultExamplary = await _dbContext.Entity<TrExemplary>()
                                   .Include(x => x.ExemplaryStudents)
                                        .ThenInclude(y => y.Student)
                                   .Include(x => x.ExemplaryStudents)
                                        .ThenInclude(y => y.Homeroom)
                                        .ThenInclude(y => y.MsGradePathwayClassroom)
                                        .ThenInclude(y => y.Classroom)
                                   .Include(x => x.ExemplaryStudents)
                                        .ThenInclude(y => y.Homeroom)
                                        .ThenInclude(y => y.Grade)
                                        .ThenInclude(y => y.MsLevel)
                                   .Include(x => x.ExemplaryAttachments)
                                   .Include(x => x.ExemplaryLikes)
                                   .Include(x => x.TrExemplaryValues)
                                        .ThenInclude(y => y.LtExemplaryValue)
                                   .Include(x => x.AcademicYear)
                                   .Where(predicate)
                                   .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                (param.IsPostedByMe == false ? true : x.UserIn == AuthInfo.UserId) &&
                                                (string.IsNullOrEmpty(param.IdGrade) ? true : x.ExemplaryStudents.Select(y => y.Homeroom.IdGrade).Any(y => y == param.IdGrade)) &&
                                                (param.StartDateExemplary == null ? true : x.ExemplaryDate.Value.Date >= param.StartDateExemplary.Value.Date) &&
                                                (param.EndDateExemplary == null ? true : x.ExemplaryDate.Value.Date <= param.EndDateExemplary.Value.Date) &&
                                                ((string.IsNullOrEmpty(param.SearchStudent) ? true : x.ExemplaryStudents.Select(y => y.IdStudent).Contains(param.SearchStudent)) ||
                                                (string.IsNullOrEmpty(param.SearchStudent) ? true : (
                                                    x.ExemplaryStudents.Select(y =>
                                                        (string.IsNullOrWhiteSpace(y.Student.FirstName) ? "" : (y.Student.FirstName.Trim() + " ")) +
                                                    (string.IsNullOrWhiteSpace(y.Student.MiddleName) ? "" : (y.Student.MiddleName.Trim() + " ")) +
                                                    (string.IsNullOrWhiteSpace(y.Student.LastName) ? "" : (y.Student.LastName.Trim()))
                                                    ).Contains(param.SearchStudent))
                                                )) &&
                                                (string.IsNullOrEmpty(param.IdExemplaryValue) ? true : x.TrExemplaryValues.Select(y => y.IdLtExemplaryValue).Any(y => y == param.IdExemplaryValue)) &&
                                                 (string.IsNullOrEmpty(param.IdHomeroom) ? true : x.ExemplaryStudents.Select(y => y.IdHomeroom).Any(y => y == param.IdHomeroom)) &&
                                                 (param.Semester == null ? true : x.ExemplaryStudents.Select(y => y.Homeroom.Semester).Any(y => y == param.Semester))
                                        )
                                   .Select(x => new GetListExemplaryCharacterSummaryResult_Exemplary
                                   {
                                       Exemplary = new NameValueVm
                                       {
                                           Id = x.Id,
                                           Name = x.LtExemplaryCategory.LongDesc
                                       },
                                       AcademicYear = new ItemValueVm
                                       {
                                           Id = x.AcademicYear.Id,
                                           Description = x.AcademicYear.Description
                                       },
                                       Value = x.TrExemplaryValues.Select(a => new ItemValueVm
                                       {
                                           Id = a.IdLtExemplaryValue,
                                           Description = a.LtExemplaryValue.LongDesc
                                       }).ToList(),
                                       Description = x.Description,
                                       ExemplaryDate = x.ExemplaryDate,
                                       LikesCount = x.ExemplaryLikes.Count(),
                                       PostedBy = new NameValueVm
                                       {
                                           Id = x.UserIn
                                       },
                                       ModifiedBy = new NameValueVm
                                       {
                                           Id = x.UserUp
                                       },
                                       PostedByMe = (AuthInfo.UserId == x.UserIn),
                                       IsLikedByMe = x.ExemplaryLikes.Select(a => a.UserIn).Any(a => a == AuthInfo.UserId),
                                       Attachment = x.ExemplaryAttachments.Select(a => new GetListExemplaryCharacterSummaryResult_Attachment
                                       {
                                           FileName = a.FileName,
                                           FileSize = a.FileSize,
                                           FileExtension = a.FileExtension,
                                           FileType = a.FileType,
                                           OriginalFileName = a.OriginalFileName,
                                           Url = a.Url
                                       }).ToList(),
                                       StudentList = x.ExemplaryStudents.Select(a => new GetListExemplaryCharacterSummaryResult_ExemplaryStudent
                                       {
                                           Grade = new ItemValueVm
                                           {
                                               Id = a.Homeroom.Grade.Id,
                                               Description = a.Homeroom.Grade.Description
                                           },
                                           Student = new NameValueVm
                                           {
                                               Id = a.Student.Id,
                                               Name = NameUtil.GenerateFullName(a.Student.FirstName, a.Student.MiddleName, a.Student.LastName)
                                           }
                                       }).ToList()
                                   })
                                   .ToListAsync();

            // fill posted by and modified by
            var userPostModifyList = await _dbContext.Entity<MsUser>()
                                        .Where(x => resultExamplary.Select(y => y.PostedBy.Id).Any(y => y == x.Id) ||
                                                    resultExamplary.Select(y => y.ModifiedBy.Id).Any(y => y == x.Id))
                                        .ToListAsync(CancellationToken);

            foreach (var exemplary in resultExamplary)
            {
                exemplary.PostedBy = string.IsNullOrEmpty(exemplary.PostedBy.Id) ? null :
                                        (
                                            userPostModifyList
                                            .Where(x => x.Id == exemplary.PostedBy.Id)
                                            .Select(x => new NameValueVm
                                            {
                                                Id = x.Id,
                                                Name = x.DisplayName
                                            })
                                            .FirstOrDefault()
                                        );

                exemplary.ModifiedBy = string.IsNullOrEmpty(exemplary.ModifiedBy.Id) ? null :
                                        (
                                            userPostModifyList
                                            .Where(x => x.Id == exemplary.ModifiedBy.Id)
                                            .Select(x => new NameValueVm
                                            {
                                                Id = x.Id,
                                                Name = x.DisplayName
                                            })
                                            .FirstOrDefault()
                                        );
            }

            #region unused code
            //var exemplaryCharacterStudentList = await _dbContext.Entity<TrExemplaryStudent>()
            //                                        .Include(es => es.Exemplary)
            //                                        .Include(es => es.Homeroom)
            //                                        .ThenInclude(h => h.Grade)
            //                                        .ThenInclude(g => g.MsLevel)
            //                                        .ThenInclude(l => l.MsAcademicYear)
            //                                        .Include(es => es.Student)
            //                                        .Where(predicateExemplary)
            //                                        .Where(x => x.Exemplary.IdAcademicYear == param.IdAcademicYear &&
            //                                                    (param.IsPostedByMe == false ? true : x.Exemplary.UserIn == AuthInfo.UserId) &&
            //                                                    (string.IsNullOrEmpty(param.IdGrade) ? true : x.Homeroom.Grade.Id == param.IdGrade) &&
            //                                                    (param.StartDateExemplary == null ? true : x.Exemplary.ExemplaryDate.Value.Date >= param.StartDateExemplary.Value.Date) &&
            //                                                    (param.EndDateExemplary == null ? true : x.Exemplary.ExemplaryDate.Value.Date <= param.EndDateExemplary.Value.Date) &&
            //                                                    ((string.IsNullOrEmpty(param.SearchStudent) ? true : x.IdStudent == param.SearchStudent) ||
            //                                                    (string.IsNullOrEmpty(param.SearchStudent) ? true : (
            //                                                        (string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : (x.Student.FirstName.Trim() + " ")) +
            //                                                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : (x.Student.MiddleName.Trim() + " ")) +
            //                                                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : (x.Student.LastName.Trim()))
            //                                                    ).Contains(param.SearchStudent))
            //                                                    ))
            //                                        .ToListAsync(CancellationToken);

            //var exemplaryCharacterRawList = exemplaryCharacterStudentList
            //                                    .Select(x => x.Exemplary)
            //                                        // left join userin
            //                                        .GroupJoin(
            //                                            _dbContext.Entity<MsUser>(),
            //                                            exemplary => exemplary.UserIn,
            //                                            joinUserIn => joinUserIn.Id,
            //                                            (exemplary, joinUserIn) => new { exemplary, joinUserIn }
            //                                            )
            //                                        .SelectMany(
            //                                           x => x.joinUserIn.DefaultIfEmpty(),
            //                                           (exemplary, joinUserIn) => new { exemplary, joinUserIn })

            //                                        // left join userup
            //                                        .GroupJoin(
            //                                            _dbContext.Entity<MsUser>(),
            //                                            exemplary => exemplary.exemplary.exemplary.UserUp,
            //                                            joinUserUp => joinUserUp.Id,
            //                                            (exemplary, joinUserUp) => new { exemplary, joinUserUp }
            //                                            )
            //                                        .SelectMany(
            //                                           x => x.joinUserUp.DefaultIfEmpty(),
            //                                           (exemplary, joinUserUp) => new { exemplary, joinUserUp })
            //                                    .ToList();

            //var exemplaryCharacterList = exemplaryCharacterRawList
            //                                .GroupBy(x => x.exemplary.exemplary.exemplary.exemplary.Id)
            //                                .Select(x => x.First())
            //                                .ToList();

            //exemplaryCharacterCount = exemplaryCharacterList.Count();

            //var exemplaryAttachmentRawList = await _dbContext.Entity<TrExemplaryAttachment>()
            //                                    .Where(x => exemplaryCharacterList.Select(y => y.exemplary.exemplary.exemplary.exemplary.Id).Any(y => y == x.IdExemplary))
            //                                    .ToListAsync(CancellationToken);

            //var exemplaryCategoryRawList = await _dbContext.Entity<TrExemplaryCategory>()
            //                                    .Include(ec => ec.LtExemplaryCategory)
            //                                    //.Where(predicateCategory)
            //                                    .Where(x => string.IsNullOrEmpty(param.IdExemplaryCategory) ? true : x.IdLtExemplaryCategory == param.IdExemplaryCategory)
            //                                   .Where(x => exemplaryCharacterList.Select(y => y.exemplary.exemplary.exemplary.exemplary.Id).Any(y => y == x.IdExemplary))
            //                                   .ToListAsync(CancellationToken);

            //var exemplaryLikesRawList = await _dbContext.Entity<TrExemplaryLikes>()
            //                                   .Where(x => exemplaryCharacterList.Select(y => y.exemplary.exemplary.exemplary.exemplary.Id).Any(y => y == x.IdExemplary))
            //                                   .ToListAsync(CancellationToken);

            //var resultExamplary = new List<GetListExemplaryCharacterSummaryResult_Exemplary>();

            //foreach (var exemplaryCharacter in exemplaryCharacterList)
            //{
            //    var resultExemplaryAttachment = exemplaryAttachmentRawList
            //                                    .Where(x => x.IdExemplary == exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Id)
            //                                    .Select(x => new GetListExemplaryCharacterSummaryResult_Attachment
            //                                    {
            //                                        FileName = x.FileName,
            //                                        FileSize = x.FileSize,
            //                                        FileExtension = x.FileExtension,
            //                                        FileType = x.FileType,
            //                                        OriginalFileName = x.OriginalFileName,
            //                                        Url = x.Url
            //                                    })
            //                                    .ToList();

            //    var resultExemplaryStudent = exemplaryCharacterStudentList
            //                                    .Where(x => x.IdExemplary == exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Id)
            //                                    .Select(x => new GetListExemplaryCharacterSummaryResult_ExemplaryStudent
            //                                    {
            //                                        Grade = new ItemValueVm
            //                                        {
            //                                            Id = x.Homeroom.Grade.Id,
            //                                            Description = x.Homeroom.Grade.Description
            //                                        },
            //                                        Student = new NameValueVm
            //                                        {
            //                                            Id = x.Student.Id,
            //                                            Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
            //                                        }
            //                                    })
            //                                    .ToList();

            //    exemplaryAllStudentList.AddRange(resultExemplaryStudent.Select(x => x.Student.Id).Distinct().ToList());

            //    var resultExemplaryCategory = exemplaryCategoryRawList
            //                                    .Where(x => x.IdExemplary == exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Id)
            //                                    .Select(x => new ItemValueVm
            //                                    {
            //                                        Id = x.IdLtExemplaryCategory,
            //                                        Description = x.LtExemplaryCategory.LongDesc
            //                                    })
            //                                    .ToList();

            //    var resultExemplaryLikes = exemplaryLikesRawList
            //                                    .Where(x => x.IdExemplary == exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Id)
            //                                    .ToList();

            //    var tempResultExemplary = new GetListExemplaryCharacterSummaryResult_Exemplary
            //    {
            //        Exemplary = new NameValueVm
            //        {
            //            Id = exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Id,
            //            Name = exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Title
            //        },
            //        AcademicYear = new ItemValueVm
            //        {
            //            Id = exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.AcademicYear.Id,
            //            Description = exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.AcademicYear.Description
            //        },
            //        Category = resultExemplaryCategory,
            //        Description = exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.Description,
            //        ExemplaryDate = exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.ExemplaryDate,
            //        PostedBy = new NameValueVm
            //        {
            //            Id = exemplaryCharacter.exemplary.exemplary.joinUserIn?.Id,
            //            Name = exemplaryCharacter.exemplary.exemplary.joinUserIn?.DisplayName,
            //        },
            //        ModifiedBy = new NameValueVm
            //        {
            //            Id = exemplaryCharacter.joinUserUp?.Id,
            //            Name = exemplaryCharacter.joinUserUp?.DisplayName,
            //        },
            //        IsLikedByMe = resultExemplaryLikes.Where(x => x.UserIn == AuthInfo.UserId).Any(),
            //        PostedByMe = AuthInfo.UserId == exemplaryCharacter.exemplary.exemplary.exemplary.exemplary.UserIn,
            //        LikesCount = resultExemplaryLikes.Count,
            //        Attachment = resultExemplaryAttachment,
            //        StudentList = resultExemplaryStudent
            //    };

            //    resultExamplary.Add(tempResultExemplary);
            //}
            #endregion

            var exemplaryCharacterCount = resultExamplary.Select(x => x.Exemplary.Id).Count();

            var exemplaryStudentCount = resultExamplary.SelectMany(x => x.StudentList).Select(x => x.Student.Id).Distinct().Count();

            var exemplaryItems = resultExamplary
                                    .AsQueryable()
                                    .OrderByDescending(x => x.AcademicYear.Description)
                                    .ThenByDescending(x => x.ExemplaryDate)
                                    .ThenBy(x => x.Exemplary.Name)
                                    .OrderByDynamic(param, aliasColumns)
                                    .SetPagination(param)
                                    .ToList();

            var count = param.CanCountWithoutFetchDb(resultExamplary.Count)
                ? resultExamplary.Count
                : resultExamplary.Select(x => x.Exemplary.Id).Count();

            var result = new GetListExemplaryCharacterSummaryResult
            {
                TotalExemplary = exemplaryCharacterCount,
                TotalStudent = exemplaryStudentCount,
                ExemplaryList = exemplaryItems
            };

            return Request.CreateApiResult2(result as object, param.CreatePaginationProperty(count));
        }
    }
}
