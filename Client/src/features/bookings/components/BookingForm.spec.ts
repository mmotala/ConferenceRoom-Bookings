import { defineComponent, h } from 'vue';
import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const bookingsApi = vi.hoisted(() => ({
  createBooking: vi.fn()
}));

vi.mock('@/features/bookings/api/bookingsApi', () => bookingsApi);

vi.mock('@vuepic/vue-datepicker', () => ({
  VueDatePicker: defineComponent({
    name: 'VueDatePicker',
    props: {
      modelValue: {
        type: Date,
        default: null
      }
    },
    emits: ['update:modelValue'],
    setup(props, { emit }) {
      return () =>
        h('input', {
          class: 'date-picker-stub',
          value: props.modelValue instanceof Date ? props.modelValue.toISOString() : '',
          onInput: (event: Event) => {
            emit('update:modelValue', new Date((event.target as HTMLInputElement).value));
          }
        });
    }
  })
}));

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

    expect(wrapper.emitted('error')).toEqual([['Room is required.']]);

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
});
