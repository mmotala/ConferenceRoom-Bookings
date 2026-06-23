import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/shared/api/apiClient', () => ({
  apiRequest: vi.fn()
}));

import { apiRequest } from '@/shared/api/apiClient';
import {
  cancelBooking,
  cancelRecurringSeries,
  createBooking,
  createRecurringBooking,
  getBookingCalendar,
  getBookings,
  quickBookRoom
} from './bookingsApi';

const mockedApiRequest = vi.mocked(apiRequest);

describe('bookings api', () => {
  beforeEach(() => {
    mockedApiRequest.mockResolvedValue(undefined);
  });

  it('calls booking endpoints with the expected paths', async () => {
    await getBookings('Active');
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings?status=Active');

    await getBookings();
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings');

    const bookingRequest = {
      roomId: 'r1',
      startTimeUtc: '2030-01-01T10:00:00.000Z',
      endTimeUtc: '2030-01-01T11:00:00.000Z',
      purpose: 'Planning'
    };

    await createBooking(bookingRequest);
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings', {
      method: 'POST',
      body: bookingRequest
    });

    await quickBookRoom({
      startTimeUtc: bookingRequest.startTimeUtc,
      endTimeUtc: bookingRequest.endTimeUtc,
      numberOfPeople: 4,
      purpose: 'Sync'
    });
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings/quick', {
      method: 'POST',
      body: {
        startTimeUtc: bookingRequest.startTimeUtc,
        endTimeUtc: bookingRequest.endTimeUtc,
        numberOfPeople: 4,
        purpose: 'Sync'
      }
    });

    await createRecurringBooking({
      ...bookingRequest,
      recurrenceType: 'Weekly',
      recurrenceUntilUtc: '2030-02-01T10:00:00.000Z'
    });
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings/recurring', {
      method: 'POST',
      body: {
        ...bookingRequest,
        recurrenceType: 'Weekly',
        recurrenceUntilUtc: '2030-02-01T10:00:00.000Z'
      }
    });
  });

  it('calls cancel endpoints', async () => {
    await cancelBooking('b1');
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings/b1/cancel', {
      method: 'POST'
    });

    await cancelRecurringSeries('series-1');
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/bookings/series/series-1/cancel', {
      method: 'POST'
    });
  });

  it('builds the calendar query string', async () => {
    await getBookingCalendar({
      fromUtc: '2030-01-01T00:00:00.000Z',
      toUtc: '2030-02-01T00:00:00.000Z',
      roomId: 'r1',
      includeCancelled: true
    });

    const calls = mockedApiRequest.mock.calls;
    const [path] = calls[calls.length - 1]!;
    const url = new URL(path, 'http://localhost');

    expect(url.pathname).toBe('/api/bookings/calendar');
    expect(url.searchParams.get('fromUtc')).toBe('2030-01-01T00:00:00.000Z');
    expect(url.searchParams.get('toUtc')).toBe('2030-02-01T00:00:00.000Z');
    expect(url.searchParams.get('roomId')).toBe('r1');
    expect(url.searchParams.get('includeCancelled')).toBe('true');
  });
});
