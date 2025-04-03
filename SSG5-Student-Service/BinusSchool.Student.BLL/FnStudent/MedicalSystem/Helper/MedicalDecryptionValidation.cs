using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Utils;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Helper
{
    public static class MedicalDecryptionValidation
    {
        public static string ValidateDecryptionData(string EncryptedId)
        {
            // for testing
            //var encryptString = AESCBCEncryptionUtil.Encrypt($"{EncryptedId}#{DateTimeUtil.ServerTime.ToString("ddMMyyyy")}");
            //var decryptString = AESCBCEncryptionUtil.Decrypt(encryptString);

            var decryptString = AESCBCEncryptionUtil.DecryptBase64Url(EncryptedId);

            if (!decryptString.Contains("#"))
                throw new BadRequestException("Invalid credential");

            var split = decryptString.Split('#');
            var IdUser = split[0];
            var dateNow = split[1];

            if (dateNow != DateTimeUtil.ServerTime.ToString("ddMMyyyy"))
                throw new BadRequestException("Invalid credential");

            return IdUser;
        }
    }
}
