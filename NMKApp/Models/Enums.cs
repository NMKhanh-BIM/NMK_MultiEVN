namespace NMKApp.Models;

public enum TaskStatus
{
    New = 0,
    Accepted = 1,
    Started = 2,
    Completed = 3,
    Checked = 4,
    Rejected = 5,
    Cancelled = 6
}

public enum LeaveType
{
    AnnualLeave = 1,
    SickLeave = 2,
    PersonalLeave = 3,
    UnpaidLeave = 4,
    Other = 5
}

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum HalfDaySlot
{
    Morning = 1,
    Afternoon = 2
}

public enum NotifyType
{
    General = 0,
    Task = 1,
    Leave = 2,
    System = 3
}

public enum EntityType
{
    Unknown = 0,
    Task = 1,
    Leave = 2,
    Project = 3,
    User = 4
}

public enum EventType
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
    StatusChanged = 3,
    Assigned = 4,
    Commented = 5
}

public enum AttendanceStatus
{
    Unknown = 0,
    Present = 1,
    Absent = 2,
    OnLeave = 3,
    HalfDay = 4
}

public enum EmploymentStatus
{
    Active = 1,
    Inactive = 2,
    Terminated = 3
}

public enum AvailabilityStatus
{
    Available = 0,
    Busy = 1,
    Away = 2,
    DoNotDisturb = 3
}

public enum MessageType
{
    Text = 1,
    File = 2,
    System = 3,
    Image = 4
}

public enum NotifyPriority
{
    Normal = 0,
    High = 1,
    Urgent = 2
}
