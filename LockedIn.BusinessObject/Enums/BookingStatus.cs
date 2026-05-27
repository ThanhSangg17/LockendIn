namespace LockedIn.BusinessObject.Enums;

public enum BookingStatus
{
    PendingPayment = 1,
    PaidPendingAcceptance = 2,
    Active = 3,
    CompletedPendingSettlement = 4,
    Settled = 5,
    Cancelled = 6,
    Refunded = 7,
    Disputed = 10
}
