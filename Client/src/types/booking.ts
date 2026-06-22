export type Booking = {
  id: string;
  bookingId?: string;
  roomId: string;
  roomName?: string;
  roomCapacity?: number;
  userId: string;
  userName?: string;
  startTimeUtc: string;
  endTimeUtc: string;
  purpose: string;
  status: string;
};

export type CreateBookingRequest = {
  roomId: string;
  startTimeUtc: string;
  endTimeUtc: string;
  purpose: string;
};

export type QuickBookingRequest = {
  startTimeUtc: string;
  endTimeUtc: string;
  numberOfPeople: number;
  purpose: string;
};
