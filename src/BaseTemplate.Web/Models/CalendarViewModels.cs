// src/BaseTemplate.Web/Models/CalendarViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace BaseTemplate.Web.Models;


public class CalendarRowModel
{
    public string EncryptedId { get; set; } = "";
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public int Year { get; set; }
    public int WorkingDays { get; set; }
    public int WorkingHours { get; set; }
}

public class CalendarIndexModel
{
    public List<CalendarRowModel> Calendars { get; set; } = new();
}

public class CalendarFormModel
{
    [Required]
    [Range(1, 12, ErrorMessage = "Select a valid month.")]
    public int Month { get; set; }

    [Required]
    [Range(2000, 2099, ErrorMessage = "Enter a valid year.")]
    public int Year { get; set; }

    [Required]
    [Range(1, 31, ErrorMessage = "Working days must be between 1 and 31.")]
    [Display(Name = "Working Days")]
    public int WorkingDays { get; set; } = 22;

    [Required]
    [Range(1, 248, ErrorMessage = "Working hours must be between 1 and 248.")]
    [Display(Name = "Working Hours")]
    public int WorkingHours { get; set; } = 176;

    public string EncryptedId { get; set; } = ""; // empty = create
}


// ── Public Holiday ────────────────────────────────────────────────────────────

public class PublicHolidayListViewModel
{
    public int?  FilterYear { get; set; }
    public List<PublicHolidayRowModel> Holidays { get; set; } = new();
}

public class PublicHolidayRowModel
{
    public string EncryptedId  { get; set; } = "";
    public string Name         { get; set; } = "";
    public string Date         { get; set; } = "";
    public string DayOfWeek    { get; set; } = "";
    public string Type         { get; set; } = "";
    public string TypeBadge    { get; set; } = "";
    public bool   IsRecurring  { get; set; }
    public bool   IsActive     { get; set; }
    public string Description  { get; set; } = "";
}

public class PublicHolidayFormModel
{
    public string EncryptedId { get; set; } = "";
    public string Name        { get; set; } = "";
    public string Date        { get; set; } = "";
    public int    HolidayType { get; set; }
    public bool   IsRecurring { get; set; }
    public string Description { get; set; } = "";
    public bool   IsActive    { get; set; } = true;
}

