public static class BookingErrors
{
    public static readonly Error NotFound = new(
        "Booking.NotFound",
        "The requested booking was not found.",
        ErrorType.NotFound);

    public static readonly Error RoomUnavailable = new(
        "Booking.RoomUnavailable",
        "The selected room is already booked for this time slot.",
        ErrorType.Conflict);

    public static readonly Error CannotCancel = new(
        "Booking.CannotCancel",
        "This booking cannot be cancelled.",
        ErrorType.Validation);

    public static readonly Error CannotUpdateCancelledBooking = new(
        "Booking.CannotUpdateCancelledBooking",
        "A cancelled booking cannot be updated.",
        ErrorType.Validation);

    public static readonly Error NoSuitableRoomAvailable = new(
    "Booking.NoSuitableRoomAvailable",
    "No suitable room is available for the requested time and number of people.",
    ErrorType.NotFound);
}