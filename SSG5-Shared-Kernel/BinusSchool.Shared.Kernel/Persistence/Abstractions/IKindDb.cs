namespace BinusSchool.Persistence.Abstractions
{
    public interface IKindDb {}
    public interface IAttendanceDb : IKindDb {}
    public interface IDocumentDb : IKindDb {}
    public interface IEmployeeDb : IKindDb {}
    public interface ISchedulingDb : IKindDb {}
    public interface ISchoolDb : IKindDb {}
    public interface IScoringDb : IKindDb {}
    public interface IStudentDb : IKindDb {}
    public interface ITeachingDb : IKindDb {}
    public interface IUserDb : IKindDb {}
    public interface IWorkflowDb : IKindDb {}
}