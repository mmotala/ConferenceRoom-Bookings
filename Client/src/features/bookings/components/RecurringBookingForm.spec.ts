import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const bookingsApi = vi.hoisted(() => ({
  createRecurringBooking: vi.fn()
}));

vi.mock('@/features/bookings/api/bookingsApi', () => bookingsApi);

import RecurringBookingForm from './RecurringBookingForm.vue';

const room = {
  id: 'r1',
  name: 'Boardroom',
  capacity: 12,
  location: 'First Floor'
};

describe('RecurringBookingForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    bookingsApi.createRecurringBooking.mockResolvedValue({});
  });

  it('validates and submits recurring bookings', async () => {
    const wrapper = mount(RecurringBookingForm, {
      props: {
        rooms: [room]
      }
    });

    await wrapper.get('button').trigger('click');

    expect(wrapper.emitted('error')).toEqual([['Please fix the highlighted fields.']]);
    expect(wrapper.text()).toContain('Room is required.');

    await wrapper.get('select').setValue('r1');
    await wrapper.get('input[placeholder="Weekly team sync"]').setValue('Leadership sync');
    await wrapper.get('button').trigger('click');
    await vi.dynamicImportSettled();

    expect(bookingsApi.createRecurringBooking).toHaveBeenCalledWith(
      expect.objectContaining({
        roomId: 'r1',
        purpose: 'Leadership sync',
        recurrenceType: 'Weekly'
      })
    );
    expect(wrapper.emitted('created')).toHaveLength(1);
  });
});
