import { defineComponent, h } from 'vue';
import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const bookingsApi = vi.hoisted(() => ({
  quickBookRoom: vi.fn()
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

import QuickBookingForm from './QuickBookingForm.vue';

const booking = {
  id: 'b1',
  roomId: 'r1',
  roomName: 'Boardroom',
  userId: 'u1',
  userName: 'Ada Admin',
  startTimeUtc: '2030-01-10T09:00:00.000Z',
  endTimeUtc: '2030-01-10T10:00:00.000Z',
  purpose: 'Team sync',
  status: 'Active' as const,
  recurringBookingSeriesId: null
};

describe('QuickBookingForm', () => {
  beforeEach(() => {
    vi.setSystemTime(new Date('2030-01-01T08:00:00.000Z'));
    bookingsApi.quickBookRoom.mockResolvedValue(booking);
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  it('validates and submits quick bookings', async () => {
    const wrapper = mount(QuickBookingForm);

    await wrapper.get('button').trigger('click');

    expect(wrapper.emitted('error')).toEqual([['Purpose is required.']]);

    await wrapper.get('input[type="number"]').setValue(5);
    await wrapper.get('input[placeholder="Team sync"]').setValue('Team sync');
    await wrapper.get('button').trigger('click');
    await vi.dynamicImportSettled();

    expect(bookingsApi.quickBookRoom).toHaveBeenCalledWith(
      expect.objectContaining({
        numberOfPeople: 5,
        purpose: 'Team sync'
      })
    );
    expect(wrapper.emitted('created')?.[0]?.[0]).toEqual(booking);
  });
});
