<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { addHours, addWeeks } from 'date-fns';

import { createRecurringBooking } from '@/features/bookings/api/bookingsApi';
import type { RecurrenceType } from '@/features/bookings/types/booking';
import type { Room } from '@/features/rooms/types/room';
import {
  toDateTimeLocalValue,
  toUtcIsoString
} from '@/shared/utils/dateUtils';
import { validateRequired } from '@/shared/utils/validation';

defineProps<{
  rooms: Room[];
}>();

const emit = defineEmits<{
  created: [];
  error: [message: string];
}>();

const isLoading = ref(false);
const minDateTime = computed(() => toDateTimeLocalValue(new Date()));

function getDefaultStartTime(): Date {
  const date = new Date();

  date.setHours(date.getHours() + 1);
  date.setMinutes(0);
  date.setSeconds(0);
  date.setMilliseconds(0);

  return date;
}

const defaultStartTime = getDefaultStartTime();
const defaultEndTime = addHours(defaultStartTime, 1);

const form = reactive({
  roomId: '',
  startTime: toDateTimeLocalValue(defaultStartTime),
  endTime: toDateTimeLocalValue(defaultEndTime),
  purpose: '',
  recurrenceType: 'Weekly' as RecurrenceType,
  recurrenceUntil: toDateTimeLocalValue(addWeeks(defaultStartTime, 4))
});

const fieldErrors = reactive({
  roomId: '',
  startTime: '',
  endTime: '',
  recurrenceUntil: '',
  purpose: ''
});

function clearErrors() {
  fieldErrors.roomId = '';
  fieldErrors.startTime = '';
  fieldErrors.endTime = '';
  fieldErrors.recurrenceUntil = '';
  fieldErrors.purpose = '';
}

function toDate(value: string): Date | null {
  if (!value) {
    return null;
  }

  const date = new Date(value);

  return Number.isNaN(date.getTime()) ? null : date;
}

function onStartTimeChanged() {
  const startTime = toDate(form.startTime);
  const endTime = toDate(form.endTime);

  if (startTime && (!endTime || endTime <= startTime)) {
    form.endTime = toDateTimeLocalValue(addHours(startTime, 1));
  }
}

function validateForm() {
  clearErrors();

  fieldErrors.roomId = validateRequired(form.roomId, 'Room') ?? '';
  fieldErrors.purpose = validateRequired(form.purpose, 'Purpose') ?? '';

  const startTime = toDate(form.startTime);
  const endTime = toDate(form.endTime);
  const recurrenceUntil = toDate(form.recurrenceUntil);

  if (!startTime) {
    fieldErrors.startTime = 'Start time is required.';
  }

  if (!endTime) {
    fieldErrors.endTime = 'End time is required.';
  }

  if (startTime && startTime <= new Date()) {
    fieldErrors.startTime = 'Start time must be in the future.';
  }

  if (startTime && endTime && endTime <= startTime) {
    fieldErrors.endTime = 'End time must be after start time.';
  }

  if (!recurrenceUntil) {
    fieldErrors.recurrenceUntil = 'Recurring end date is required.';
  }

  if (startTime && recurrenceUntil && recurrenceUntil <= startTime) {
    fieldErrors.recurrenceUntil = 'Recurring end date must be after the first booking.';
  }

  return !Object.values(fieldErrors).some(Boolean);
}

async function submit() {
  if (!validateForm()) {
    emit('error', 'Please fix the highlighted fields.');
    return;
  }

  isLoading.value = true;

  try {
    await createRecurringBooking({
      roomId: form.roomId,
      startTimeUtc: toUtcIsoString(form.startTime),
      endTimeUtc: toUtcIsoString(form.endTime),
      purpose: form.purpose.trim(),
      recurrenceType: form.recurrenceType,
      recurrenceUntilUtc: toUtcIsoString(form.recurrenceUntil)
    });

    form.purpose = '';
    clearErrors();

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

    <div class="form-row" :class="{ invalid: fieldErrors.roomId }">
      <label for="recurring-room">Room</label>
      <select id="recurring-room" v-model="form.roomId" :aria-invalid="Boolean(fieldErrors.roomId)">
        <option value="">Select room</option>
        <option v-for="room in rooms" :key="room.id" :value="room.id">
          {{ room.name }} — {{ room.capacity }} people
        </option>
      </select>
      <p v-if="fieldErrors.roomId" class="field-error">{{ fieldErrors.roomId }}</p>
    </div>

    <div class="grid two">
      <div class="form-row" :class="{ invalid: fieldErrors.startTime }">
        <label for="recurring-start">Start</label>
        <input
          id="recurring-start"
          v-model="form.startTime"
          type="datetime-local"
          :min="minDateTime"
          step="900"
          :aria-invalid="Boolean(fieldErrors.startTime)"
          @change="onStartTimeChanged"
        />
        <p v-if="fieldErrors.startTime" class="field-error">{{ fieldErrors.startTime }}</p>
      </div>

      <div class="form-row" :class="{ invalid: fieldErrors.endTime }">
        <label for="recurring-end">End</label>
        <input
          id="recurring-end"
          v-model="form.endTime"
          type="datetime-local"
          :min="form.startTime || minDateTime"
          step="900"
          :aria-invalid="Boolean(fieldErrors.endTime)"
        />
        <p v-if="fieldErrors.endTime" class="field-error">{{ fieldErrors.endTime }}</p>
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

      <div class="form-row" :class="{ invalid: fieldErrors.recurrenceUntil }">
        <label for="recurring-until">Until</label>
        <input
          id="recurring-until"
          v-model="form.recurrenceUntil"
          type="datetime-local"
          :min="form.startTime || minDateTime"
          step="900"
          :aria-invalid="Boolean(fieldErrors.recurrenceUntil)"
        />
        <p v-if="fieldErrors.recurrenceUntil" class="field-error">{{ fieldErrors.recurrenceUntil }}</p>
      </div>
    </div>

    <div class="form-row" :class="{ invalid: fieldErrors.purpose }">
      <label for="recurring-purpose">Purpose</label>
      <input
        id="recurring-purpose"
        v-model="form.purpose"
        type="text"
        placeholder="Weekly team sync"
        :aria-invalid="Boolean(fieldErrors.purpose)"
      />
      <p v-if="fieldErrors.purpose" class="field-error">{{ fieldErrors.purpose }}</p>
    </div>

    <button class="secondary-button" :disabled="isLoading" @click="submit">
      {{ isLoading ? 'Creating...' : 'Create recurring booking' }}
    </button>
  </section>
</template>
