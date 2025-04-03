using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel.XML2008Type
{
    [XmlRoot(ElementName = "day")]
    public class Day
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "short")]
        public string Short { get; set; }
        [XmlAttribute(AttributeName = "day")]
        public string _day { get; set; }
    }

    [XmlRoot(ElementName = "days")]
    public class Days
    {
        [XmlElement(ElementName = "day")]
        public List<Day> Day { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "period")]
    public class Period
    {
        [XmlAttribute(AttributeName = "period")]
        public string _period { get; set; }
        [XmlAttribute(AttributeName = "starttime")]
        public string Starttime { get; set; }
        [XmlAttribute(AttributeName = "endtime")]
        public string Endtime { get; set; }
    }

    [XmlRoot(ElementName = "periods")]
    public class Periods
    {
        [XmlElement(ElementName = "period")]
        public List<Period> Period { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "dayperiod")]
    public class Dayperiod
    {
        [XmlAttribute(AttributeName = "day")]
        public string Day { get; set; }
        [XmlAttribute(AttributeName = "period")]
        public string Period { get; set; }
        [XmlAttribute(AttributeName = "starttime")]
        public string Starttime { get; set; }
        [XmlAttribute(AttributeName = "endtime")]
        public string Endtime { get; set; }
    }

    [XmlRoot(ElementName = "dayperiods")]
    public class Dayperiods
    {
        [XmlElement(ElementName = "dayperiod")]
        public List<Dayperiod> Dayperiod { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "teacher")]
    public class Teacher
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "short")]
        public string Short { get; set; }
        [XmlAttribute(AttributeName = "gender")]
        public string Gender { get; set; }
        [XmlAttribute(AttributeName = "color")]
        public string Color { get; set; }
    }

    [XmlRoot(ElementName = "teachers")]
    public class Teachers
    {
        [XmlElement(ElementName = "teacher")]
        public List<Teacher> Teacher { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "class")]
    public class Class
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "short")]
        public string Short { get; set; }
        [XmlAttribute(AttributeName = "teacherid")]
        public string Teacherid { get; set; }
        [XmlAttribute(AttributeName = "classroomids")]
        public string Classroomids { get; set; }
        [XmlAttribute(AttributeName = "grade")]
        public string Grade { get; set; }
    }

    [XmlRoot(ElementName = "classes")]
    public class Classes
    {
        [XmlElement(ElementName = "class")]
        public List<Class> Class { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "subject")]
    public class Subject
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "short")]
        public string Short { get; set; }
    }

    [XmlRoot(ElementName = "subjects")]
    public class Subjects
    {
        [XmlElement(ElementName = "subject")]
        public List<Subject> Subject { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "classroom")]
    public class Classroom
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "short")]
        public string Short { get; set; }
    }

    [XmlRoot(ElementName = "classrooms")]
    public class Classrooms
    {
        [XmlElement(ElementName = "classroom")]
        public List<Classroom> Classroom { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "students")]
    public class Students
    {
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "group")]
    public class Group
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "classid")]
        public string Classid { get; set; }
        [XmlAttribute(AttributeName = "entireclass")]
        public string Entireclass { get; set; }
        [XmlAttribute(AttributeName = "divisiontag")]
        public string Divisiontag { get; set; }
        [XmlAttribute(AttributeName = "studentcount")]
        public string Studentcount { get; set; }
    }

    [XmlRoot(ElementName = "groups")]
    public class Groups
    {
        [XmlElement(ElementName = "group")]
        public List<Group> Group { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "lesson")]
    public class Lesson
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "classids")]
        public string Classids { get; set; }
        [XmlAttribute(AttributeName = "subjectid")]
        public string Subjectid { get; set; }
        [XmlAttribute(AttributeName = "periodspercard")]
        public string Periodspercard { get; set; }
        [XmlAttribute(AttributeName = "periodsperweek")]
        public string Periodsperweek { get; set; }
        [XmlAttribute(AttributeName = "teacherids")]
        public string Teacherids { get; set; }
        [XmlAttribute(AttributeName = "classroomids")]
        public string Classroomids { get; set; }
        [XmlAttribute(AttributeName = "groupids")]
        public string Groupids { get; set; }
        [XmlAttribute(AttributeName = "studentids")]
        public string Studentids { get; set; }
        [XmlAttribute(AttributeName = "weeks")]
        public string Weeks { get; set; }
    }

    [XmlRoot(ElementName = "lessons")]
    public class Lessons
    {
        [XmlElement(ElementName = "lesson")]
        public List<Lesson> Lesson { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "card")]
    public class Card
    {
        [XmlAttribute(AttributeName = "lessonid")]
        public string Lessonid { get; set; }
        [XmlAttribute(AttributeName = "classroomids")]
        public string Classroomids { get; set; }
        [XmlAttribute(AttributeName = "day")]
        public string Day { get; set; }
        [XmlAttribute(AttributeName = "period")]
        public string Period { get; set; }
    }

    [XmlRoot(ElementName = "cards")]
    public class Cards
    {
        [XmlElement(ElementName = "card")]
        public List<Card> Card { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "grade")]
    public class Grade
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "short")]
        public string Short { get; set; }
        [XmlAttribute(AttributeName = "grade")]
        public string _grade { get; set; }
    }

    [XmlRoot(ElementName = "grades")]
    public class Grades
    {
        [XmlElement(ElementName = "grade")]
        public List<Grade> Grade { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
    }

    [XmlRoot(ElementName = "timetable")]
    public class Timetable2020
    {
        [XmlElement(ElementName = "days")]
        public Days Days { get; set; }
        [XmlElement(ElementName = "periods")]
        public Periods Periods { get; set; }
        [XmlElement(ElementName = "dayperiods")]
        public Dayperiods Dayperiods { get; set; }
        [XmlElement(ElementName = "teachers")]
        public Teachers Teachers { get; set; }
        [XmlElement(ElementName = "classes")]
        public Classes Classes { get; set; }
        [XmlElement(ElementName = "subjects")]
        public Subjects Subjects { get; set; }
        [XmlElement(ElementName = "classrooms")]
        public Classrooms Classrooms { get; set; }
        [XmlElement(ElementName = "students")]
        public Students Students { get; set; }
        [XmlElement(ElementName = "groups")]
        public Groups Groups { get; set; }
        [XmlElement(ElementName = "lessons")]
        public Lessons Lessons { get; set; }
        [XmlElement(ElementName = "cards")]
        public Cards Cards { get; set; }
        [XmlElement(ElementName = "grades")]
        public Grades Grades { get; set; }
        [XmlAttribute(AttributeName = "ascttversion")]
        public string Ascttversion { get; set; }
        [XmlAttribute(AttributeName = "importtype")]
        public string Importtype { get; set; }
        [XmlAttribute(AttributeName = "options")]
        public string Options { get; set; }
        [XmlAttribute(AttributeName = "defaultexport")]
        public string Defaultexport { get; set; }
        [XmlAttribute(AttributeName = "displayname")]
        public string Displayname { get; set; }
        [XmlAttribute(AttributeName = "displaycountries")]
        public string Displaycountries { get; set; }
    }
}
