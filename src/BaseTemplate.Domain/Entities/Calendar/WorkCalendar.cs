

// ═══════════════════════════════════════════════════════════════════
// src/BaseTemplate.Domain/Entities/Calendar/WorkCalendar.cs
// ═══════════════════════════════════════════════════════════════════
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Calendar;

public class WorkCalendar : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public int WorkingDays { get; private set; }
    public int WorkingHours { get; private set; }

    private WorkCalendar() { }

    public WorkCalendar(Guid organizationId, int month, int year,
                        int workingDays, int workingHours)
    {
        OrganizationId = organizationId;
        Month = month;
        Year = year;
        WorkingDays = workingDays;
        WorkingHours = workingHours;
    }

    public void Update(int workingDays, int workingHours)
    {
        WorkingDays = workingDays;
        WorkingHours = workingHours;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
