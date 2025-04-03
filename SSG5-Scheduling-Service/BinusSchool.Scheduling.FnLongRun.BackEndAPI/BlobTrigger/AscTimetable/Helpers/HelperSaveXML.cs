using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Helpers
{
    public class HelperSaveXML
    {
        private readonly ISchedulingDbContext _dbContext;
        public HelperSaveXML(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// simpan data lesson asc
        /// </summary>
        /// <param name="ascTimetableId"></param>
        /// <param name="idLesson"></param>
        /// <returns></returns>
        public string SaveLessonAsc(string ascTimetableId, string idLesson)
        {
            var simpn = new TrAscTimetableLesson();
            simpn.Id = Guid.NewGuid().ToString();
            simpn.IdAscTimetable = ascTimetableId;
            simpn.IdLesson = idLesson;

            _dbContext.Entity<TrAscTimetableLesson>().Add(simpn);

            return string.Empty;
        }


        /// <summary>
        /// simpan data home rome asc
        /// </summary>
        /// <param name="ascTimetableId"></param>
        /// <param name="idHomerome"></param>
        /// <returns></returns>
        public string SaveHomerromAsc(string ascTimetableId, string idHomerome)
        {
            var simpn = new TrAscTimetableHomeroom();
            simpn.Id = Guid.NewGuid().ToString();
            simpn.IdAscTimetable = ascTimetableId;
            simpn.IdHomeroom = idHomerome;

            _dbContext.Entity<TrAscTimetableHomeroom>().Add(simpn);

            return string.Empty;
        }



        /// <summary>
        /// update data homeroom dari database pake data xml yg masuk
        /// </summary>
        /// <param name="homeroom"></param>
        /// <param name="model"></param>
        /// <param name="semester"></param>
        /// <param name="CaPosition"></param>
        /// <returns></returns>
        public List<MsHomeroomStudent> UpdateDataHomeRoom(MsHomeroom homeroom, ClassPackageVm model, int semester, string CaPosition, bool saveEnrolmentStudent = true)
        {
            var result = new List<MsHomeroomStudent>();
            homeroom.IdVenue = !string.IsNullOrEmpty(model.Venue.IdFromMasterData) ? model.Venue.IdFromMasterData : null;
            homeroom.IdGradePathway = model.Grade.IdGradePathway;
            _dbContext.Entity<MsHomeroom>().Update(homeroom);


            //jika data homerome student nya harus di simpan dari yg data xml 
            //maka harus di kompare 2 list student dari database dan dari data dari xml 

            if (saveEnrolmentStudent)
            {
                if (homeroom.HomeroomStudents.Count > 0)
                {
                    //cek apabila data yg lolos dari validasi di depan 
                    var studentXML = model.StudentInHomeRoom.Where(p => p.StudentIsreadyFromMaster).ToList();
                    if (studentXML.Count > 0)
                    {
                        var studentIdXml = studentXML.Select(p => p.IdStudent).ToList();
                        var studentIdDatabase = homeroom.HomeroomStudents.Select(p => p.IdStudent).ToList();

                        //get data yg tidak sama dari database dan unutk di delete
                        var dataStudentMaterDeleted = homeroom.HomeroomStudents.Where(p => !studentIdXml.Contains(p.IdStudent)).ToList();
                        //getdata yg baru dari data xml dan unutk di insert
                        var dataStudentinsert = studentXML.Where(p => !studentIdDatabase.Contains(p.IdStudent)).ToList();

                        foreach (var itemhomeromeStudent in dataStudentinsert)
                        {
                            var getStudent = new MsHomeroomStudent();
                            getStudent.Id = Guid.NewGuid().ToString();
                            getStudent.IdStudent = itemhomeromeStudent.IdStudent;
                            getStudent.Semester = semester;
                            getStudent.Gender = itemhomeromeStudent.Gender == "Male" ? Gender.Male : Gender.Female;
                            getStudent.IdHomeroom = homeroom.Id;
                            getStudent.Religion = !string.IsNullOrWhiteSpace(itemhomeromeStudent.Religion) ? itemhomeromeStudent.Religion : "";

                            _dbContext.Entity<MsHomeroomStudent>().Add(getStudent);

                            result.Add(getStudent);
                        }

                        //hapus data enrolemnt yg ada dari yg ada 
                        foreach (var item in dataStudentMaterDeleted)
                        {
                            foreach (var itemEnrolment in item.HomeroomStudentEnrollments)
                            {
                                itemEnrolment.IsActive = false;
                                _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(itemEnrolment);
                                foreach (var itemTrAsc in itemEnrolment.AscTimetableEnrollments)
                                {
                                    itemTrAsc.IsActive = false;
                                    _dbContext.Entity<TrAscTimetableEnrollment>().Update(itemTrAsc);
                                }

                            }
                            item.IsActive = false;
                            _dbContext.Entity<MsHomeroomStudent>().Update(item);
                        }

                        //get data yg ada di database untuk save enrolemnt di function save enrolment 
                        //dan unutk update kalo butuh 
                        var dataStudent = homeroom.HomeroomStudents.Where(p => studentIdXml.Contains(p.IdStudent)).ToList();
                        result.AddRange(dataStudent);
                    }
                    else
                    {
                        result.AddRange(homeroom.HomeroomStudents);
                    }

                }
            }
            else
            {
                //get all student for add student enrolment in function SaveEnrollment
                result.AddRange(homeroom.HomeroomStudents);
            }


            //apabila data teacher dari xml nya ada maka simpan satu saja sebagai CA di home rome
            //dan hapus data teacher sebelum nya 
            if (model.Teacher.Where(p => p.DataIsUseInMaster).Count() > 0)
            {
                //if (!string.IsNullOrWhiteSpace(CaPosition))
                //{
                //insert data teacher yg baru dari xml
                var get = model.Teacher.Where(p => p.DataIsUseInMaster).FirstOrDefault();
                if (get != null)
                {
                    //hapus data teacher yg lama nya
                    _dbContext.Entity<MsHomeroomTeacher>().RemoveRange(homeroom.HomeroomTeachers);

                    var getTeacher = new MsHomeroomTeacher();
                    getTeacher.Id = Guid.NewGuid().ToString();
                    getTeacher.IdBinusian = get.IdFromMasterData;
                    //default position di isi CA harus nya 
                    getTeacher.IdTeacherPosition = CaPosition;
                    getTeacher.IdHomeroom = homeroom.Id;
                    getTeacher.IsAttendance = true;
                    getTeacher.IsScore = true;
                    getTeacher.IsShowInReportCard = true;

                    _dbContext.Entity<MsHomeroomTeacher>().Add(getTeacher);
                }
                //dan nanti ngaruh ke modul teacing load nya tapi blm di implement
                //}
            }

            //update home room pathway 
            UpdateHomeroomPathway(homeroom.HomeroomPathways.ToList(), model.Pathway, homeroom.Id);

            return result;
        }

        private void UpdateHomeroomPathway(List<MsHomeroomPathway> dataInDB, List<DataModelGeneral> dataFromXMl, string homeRoomID)
        {
            if (dataFromXMl.Any())
            {
                var getPathwayDetailFromXMl = dataFromXMl.Select(p => p.IdFromMasterData).ToList();
                var getPathwayDetailFromDB = dataInDB.Select(p => p.IdGradePathwayDetail).ToList();

                //get data yg tidak sama dari database dan unutk di delete
                var getdataDelete = dataInDB.Where(p => !getPathwayDetailFromXMl.Contains(p.IdGradePathwayDetail)).ToList();

                foreach (var item in getdataDelete)
                {
                    item.IsActive = false;
                    _dbContext.Entity<MsHomeroomPathway>().Update(item);
                }

                //get data unutk di insert
                var getdataInsert = dataFromXMl.Where(p => !getPathwayDetailFromDB.Contains(p.IdFromMasterData)).ToList();
                foreach (var item in getdataInsert)
                {
                    var getPathway = new MsHomeroomPathway();
                    getPathway.Id = Guid.NewGuid().ToString();
                    getPathway.IdGradePathwayDetail = item.IdFromMasterData;
                    getPathway.IdHomeroom = homeRoomID;

                    _dbContext.Entity<MsHomeroomPathway>().Add(getPathway);
                }
            }

        }


        /// <summary>
        /// update data lesson 
        /// </summary>
        /// <param name="lesson"></param>
        /// <param name="model"></param>
        public void UpdateLesson(MsLesson lesson, LessonUploadXMLVm model, List<MsHomeroomPathway> homeroomPathway)
        {
            if (model.WeekCode == "All")
            {
                lesson.IdWeekVariant = "AW";
            }
            else
            {
                lesson.IdWeekVariant = model.TotalWeek <= 2 ? "VAR2" : model.TotalWeek == 3 ? "VAR3" : model.TotalWeek == 4 ? "VAR4" : "VAR4";
            }
            lesson.TotalPerWeek = model.TotalWeek;

            _dbContext.Entity<MsLesson>().Update(lesson);

            //var getdataSchedule = ascTimetable.AscTimetableSchedules.Where(p => p.Schedule.IdLesson == getdata.Id).ToList();

            //insert data teacher yg braru
            if (model.Teacher.Count > 0)
            {
                if (lesson.LessonTeachers.Count > 0)
                {
                    foreach (var item in lesson.LessonTeachers)
                    {
                        item.IsActive = false;
                        _dbContext.Entity<MsLessonTeacher>().Update(item);
                    }

                }

                int countTeacher = 0;
                foreach (var itemTeacher in model.Teacher)
                {
                    countTeacher++;
                    var isprimary = false;
                    var isAttendance = false;
                    var isScore = false;
                    if (countTeacher == 1)
                    {
                        isprimary = true;
                    }

                    isAttendance = true;
                    isScore = true;

                    var lesonTeacher = new MsLessonTeacher();
                    lesonTeacher.Id = Guid.NewGuid().ToString();
                    lesonTeacher.IdUser = itemTeacher.IdFromMasterData;
                    lesonTeacher.IsPrimary = isprimary;
                    lesonTeacher.IsAttendance = isAttendance;
                    lesonTeacher.IsScore = isScore;
                    lesonTeacher.IdLesson = lesson.Id;

                    _dbContext.Entity<MsLessonTeacher>().Add(lesonTeacher);
                }
            }

            UpdatelessonPathway(lesson.LessonPathways.ToList(), homeroomPathway, lesson.Id);
        }

        public void UpdatelessonPathway(List<MsLessonPathway> dataInDb, List<MsHomeroomPathway> dataNew, string lessonId)
        {
            foreach (var dataOld in dataInDb)
            {
                dataOld.IsActive = false;
                _dbContext.Entity<MsLessonPathway>().Update(dataOld);
            }

            foreach (var itemPathway in dataNew)
            {
                var lessonPathway = new MsLessonPathway();
                lessonPathway.Id = Guid.NewGuid().ToString();
                lessonPathway.IdHomeroomPathway = itemPathway.Id;
                lessonPathway.IdLesson = lessonId;

                _dbContext.Entity<MsLessonPathway>().Add(lessonPathway);
            }
        }

        /// <summary>
        /// simpan data lesson 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="academicyears"></param>
        /// <param name="formatClass"></param>
        /// <param name="IdHomeroomPathway"></param>
        /// <param name="semester"></param>
        /// <returns></returns>
        public MsLesson SimpanLesson(LessonUploadXMLVm model, string academicyears, string formatClass, List<MsHomeroomPathway> HomeroomPathway, int semester)
        {
            var result = new MsLesson();
            //insert kalo blm ada datanya 
            var dataLesson = new MsLesson();
            dataLesson.Id = Guid.NewGuid().ToString();
            dataLesson.IdGrade = model.Grade.IdFromMasterData;
            dataLesson.IdAcademicYear = academicyears;
            dataLesson.IdSubject = model.Subject.IdFromMasterData;
            dataLesson.ClassIdExample = model.ClassId;
            dataLesson.TotalPerWeek = model.TotalWeek;
            dataLesson.ClassIdFormat = !string.IsNullOrWhiteSpace(formatClass) ? formatClass : "";
            dataLesson.ClassIdGenerated = model.ClassId;
            if (model.WeekCode == "All")
            {
                dataLesson.IdWeekVariant = "AW";
            }
            else
            {
                dataLesson.IdWeekVariant = model.TotalWeek <= 2 ? "VAR2" : model.TotalWeek == 3 ? "VAR3" : model.TotalWeek == 4 ? "VAR4" : "VAR4";
            }
            dataLesson.Semester = semester;

            result = dataLesson;

            _dbContext.Entity<MsLesson>().Add(dataLesson);

            foreach (var itemPathway in HomeroomPathway)
            {
                var lessonPathway = new MsLessonPathway();
                lessonPathway.Id = Guid.NewGuid().ToString();
                lessonPathway.IdHomeroomPathway = itemPathway.Id;
                lessonPathway.IdLesson = dataLesson.Id;

                _dbContext.Entity<MsLessonPathway>().Add(lessonPathway);
            }

            int countTeacher = 0;
            foreach (var itemTeacher in model.Teacher)
            {
                countTeacher++;
                var isprimary = false;
                var isAttendance = false;
                var isScore = false;
                if (countTeacher == 1)
                {
                    isprimary = true;
                    isAttendance = true;
                    isScore = true;
                }
                var lesonTeacher = new MsLessonTeacher();
                lesonTeacher.Id = Guid.NewGuid().ToString();
                lesonTeacher.IdUser = itemTeacher.IdFromMasterData;
                lesonTeacher.IsPrimary = isprimary;
                lesonTeacher.IsAttendance = isAttendance;
                lesonTeacher.IsScore = isScore;
                lesonTeacher.IdLesson = dataLesson.Id;

                _dbContext.Entity<MsLessonTeacher>().Add(lesonTeacher);
            }
            return result;
        }

        /// <summary>
        /// simpand home rome data
        /// </summary>
        /// <param name="homeRoom"></param>
        /// <param name="semester"></param>
        /// <param name="academicYeras"></param>
        /// <param name="CaPosition"></param>
        /// <returns></returns>
        public string SaveHomeRoomData(ClassPackageVm homeRoom, int semester, string academicYeras, string CaPosition)
        {
            var homeromesDatas = new MsHomeroom();
            homeromesDatas.Id = Guid.NewGuid().ToString();
            homeromesDatas.IdAcademicYear = academicYeras;
            homeromesDatas.IdGrade = homeRoom.Grade.IdFromMasterData;
            homeromesDatas.IdVenue = !string.IsNullOrEmpty(homeRoom.Venue.IdFromMasterData) ? homeRoom.Venue.IdFromMasterData : null;
            homeromesDatas.Semester = semester;
            homeromesDatas.IdGradePathwayClassRoom = homeRoom.Class.IdFromMasterData;
            homeromesDatas.IdGradePathway = homeRoom.Grade.IdGradePathway;

            _dbContext.Entity<MsHomeroom>().Add(homeromesDatas);

            if (homeRoom.Teacher.Where(p => p.DataIsUseInMaster).Count() > 0)
            {
                //ini di cek dulu posisi ca ny ada atu ga 
                //if (!string.IsNullOrWhiteSpace(CaPosition))
                //{
                var get = homeRoom.Teacher.Where(p => p.DataIsUseInMaster).FirstOrDefault();
                if (get != null)
                {
                    var getTeacher = new MsHomeroomTeacher();
                    getTeacher.Id = Guid.NewGuid().ToString();
                    getTeacher.IdBinusian = get.IdFromMasterData;
                    //default position di isi CA harus nya 
                    getTeacher.IdTeacherPosition = CaPosition;
                    getTeacher.IdHomeroom = homeromesDatas.Id;
                    getTeacher.IsAttendance = true;
                    getTeacher.IsScore = true;
                    getTeacher.IsShowInReportCard = true;

                    _dbContext.Entity<MsHomeroomTeacher>().Add(getTeacher);
                }
                //}
            }

            return homeromesDatas.Id;
        }

        /// <summary>
        /// simpan home rome pathway
        /// </summary>
        /// <param name="homeRoomPathway"></param>
        /// <param name="homeRoomId"></param>
        /// <returns></returns>
        public List<string> SaveHomeRoomPathway(List<DataModelGeneral> homeRoomPathway, string homeRoomId)
        {
            var result = new List<string>();

            foreach (var itemhomeromePthway in homeRoomPathway)
            {
                var getPathway = new MsHomeroomPathway();
                getPathway.Id = Guid.NewGuid().ToString();
                getPathway.IdGradePathwayDetail = itemhomeromePthway.IdFromMasterData;
                getPathway.IdHomeroom = homeRoomId;

                _dbContext.Entity<MsHomeroomPathway>().Add(getPathway);

                result.Add(getPathway.Id);
            }

            return result;
        }

        /// <summary>
        /// simpan home rome student 
        /// </summary>
        /// <param name="homeRoomData"></param>
        /// <param name="homeRoomId"></param>
        /// <param name="semester"></param>
        /// <param name="isSaveData"></param>
        /// <returns></returns>
        public List<MsHomeroomStudent> SaveHomeRoomStudent(List<StudentInHomeRoom> homeRoomData, string homeRoomId, int semester, bool isSaveData = true)
        {
            var homeromestudent = new List<MsHomeroomStudent>();
            if (isSaveData && homeRoomData != null)
            {
                var studentXML = homeRoomData.Where(p => p.StudentIsreadyFromMaster).ToList();
                if (studentXML.Any())
                {
                    foreach (var itemhomeromeStudent in homeRoomData.Where(p => p.StudentIsreadyFromMaster))
                    {
                        var getStudent = new MsHomeroomStudent();
                        getStudent.Id = Guid.NewGuid().ToString();
                        getStudent.IdStudent = itemhomeromeStudent.IdStudent;
                        getStudent.Semester = semester;
                        getStudent.Gender = itemhomeromeStudent.Gender == "Male" ? Gender.Male : Gender.Female;
                        getStudent.IdHomeroom = homeRoomId;
                        getStudent.Religion = !string.IsNullOrWhiteSpace(itemhomeromeStudent.Religion) ? itemhomeromeStudent.Religion : "";

                        _dbContext.Entity<MsHomeroomStudent>().Add(getStudent);

                        homeromestudent.Add(getStudent);
                    }
                }
            }
            return homeromestudent;
        }
    }
}
