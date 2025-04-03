using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class UserByExcel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string BinusianId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    } 
    public class GetUserByExcelResult
    {
        public List<UserByExcel> Success { get; set; }
        public List<UserByExcel> Failed { get; set; }
    }
}