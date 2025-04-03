using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.User.FnCommunication.Message
{
    public class GetUserByExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetUserByExcelHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                throw new BadRequestException("Excel file not provided");

            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".xlsx")
                throw new BadRequestException("File extension not supported");

            using var fs = file.OpenReadStream();
            var workbook = new XSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);

            var firstRowVal = sheet.GetRow(0);

            if(firstRowVal.GetCell(0).ToString() != "username")
                throw new BadRequestException("Wrong format excel");

            if(sheet.LastRowNum == 0)
                throw new BadRequestException("No data is imported");

            var usernames = new List<string>();
            for (var row = 1; row <= sheet.LastRowNum; row++)
            {
                var rowVal = sheet.GetRow(row);
                if (rowVal != null && rowVal.GetCell(0).ToString() != "") // check if row not empty
                {
                    usernames.Add(rowVal.GetCell(0).ToString());
                }
            }

            var failed = new List<UserByExcel>();

            var users = await _dbContext.Entity<MsUser>()
                                        .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                                        .Where(x => usernames.Contains(x.Username))
                                        .ToListAsync();

            var studentIds = users.Select(x => x.Id).ToList();

            var homerooms = await _dbContext.Entity<MsHomeroom>()
                                                   .Include(x => x.HomeroomStudents)
                                                   .Include(x => x.Grade).ThenInclude(x => x.MsLevel)
                                                   .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.MsClassroom)
                                                   .Where(x => x.HomeroomStudents.Any(y => studentIds.Contains(y.IdStudent)))
                                                   .ToListAsync(CancellationToken);

            var success = new List<UserByExcel>();
            foreach (var user in users)
            {
                var homeroom = homerooms.FirstOrDefault(x => x.HomeroomStudents.Any(y => y.IdStudent == user.Id));
                success.Add(new UserByExcel
                {
                    UserId = user.Id,
                    Role = user.UserRoles.FirstOrDefault()?.Role.Description ?? "-",
                    Level = homeroom?.Grade.MsLevel.Description ?? "-",
                    Grade = homeroom?.Grade.Description ?? "-",
                    Homeroom = homeroom != null ? $"{homeroom.Grade.Code}{homeroom.GradePathwayClassroom.MsClassroom.Code}" : "-",
                    BinusianId = user.Id,
                    Username = user.Username,
                    FullName = user.DisplayName
                });

                if (usernames.Contains(user.Username))
                {
                    usernames.Remove(user.Username);
                }
            }

            //var success = await (
            //    from a in _dbContext.Entity<MsUser>()
            //    join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
            //    join lr in _dbContext.Entity<LtRole>() on r.IdRole equals lr.Id
            //    join s in _dbContext.Entity<MsStudent>() on a.Id equals s.Id into slf
            //    from xs in slf.DefaultIfEmpty()
            //    join sg in _dbContext.Entity<MsStudentGrade>() on a.Id equals sg.IdStudent into sglf
            //    from xsg in sglf.DefaultIfEmpty()
            //    join g in _dbContext.Entity<MsGrade>() on xsg.IdGrade equals g.Id into glf
            //    from xg in glf.DefaultIfEmpty()
            //    join l in _dbContext.Entity<MsLevel>() on xg.IdLevel equals l.Id into llf
            //    from xl in llf.DefaultIfEmpty()
            //    join ay in _dbContext.Entity<MsAcademicYear>() on xl.IdAcademicYear equals ay.Id into aylf
            //    from xay in aylf.DefaultIfEmpty()
            //    join h in _dbContext.Entity<MsHomeroom>() on xg.Id equals h.IdGrade into hlf
            //    from xh in hlf.DefaultIfEmpty()
            //    join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on xh.IdGradePathwayClassRoom equals gpc.Id into gpclf
            //    from xgpc in gpclf.DefaultIfEmpty()
            //    join c in _dbContext.Entity<MsClassroom>() on xgpc.IdClassroom equals c.Id into clf
            //    from xc in clf.DefaultIfEmpty()
            //    where usernames.Contains(a.Username)

            //    select new UserByExcel
            //    {
            //        UserId = a.Id,
            //        Role = lr.Description,
            //        Level = xl.Description != null ? xl.Description : "-",
            //        Grade = xg.Description != null ? xg.Description : "-",
            //        Homeroom = xg.Code != null ? $"{xg.Code} {xc.Code}" : "-",
            //        BinusianId = a.Id,
            //        Username = a.Username,
            //        FullName = a.DisplayName
            //    })
            //.Distinct()
            //.ToListAsync(CancellationToken);

            //foreach (var user in success)
            //{
            //    if (usernames.Contains(user.Username))
            //    {
            //        usernames.Remove(user.Username);
            //    }
            //}

            foreach (var user in usernames)
            {
                failed.Add(new UserByExcel
                {
                    UserId = "-",
                    Role = "-",
                    Level = "-",
                    Grade = "-",
                    Homeroom = "-",
                    BinusianId = "-",
                    Username = user,
                    FullName = "-"
                });
            }

            var result = new GetUserByExcelResult()
            {
                Success = success,
                Failed = failed
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
