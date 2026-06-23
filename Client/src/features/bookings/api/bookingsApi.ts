import { apiRequest } from '@/shared/api/apiClient';
import type {
  Booking,
  BookingStatus,
  CalendarBooking,
  CreateBookingRequest,
  CreateRecurringBookingRequest,
  QuickBookingRequest
} from '@/features/bookings/types/booking';

export function getBookings(status?: BookingStatus): Promise<Booking[]> {
  const query = status ? `?status=${status}` : '';

  return apiRequest<Booking[]>(`/api/bookings${query}`);
}

export function createBooking(request: CreateBookingRequest): Promise<Booking> {
  return apiRequest<Booking>('/api/bookings', {
    method: 'POST',
    body: request
  });
}

export function quickBookRoom(request: QuickBookingRequest): Promise<Booking> {
  return apiRequest<Booking>('/api/bookings/quick', {
    method: 'POST',
    body: request
  });
}

export function createRecurringBooking(
  request: CreateRecurringBookingRequest
): Promise<unknown> {
  return apiRequest<unknown>('/api/bookings/recurring', {
    method: 'POST',
    body: request
  });
}

export function cancelBooking(bookingId: string): Promise<void> {
  return apiRequest<void>(`/api/bookings/${bookingId}/cancel`, {
    method: 'POST'
  });
}

export function cancelRecurringSeries(seriesId: string): Promise<void> {
  return apiRequest<void>(`/api/bookings/series/${seriesId}/cancel`, {
    method: 'POST'
  });
}

export function getBookingCalendar(params: {
  fromUtc: string;
  toUtc: string;
  roomId?: string;
  includeCancelled?: boolean;
}): Promise<CalendarBooking[]> {
  const searchParams = new URLSearchParams({
    fromUtc: params.fromUtc,
    toUtc: params.toUtc,
    includeCancelled: String(params.includeCancelled ?? false)
  });

  if (params.roomId) {
    searchParams.set('roomId', params.roomId);
  }

  return apiRequest<CalendarBooking[]>(
    `/api/bookings/calendar?${searchParams.toString()}`
  );
}
