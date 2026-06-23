export type BookingStatus = 'Active' | 'Cancelled';

export type RecurrenceType = 'Daily' | 'Weekly' | 'Monthly';

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
  status: BookingStatus;
  recurringBookingSeriesId?: string | null;
};

export type CreateBookingRequest = {
  roomId: string;
  startTimeUtc: string;
  endTimeUtc: string;
  purpose: string;
};

export type UpdateBookingRequest = CreateBookingRequest;

export type QuickBookingRequest = {
  startTimeUtc: string;
  endTimeUtc: string;
  numberOfPeople: number;
  purpose: string;
};

export type CreateRecurringBookingRequest = {
  roomId: string;
  startTimeUtc: string;
  endTimeUtc: string;
  purpose: string;
  recurrenceType: RecurrenceType;
  recurrenceUntilUtc: string;
};

export type CalendarBooking = {
  bookingId: string;
  roomId: string;
  roomName: string;
  userId: string;
  userName: string;
  startTimeUtc: string;
  endTimeUtc: string;
  purpose: string;
  status: BookingStatus;
  recurringBookingSeriesId: string | null;
};
