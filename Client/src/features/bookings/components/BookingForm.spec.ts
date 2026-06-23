import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const bookingsApi = vi.hoisted(() => ({
  createBooking: vi.fn(),
  updateBooking: vi.fn()
}));

vi.mock('@/features/bookings/api/bookingsApi', () => bookingsApi);

import BookingForm from './BookingForm.vue';

const room = {
  id: 'r1',
  name: 'Boardroom',
  capacity: 12,
  location: 'First Floor'
};

const booking = {
  id: 'b1',
  roomId: 'r1',
  roomName: 'Boardroom',
  userId: 'u1',
  userName: 'Ada Admin',
  startTimeUtc: '2030-01-10T09:00:00.000Z',
  endTimeUtc: '2030-01-10T10:00:00.000Z',
  purpose: 'Project planning',
  status: 'Active' as const,
  recurringBookingSeriesId: null
};

describe('BookingForm', () => {
  beforeEach(() => {
    vi.setSystemTime(new Date('2030-01-01T08:00:00.000Z'));
    bookingsApi.createBooking.mockResolvedValue(booking);
    bookingsApi.updateBooking.mockResolvedValue(booking);
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  it('validates and submits manual bookings', async () => {
    const wrapper = mount(BookingForm, {
      props: {
        rooms: [room]
      }
    });

    await wrapper.get('button').trigger('click');

    expect(wrapper.emitted('error')).toEqual([['Please fix the highlighted fields.']]);
    expect(wrapper.text()).toContain('Room is required.');

    await wrapper.get('select').setValue('r1');
    await wrapper.get('input[placeholder="Project planning"]').setValue('Project planning');
    await wrapper.get('button').trigger('click');
    await vi.dynamicImportSettled();

    expect(bookingsApi.createBooking).toHaveBeenCalledWith(
      expect.objectContaining({
        roomId: 'r1',
        purpose: 'Project planning'
      })
    );
    expect(wrapper.emitted('created')?.[0]?.[0]).toEqual(booking);
  });

  it('submits updates for an existing booking', async () => {
    const wrapper = mount(BookingForm, {
      props: {
        rooms: [room],
        booking
      }
    });

    await wrapper.get('input[placeholder="Project planning"]').setValue('Updated planning');
    await wrapper.get('button.primary-button').trigger('click');
    await vi.dynamicImportSettled();

    expect(bookingsApi.updateBooking).toHaveBeenCalledWith(
      'b1',
      expect.objectContaining({
        roomId: 'r1',
        purpose: 'Updated planning'
      })
    );
    expect(wrapper.emitted('updated')?.[0]?.[0]).toEqual(booking);
  });
});
