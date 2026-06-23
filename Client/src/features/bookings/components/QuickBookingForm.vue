<script setup lang="ts">
import { computed, reactive, ref } from 'vue';

import { quickBookRoom } from '@/features/bookings/api/bookingsApi';
import type { Booking } from '@/features/bookings/types/booking';
import {
  toDateTimeLocalValue,
  toUtcIsoString
} from '@/shared/utils/dateUtils';
import {
  validateDateRange,
  validatePositiveNumber,
  validateRequired
} from '@/shared/utils/validation';

const emit = defineEmits<{
  created: [booking: Booking];
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

function addHours(date: Date, hours: number): Date {
  const newDate = new Date(date);
  newDate.setHours(newDate.getHours() + hours);
  return newDate;
}

const defaultStartTime = getDefaultStartTime();
const defaultEndTime = addHours(defaultStartTime, 1);

const form = reactive({
  startTime: toDateTimeLocalValue(defaultStartTime),
  endTime: toDateTimeLocalValue(defaultEndTime),
  numberOfPeople: 1,
  purpose: ''
});

const fieldErrors = reactive({
  startTime: '',
  endTime: '',
  numberOfPeople: '',
  purpose: ''
});

function clearErrors() {
  fieldErrors.startTime = '';
  fieldErrors.endTime = '';
  fieldErrors.numberOfPeople = '';
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

  fieldErrors.numberOfPeople =
    validatePositiveNumber(Number(form.numberOfPeople), 'Number of people') ?? '';
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
    const booking = await quickBookRoom({
      startTimeUtc: toUtcIsoString(form.startTime),
      endTimeUtc: toUtcIsoString(form.endTime),
      numberOfPeople: Number(form.numberOfPeople),
      purpose: form.purpose.trim()
    });

    form.purpose = '';
    form.numberOfPeople = 1;
    clearErrors();

    emit('created', booking);
  } catch (error) {
    emit(
      'error',
      error instanceof Error
        ? error.message
        : 'Could not find an available room. Please try another time.'
    );
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel quick-booking-panel">
    <div>
      <p class="eyebrow">Quick Booking</p>
      <h2>Find me a room</h2>
      <p class="muted">
        Pick a time and number of people. The system will choose the smallest available room.
      </p>
    </div>

    <div class="grid two">
      <div class="form-row" :class="{ invalid: fieldErrors.startTime }">
        <label for="quick-start">Start time</label>
        <input
          id="quick-start"
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
        <label for="quick-end">End time</label>
        <input
          id="quick-end"
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
      <div class="form-row" :class="{ invalid: fieldErrors.numberOfPeople }">
        <label for="quick-people">Number of people</label>
        <input
          id="quick-people"
          v-model.number="form.numberOfPeople"
          type="number"
          min="1"
          :aria-invalid="Boolean(fieldErrors.numberOfPeople)"
        />
        <p v-if="fieldErrors.numberOfPeople" class="field-error">{{ fieldErrors.numberOfPeople }}</p>
      </div>

      <div class="form-row" :class="{ invalid: fieldErrors.purpose }">
        <label for="quick-purpose">Purpose</label>
        <input
          id="quick-purpose"
          v-model="form.purpose"
          type="text"
          maxlength="250"
          placeholder="Team sync"
          :aria-invalid="Boolean(fieldErrors.purpose)"
        />
        <p v-if="fieldErrors.purpose" class="field-error">{{ fieldErrors.purpose }}</p>
      </div>
    </div>

    <button
      class="primary-button"
      :disabled="isLoading"
      @click="submit"
    >
      {{ isLoading ? 'Finding room...' : 'Quick book' }}
    </button>
  </section>
</template>
