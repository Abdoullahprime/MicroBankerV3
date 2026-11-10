// src/BaseTemplate.Domain/Entities/Calendar/PublicHoliday.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Calendar;

public enum HolidayType { National = 0, Local = 1 }

public class PublicHoliday : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = "";
    public DateTime HolidayDate { get; private set; }
    public HolidayType HolidayType { get; private set; }
    public bool IsRecurring { get; private set; }
    public string Description { get; private set; } = "";
    public bool IsActive { get; private set; }

    private PublicHoliday() { }

    public PublicHoliday(Guid orgId, string name, DateTime date, HolidayType type)
    {
        OrganizationId = orgId;
        Name           = name;
        HolidayDate    = date.Date;
        HolidayType    = type;
        IsActive       = true;
    }

    public void Update(string name, DateTime date, HolidayType type,
        bool isRecurring, string description, bool isActive)
    {
        Name        = name;
        HolidayDate = date.Date;
        HolidayType = type;
        IsRecurring = isRecurring;
        Description = description ?? "";
        IsActive    = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
