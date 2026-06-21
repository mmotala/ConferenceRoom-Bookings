public static class RoomErrors
{
    public static readonly Error NotFound = new(
        "Room.NotFound",
        "The requested room was not found.",
        ErrorType.NotFound);

    public static readonly Error AlreadyExists = new(
        "Room.AlreadyExists",
        "A room with this name already exists.",
        ErrorType.Conflict);
}