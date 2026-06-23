<script setup lang="ts">
import { reactive, ref } from 'vue';
import {VueDatePicker} from '@vuepic/vue-datepicker';
import { addHours, addWeeks } from 'date-fns';

import { createRecurringBooking } from '@/features/bookings/api/bookingsApi';
import type { RecurrenceType } from '@/features/bookings/types/booking';
import type { Room } from '@/features/rooms/types/room';
import { validateRequired } from '@/shared/utils/validation';

defineProps<{
  rooms: Room[];
}>();

const emit = defineEmits<{
  created: [];
  error: [message: string];
}>();

const isLoading = ref(false);

const form = reactive({
  roomId: '',
  startTime: new Date(),
  endTime: addHours(new Date(), 1),
  purpose: '',
  recurrenceType: 'Weekly' as RecurrenceType,
  recurrenceUntil: addWeeks(new Date(), 4)
});

async function submit() {
  const errors = [
    validateRequired(form.roomId, 'Room'),
    validateRequired(form.purpose, 'Purpose')
  ].filter(Boolean);

  if (errors.length > 0) {
    emit('error', errors[0]!);
    return;
  }

  if (form.endTime <= form.startTime) {
    emit('error', 'End time must be after start time');
    return;
  }

  if (form.recurrenceUntil <= form.startTime) {
    emit('error', 'Recurring end date must be after the first booking');
    return;
  }

  isLoading.value = true;

  try {
    await createRecurringBooking({
      roomId: form.roomId,
      startTimeUtc: form.startTime.toISOString(),
      endTimeUtc: form.endTime.toISOString(),
      purpose: form.purpose,
      recurrenceType: form.recurrenceType,
      recurrenceUntilUtc: form.recurrenceUntil.toISOString()
    });

    form.purpose = '';

    emit('created');
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Failed to create recurring booking');
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel">
    <div>
      <p class="eyebrow">Recurring Booking</p>
      <h2>Create recurring event</h2>
    </div>

    <div class="form-row">
      <label>Room</label>
      <select v-model="form.roomId">
        <option value="">Select room</option>
        <option v-for="room in rooms" :key="room.id" :value="room.id">
          {{ room.name }} — {{ room.capacity }} people
        </option>
      </select>
    </div>

    <div class="grid two">
      <div class="form-row">
        <label>Start</label>
        <VueDatePicker v-model="form.startTime" />
      </div>

      <div class="form-row">
        <label>End</label>
        <VueDatePicker v-model="form.endTime" />
      </div>
    </div>

    <div class="grid two">
      <div class="form-row">
        <label>Repeats</label>
        <select v-model="form.recurrenceType">
          <option value="Daily">Daily</option>
          <option value="Weekly">Weekly</option>
          <option value="Monthly">Monthly</option>
        </select>
      </div>

      <div class="form-row">
        <label>Until</label>
        <VueDatePicker v-model="form.recurrenceUntil" />
      </div>
    </div>

    <div class="form-row">
      <label>Purpose</label>
      <input v-model="form.purpose" type="text" placeholder="Weekly team sync" />
    </div>

    <button class="secondary-button" :disabled="isLoading" @click="submit">
      {{ isLoading ? 'Creating...' : 'Create recurring booking' }}
    </button>
  </section>
</template>
