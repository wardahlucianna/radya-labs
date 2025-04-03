using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class GetExemplaryCharacterViewNewHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public GetExemplaryCharacterViewNewHandler(IStudentDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        private static readonly IDictionary<string, string[]> _fileTypes = new Dictionary<string, string[]>()
        {
            { "image", new[]{ ".png", ".jpg", ".jpeg" } },
            { "video", new[]{ ".mkv", ".mp4", ".webm", ".mov", ".wmv" } }
        };


        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExemplaryCharacterViewRequest>(
                         nameof(GetExemplaryCharacterViewRequest.IdSchool),
                         nameof(GetExemplaryCharacterViewRequest.IdUserRequested),
                         nameof(GetExemplaryCharacterViewRequest.Type));

            List<string> childList = new List<string>();
            if(param.IsParent != null && param.IsParent.Value)
            {
                #region get child
                var username = param.IdUserRequested;

                var idStudent = string.Concat(username.Where(char.IsDigit));

                var siblingGroup = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(x => x.IdStudent == idStudent).Select(x => x.Id).FirstOrDefaultAsync(CancellationToken);

                if (siblingGroup != null)
                {
                    var siblingStudent = await _dbContext.Entity<MsSiblingGroup>().Where(x => x.Id == siblingGroup).Select(x => x.IdStudent).ToListAsync(CancellationToken);

                    var childListTemp = await _dbContext.Entity<MsStudent>()
                                    .Where(x => siblingStudent.Any(y => y == x.Id))
                                    .Select(x => x.Id)
                                    .ToListAsync(CancellationToken);

                    childList.AddRange(childListTemp);
                }
                else
                {

                    var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                            .Where(x => x.IdStudent == idStudent)
                                            .Select(x => new
                                            {
                                                idParent = x.IdParent
                                            }).FirstOrDefaultAsync(CancellationToken);

                    var childListTemp = await _dbContext.Entity<MsStudentParent>()
                                .Include(x => x.Student)
                                .Where(x => x.IdParent == dataStudentParent.idParent)
                                .Select(x => x.Student.Id)
                                .ToListAsync(CancellationToken);

                    childList.AddRange(childListTemp);
                }
                #endregion

            }

            string[] paramlistValue = null;
            if (param.Type == "value")
            {
                paramlistValue = param.IdValueList.Split("~");
            }

            var AcademicActived = await _dbContext.Entity<MsPeriod>()
                                    .Include(x => x.Grade).ThenInclude(y => y.MsLevel)
                                    .Where(a => a.StartDate >= DateTime.UtcNow)
                                    .Select(a => a.Grade.MsLevel.IdAcademicYear)
                                    .FirstOrDefaultAsync();

            var resultExemplary = new List<GetExemplaryCharacterViewResult>();

            var predicate = PredicateBuilder.True<TrExemplary>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    =>
                    x.ExemplaryStudents.Any(a => EF.Functions.Like(a.IdStudent, param.SearchPattern()))
                    || x.ExemplaryStudents.Any(a => EF.Functions.Like((a.Student.FirstName != null ? a.Student.FirstName + " " : ""), param.SearchPattern()))
                    || EF.Functions.Like(x.Title, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    );

            string SASToken = GetSasUri(5);

            var ExemplaryCharacter = await _dbContext.Entity<TrExemplary>()
                                   .Include(x => x.ExemplaryStudents).ThenInclude(y => y.Student)
                                   .Include(x => x.ExemplaryStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.MsGradePathwayClassroom).ThenInclude(y => y.Classroom)
                                   .Include(x => x.ExemplaryStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.MsLevel)
                                   .Include(x => x.ExemplaryAttachments)
                                   .Include(x => x.ExemplaryLikes)
                                   .Include(x => x.LtExemplaryCategory)
                                   .Include(x => x.TrExemplaryValues).ThenInclude(y => y.LtExemplaryValue)
                                   .Include(x => x.AcademicYear)
                                   .Where(predicate)
                                   .Where(a => (param.Type == "value" ? (a.TrExemplaryValues.Select(z => z.IdLtExemplaryValue).Where(y => paramlistValue.Contains(y)).Any()) : true)
                                   && a.AcademicYear.IdSchool == param.IdSchool
                                   && ((param.IsParent != null && param.IsParent.Value) ? (a.ExemplaryStudents.Select(z => z.IdStudent).Where(y => childList.Contains(y)).Any()) : true)
                                   )
                                   .Select(a => new GetExemplaryCharacterViewResult()
                                   {
                                       Id = a.Id,
                                       IdExemplary = a.Id,
                                       IdAcademicYear = a.IdAcademicYear,
                                       Student = (a.ExemplaryStudents.Count() > 1 ? (a.ExemplaryStudents.Count() + " Student(s)") : (a.ExemplaryStudents.Select(b => (b.Student.FirstName != null ? b.Student.FirstName + " " : "") + b.Student.LastName + " " + b.Homeroom.Grade.Code + " " + b.Homeroom.MsGradePathwayClassroom.Classroom.Code).FirstOrDefault())),
                                       StudentList = a.ExemplaryStudents.Select(b => new ExemplaryCharacterView_Student
                                       {
                                           Level = new ItemValueVm
                                           {
                                               Id = b.Homeroom.Grade.MsLevel.Id,
                                               Description = b.Homeroom.Grade.MsLevel.Description
                                           },
                                           Homeroom = new ItemValueVm
                                           {
                                               Id = b.Homeroom.Id,
                                               Description = b.Homeroom.Grade.Code + " " + b.Homeroom.MsGradePathwayClassroom.Classroom.Code
                                           },
                                           Student = new NameValueVm
                                           {
                                               Id = b.Student.Id,
                                               Name = (b.Student.FirstName != null ? b.Student.FirstName + " " : "") + b.Student.LastName
                                           }
                                       }).ToList(),
                                       Category = new ItemValueVm
                                       {
                                           Id = a.LtExemplaryCategory.Id,
                                           Description = a.LtExemplaryCategory.LongDesc
                                       },
                                       CountLikes = a.ExemplaryLikes.Count(),
                                       IsYouLiked = a.ExemplaryLikes.Where(b => b.UserIn == param.IdUserRequested).Count() > 0,
                                       Updatedby = a.UserUp,
                                       UpdatedDateView = a.DateUp != null ? ((DateTime)a.DateUp).ToString("dd MMM yyyy HH:mm") : null,
                                       Postedby = a.UserIn,
                                       PostedDate = a.PostedDate,
                                       PostedDateView = a.PostedDate.ToString("dd MMM yyyy HH:mm"),
                                       ExemplaryAttachments = a.ExemplaryAttachments.Select(b => new ExemplaryCharacterView_Attachment()
                                       {
                                           FileName = b.FileName,
                                           FileSize = b.FileSize,
                                           FileExtension = b.FileExtension,
                                           FileType = b.FileType,

                                           Url = (b.Url + SASToken).Replace(" ", "%20"),
                                           UrlWithSASToken = (b.Url + SASToken).Replace(" ", "%20")
                                       }).ToList(),
                                       ValueList = a.TrExemplaryValues.Select(b => new ExemplaryCharacterView_Value()
                                       {
                                           IdExemplaryValue = b.IdLtExemplaryValue,
                                           ShortDesc = b.LtExemplaryValue.ShortDesc,
                                           LongDesc = b.LtExemplaryValue.LongDesc
                                       }).ToList()
                                   })
                                   .OrderByDescending(a => a.PostedDate)
                                   .ToListAsync();

            //var UserData = await _dbContext.Entity<MsUser>().Where(a => ExemplaryCharacter.Select(b => b.Postedby).Any(c => c == a.Id) ||
            //                                                             ExemplaryCharacter.Select(b => b.Updatedby).Any(c => c == a.Id)).ToListAsync();


            foreach (var item in ExemplaryCharacter)
            {
                item.Postedby = (item.Postedby != null ? (_dbContext.Entity<MsUser>().Where(a => a.Id == item.Postedby).FirstOrDefault()?.DisplayName ?? item.Postedby) : null);
                item.Updatedby = (item.Updatedby != null ? (_dbContext.Entity<MsUser>().Where(a => a.Id == item.Updatedby).FirstOrDefault()?.DisplayName ?? item.Updatedby) : null);

                //item.Postedby = (UserData.Where(b => b.Id == item.Postedby).FirstOrDefault()?.DisplayName ?? item.Postedby);
                //item.Updatedby = (UserData.Where(b => b.Id == item.Updatedby).FirstOrDefault()?.DisplayName ?? item.Updatedby);
            }



            if (param.Type == "toprating")
            {
                ExemplaryCharacter = ExemplaryCharacter.Where(a => a.IdAcademicYear == (AcademicActived != null ? AcademicActived : a.IdAcademicYear))
                                                     .OrderByDescending(a => a.CountLikes)
                                                     .ToList();

            }


            IReadOnlyList<GetExemplaryCharacterViewResult> items;
            if (param.Return == CollectionType.Lov)
                items = ExemplaryCharacter.AsQueryable().ToList();
            else
                items = ExemplaryCharacter.AsQueryable().SetPagination(param).ToList();

            var resultCount = ExemplaryCharacter.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(resultCount));

        }

        public string GetSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("exemplary-character");

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            // Return the URI string for the container, including the SAS token.
            return sasContainerToken;

            //Return blob SAS Token
            //return sasContainerToken;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Student:AccountStorage"]);
                return storageAccount;
            }

        }
    }
}
