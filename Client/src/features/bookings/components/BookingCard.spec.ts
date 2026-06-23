import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import BookingCard from './BookingCard.vue';

const activeBooking = {
  id: 'b1',
  roomId: 'r1',
  roomName: 'Boardroom',
  userId: 'u1',
  userName: 'Ada Admin',
  startTimeUtc: '2030-01-10T09:00:00.000Z',
  endTimeUtc: '2030-01-10T10:00:00.000Z',
  purpose: 'Roadmap planning',
  status: 'Active' as const,
  recurringBookingSeriesId: 'series-1'
};

describe('BookingCard', () => {
  it('renders booking details and emits edit and cancel actions', async () => {
    const wrapper = mount(BookingCard, {
      props: {
        booking: activeBooking
      }
    });

    expect(wrapper.text()).toContain('Roadmap planning');
    expect(wrapper.text()).toContain('Boardroom');
    expect(wrapper.text()).toContain('Active');

    const buttons = wrapper.findAll('button');
    await buttons[0]!.trigger('click');
    await buttons[1]!.trigger('click');
    await buttons[2]!.trigger('click');

    expect(wrapper.emitted('edit')).toEqual([[activeBooking]]);
    expect(wrapper.emitted('cancel')).toEqual([['b1']]);
    expect(wrapper.emitted('cancelSeries')).toEqual([['series-1']]);
  });

  it('hides cancel actions for cancelled bookings', () => {
    const wrapper = mount(BookingCard, {
      props: {
        booking: {
          ...activeBooking,
          status: 'Cancelled' as const,
          recurringBookingSeriesId: null
        }
      }
    });

    expect(wrapper.find('button').exists()).toBe(false);
  });
});
