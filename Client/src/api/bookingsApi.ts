import { apiRequest } from '@/api/apiClient';
import type {
  Booking,
  CreateBookingRequest,
  QuickBookingRequest
} from '@/types/booking';

export function getBookings(): Promise<Booking[]> {
  return apiRequest<Booking[]>('/api/bookings');
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

export function cancelBooking(bookingId: string): Promise<void> {
  return apiRequest<void>(`/api/bookings/${bookingId}/cancel`, {
    method: 'POST'
  });
}
