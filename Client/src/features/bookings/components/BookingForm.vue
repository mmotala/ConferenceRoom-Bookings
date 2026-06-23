<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';

import { createBooking, updateBooking } from '@/features/bookings/api/bookingsApi';
import type { Booking } from '@/features/bookings/types/booking';
import type { Room } from '@/features/rooms/types/room';
import {
  toDateTimeLocalValue,
  toUtcIsoString
} from '@/shared/utils/dateUtils';
import {
  validateDateRange,
  validateRequired
} from '@/shared/utils/validation';

const props = defineProps<{
  rooms: Room[];
  booking?: Booking | null;
}>();

const emit = defineEmits<{
  created: [booking: Booking];
  updated: [booking: Booking];
  cancelEdit: [];
  error: [message: string];
}>();

const isLoading = ref(false);
const minDateTime = computed(() => toDateTimeLocalValue(new Date()));
const isEditing = computed(() => Boolean(props.booking));

function getDefaultStartTime(): Date {
  const date = new Date();

  date.setHours(date.getHours() + 1);
  date.setMinutes(0);
  date.setSeconds(0);
  date.setMilliseconds(0);

  return date;
}

function addHours(date: Date, hours: number): Date {
  const newDate = new Date(date);
  newDate.setHours(newDate.getHours() + hours);
  return newDate;
}

const defaultStartTime = getDefaultStartTime();
const defaultEndTime = addHours(defaultStartTime, 1);

const form = reactive({
  roomId: '',
  startTime: toDateTimeLocalValue(defaultStartTime),
  endTime: toDateTimeLocalValue(defaultEndTime),
  purpose: ''
});

const fieldErrors = reactive({
  roomId: '',
  startTime: '',
  endTime: '',
  purpose: ''
});

watch(
  () => props.booking,
  booking => {
    if (!booking) {
      resetForm();
      return;
    }

    form.roomId = booking.roomId;
    form.startTime = toDateTimeLocalValue(new Date(booking.startTimeUtc));
    form.endTime = toDateTimeLocalValue(new Date(booking.endTimeUtc));
    form.purpose = booking.purpose;
    clearErrors();
  },
  { immediate: true }
);

function resetForm() {
  form.roomId = '';
  form.startTime = toDateTimeLocalValue(getDefaultStartTime());
  form.endTime = toDateTimeLocalValue(addHours(new Date(form.startTime), 1));
  form.purpose = '';
  clearErrors();
}

function clearErrors() {
  fieldErrors.roomId = '';
  fieldErrors.startTime = '';
  fieldErrors.endTime = '';
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
  const dateRangeError = validateDateRange(startTime, endTime);

  if (dateRangeError) {
    if (!startTime || dateRangeError.includes('Start time')) {
      fieldErrors.startTime = dateRangeError;
    } else {
      fieldErrors.endTime = dateRangeError;
    }
  }

  return !Object.values(fieldErrors).some(Boolean);
}

async function submit() {
  if (!validateForm()) {
    emit('error', 'Please fix the highlighted fields.');
    return;
  }

  if (!form.startTime || !form.endTime) {
    fieldErrors.startTime = 'Start time is required.';
    fieldErrors.endTime = 'End time is required.';
    emit('error', 'Please fix the highlighted fields.');
    return;
  }

  isLoading.value = true;

  try {
    const request = {
      roomId: form.roomId,
      startTimeUtc: toUtcIsoString(form.startTime),
      endTimeUtc: toUtcIsoString(form.endTime),
      purpose: form.purpose.trim()
    };

    if (props.booking) {
      const booking = await updateBooking(props.booking.id ?? props.booking.bookingId!, request);
      emit('updated', booking);
      return;
    }

    const booking = await createBooking(request);

    form.purpose = '';
    emit('created', booking);
  } catch (error) {
    emit(
      'error',
      error instanceof Error
        ? error.message
        : 'Could not create booking. Please try again.'
    );
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel">
    <div>
      <p class="eyebrow">Manual Booking</p>
      <h2>{{ isEditing ? 'Update booking' : 'Create booking' }}</h2>
      <p class="muted">
        Choose a room, date, time and purpose.
      </p>
    </div>

    <div class="form-row" :class="{ invalid: fieldErrors.roomId }">
      <label for="booking-room">Room</label>
      <select id="booking-room" v-model="form.roomId" :aria-invalid="Boolean(fieldErrors.roomId)">
        <option value="">Select room</option>
        <option
          v-for="room in rooms"
          :key="room.id"
          :value="room.id"
        >
          {{ room.name }} — {{ room.capacity }} people
        </option>
      </select>
      <p v-if="fieldErrors.roomId" class="field-error">{{ fieldErrors.roomId }}</p>
    </div>

    <div class="grid two">
      <div class="form-row" :class="{ invalid: fieldErrors.startTime }">
        <label for="booking-start">Start time</label>
        <input
          id="booking-start"
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
        <label for="booking-end">End time</label>
        <input
          id="booking-end"
          v-model="form.endTime"
          type="datetime-local"
          :min="form.startTime || minDateTime"
          step="900"
          :aria-invalid="Boolean(fieldErrors.endTime)"
        />
        <p v-if="fieldErrors.endTime" class="field-error">{{ fieldErrors.endTime }}</p>
      </div>
    </div>

    <div class="form-row" :class="{ invalid: fieldErrors.purpose }">
      <label for="booking-purpose">Purpose</label>
      <input
        id="booking-purpose"
        v-model="form.purpose"
        type="text"
        maxlength="250"
        placeholder="Project planning"
        :aria-invalid="Boolean(fieldErrors.purpose)"
      />
      <p v-if="fieldErrors.purpose" class="field-error">{{ fieldErrors.purpose }}</p>
    </div>

    <div class="form-actions">
      <button
        class="primary-button"
        :disabled="isLoading"
        @click="submit"
      >
        {{ isLoading ? 'Saving...' : isEditing ? 'Update booking' : 'Create booking' }}
      </button>

      <button
        v-if="isEditing"
        class="ghost-button"
        type="button"
        @click="emit('cancelEdit')"
      >
        Cancel edit
      </button>
    </div>
  </section>
</template>
