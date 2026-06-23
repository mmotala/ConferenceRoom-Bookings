import { defineComponent, h } from 'vue';
import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const bookingsApi = vi.hoisted(() => ({
  getBookingCalendar: vi.fn()
}));

vi.mock('@/features/bookings/api/bookingsApi', () => bookingsApi);

vi.mock('@fullcalendar/vue3', () => ({
  default: defineComponent({
    name: 'FullCalendar',
    props: {
      options: {
        type: Object,
        required: true
      }
    },
    setup(props) {
      return () =>
        h(
          'div',
          { class: 'calendar-stub' },
          (props.options as { events?: Array<{ title: string }> }).events?.map(event =>
            h('span', event.title)
          )
        );
    }
  })
}));

vi.mock('@fullcalendar/daygrid', () => ({ default: {} }));
vi.mock('@fullcalendar/timegrid', () => ({ default: {} }));
vi.mock('@fullcalendar/interaction', () => ({ default: {} }));

import BookingCalendar from './BookingCalendar.vue';

const room = {
  id: 'r1',
  name: 'Boardroom',
  capacity: 12,
  location: 'First Floor'
};

describe('BookingCalendar', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    bookingsApi.getBookingCalendar.mockResolvedValue([
      {
        bookingId: 'calendar-1',
        roomId: 'r1',
        roomName: 'Boardroom',
        userId: 'u1',
        userName: 'Ada Admin',
        startTimeUtc: '2030-01-10T09:00:00.000Z',
        endTimeUtc: '2030-01-10T10:00:00.000Z',
        purpose: 'Roadmap planning',
        status: 'Active',
        recurringBookingSeriesId: null
      }
    ]);
  });

  it('loads calendar bookings and reloads when filters change', async () => {
    const wrapper = mount(BookingCalendar, {
      props: {
        rooms: [room]
      }
    });
    await vi.dynamicImportSettled();

    expect(bookingsApi.getBookingCalendar).toHaveBeenCalledWith(
      expect.objectContaining({
        includeCancelled: false
      })
    );
    expect(wrapper.text()).toContain('Boardroom - Roadmap planning');

    await wrapper.get('select').setValue('r1');
    await vi.dynamicImportSettled();

    expect(bookingsApi.getBookingCalendar).toHaveBeenLastCalledWith(
      expect.objectContaining({
        roomId: 'r1'
      })
    );
  });
});
