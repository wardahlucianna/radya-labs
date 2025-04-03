using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Utils;

namespace BinusSchool.Attendance.FnAttendance.Utils
{
    public class ProtectedString
    {
        public static string CheckIdStudentAndDate(string ParamIdStudent)
        {
            var IdStudent = "";

            if (ParamIdStudent.Length <= 10)
            {
                IdStudent = ParamIdStudent;
            }
            else
            {
                var decryptString = AESCBCEncryptionUtil.Decrypt(ParamIdStudent); 

                if (!decryptString.Contains("#"))
                {
                    throw new BadRequestException("Invalid format");
                }

                var split = decryptString.Split('#');
                IdStudent = split[0];
                var dateNow = split[1];

                if (dateNow != DateTimeUtil.ServerTime.ToString("ddMMyyyy"))
                {
                    throw new BadRequestException("Date is not Today");
                }

                if (IdStudent.Where(char.IsDigit).ToArray().Length != 10)
                {
                    throw new BadRequestException("Student ID incorrect format");
                }
            }

            return IdStudent;
        }
    }
}
