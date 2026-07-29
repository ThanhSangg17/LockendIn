using System;
using System.Collections.Generic;
using System.Linq;

namespace LockedIn.BusinessObject.Common;

public class FixedTimeSlot
{
    public string SlotCode { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public static class FixedTimeSlotCatalog
{
    public static readonly IReadOnlyList<FixedTimeSlot> Slots = new List<FixedTimeSlot>
    {
        new FixedTimeSlot { SlotCode = "SLOT_1", DisplayName = "08:00–09:00", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 0, 0) },
        new FixedTimeSlot { SlotCode = "SLOT_2", DisplayName = "09:30–10:30", StartTime = new TimeSpan(9, 30, 0), EndTime = new TimeSpan(10, 30, 0) },
        new FixedTimeSlot { SlotCode = "SLOT_3", DisplayName = "13:00–14:00", StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) },
        new FixedTimeSlot { SlotCode = "SLOT_4", DisplayName = "14:30–15:30", StartTime = new TimeSpan(14, 30, 0), EndTime = new TimeSpan(15, 30, 0) },
        new FixedTimeSlot { SlotCode = "SLOT_5", DisplayName = "18:00–19:00", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(19, 0, 0) },
        new FixedTimeSlot { SlotCode = "SLOT_6", DisplayName = "19:30–20:30", StartTime = new TimeSpan(19, 30, 0), EndTime = new TimeSpan(20, 30, 0) },
    };

    public static FixedTimeSlot? GetByCode(string slotCode)
    {
        return Slots.FirstOrDefault(s => s.SlotCode.Equals(slotCode, StringComparison.OrdinalIgnoreCase));
    }
}
