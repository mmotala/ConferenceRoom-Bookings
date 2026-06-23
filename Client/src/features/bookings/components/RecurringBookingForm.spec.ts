import { defineComponent, h } from 'vue';
import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const bookingsApi = vi.hoisted(() => ({
  createRecurringBooking: vi.fn()
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

    expect(wrapper.emitted('error')).toEqual([['Room is required.']]);

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
