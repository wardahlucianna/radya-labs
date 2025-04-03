using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Employee.FnStaff.Teacher
{
    public class StaffInformationResponseDTO
    {
        public StaffInformationResponseDTO()
        {
            this.staffEducationList = new List<StaffEducation>();
            this.familyMemberList = new List<FamilyMember>();
        }

        public bool ShowCredentialsData { get; set; }
        public string kdDep { get; set; }
        public string group_bu { get; set; }
        public string nama_groupbu { get; set; }
        public string bu { get; set; }
        public string nama_bu { get; set; }
        public string div { get; set; }
        public string nama_div { get; set; }
        public string dg { get; set; }
        public string nama_dg { get; set; }
        public string dep { get; set; }
        public string nama_dep { get; set; }
        public string section { get; set; }
        public string nama_section { get; set; }
        public string subsection { get; set; }
        public string nama_subsection { get; set; }
        public string binusian_id { get; set; }
        public string nip { get; set; } // employeeid
        public string kddsn { get; set; }
        public string nik { get; set; }
        public string passportNumber { get; set; }
        public DateTime? passportExpiredDate { get; set; }
        public string nama { get; set; }
        public string nama_ibu { get; set; }
        public string email1 { get; set; }
        public string email2 { get; set; }
        public string kd_jabatan { get; set; }
        public string nm_jabatan { get; set; }
        public int level_binusian { get; set; }
        public string atasan_langsung { get; set; }
        public string nm_atasan_langsung { get; set; }
        public string kddep_atasan { get; set; }
        public int level_binusian_2 { get; set; }
        public int? level_binusian_old { get; set; }
        // ?? place of birth
        public DateTime tgl_lahir { get; set; } //dob
        public string jnkel { get; set; }
        public string no_asuransi { get; set; }
        public string status_pernikahan { get; set; }
        public string jenis_pegawai { get; set; }
        public string level_dep { get; set; }
        public string singkatan { get; set; }
        public string lokasi_kerja { get; set; }
        public DateTime? tgl_masuk { get; set; }
        public string tmp_lahir { get; set; }
        public string mobilePhoneNumber { get; set; }
        public string residencePhoneNumber { get; set; }
        public string negara { get; set; }
        public string kd_agama { get; set; }
        public string nmagm { get; set; }
        public string nmkel { get; set; } //gender , ada genderid?
        public string alamat { get; set; }
        public string kota { get; set; }
        public string kdPos { get; set; }
        public string jkA_ID { get; set; }
        public string jkA_Name { get; set; }
        public string jkA_BandName { get; set; }
        public string nm_jenis_pegawai { get; set; }
        public string kd_kewarganegaraan { get; set; }
        public string nm_Kewarganegaraan { get; set; }
        public string nmneg { get; set; }
        public List<StaffEducation> staffEducationList { get; set; }
        public List<FamilyMember> familyMemberList { get; set; }


        public class StaffEducation
        {
            public string educationLevelID { get; set; }
            public string educationLevelName { get; set; }
            public string institutionName { get; set; }
            public int? attendingYear { get; set; }
            public int? graduateYear { get; set; }
            public string gpa { get; set; }
            public string major { get; set; }
        }
        public class FamilyMember
        {
            public string binusian_ID { get; set; }
            public string name { get; set; }
            public DateTime tgl_Lahir { get; set; }
            public string relation { get; set; }
        }
    }
}
