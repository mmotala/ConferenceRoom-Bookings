<script setup lang="ts">
import { reactive, ref } from 'vue';
import {VueDatePicker} from '@vuepic/vue-datepicker';

import { quickBookRoom } from '@/features/bookings/api/bookingsApi';
import type { Booking } from '@/features/bookings/types/booking';
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
  startTime: defaultStartTime as Date | null,
  endTime: defaultEndTime as Date | null,
  numberOfPeople: 1,
  purpose: ''
});

function onStartTimeChanged(value: Date | null) {
  form.startTime = value;

  if (!value) {
    return;
  }

  if (!form.endTime || form.endTime <= value) {
    form.endTime = addHours(value, 1);
  }
}

function onEndTimeChanged(value: Date | null) {
  form.endTime = value;
}

async function submit() {
  const errors = [
    validateDateRange(form.startTime, form.endTime),
    validatePositiveNumber(Number(form.numberOfPeople), 'Number of people'),
    validateRequired(form.purpose, 'Purpose')
  ].filter(Boolean);

  if (errors.length > 0) {
    emit('error', errors[0]!);
    return;
  }

  if (!form.startTime || !form.endTime) {
    emit('error', 'Please select both a start time and an end time.');
    return;
  }

  isLoading.value = true;

  try {
    const booking = await quickBookRoom({
      startTimeUtc: form.startTime.toISOString(),
      endTimeUtc: form.endTime.toISOString(),
      numberOfPeople: Number(form.numberOfPeople),
      purpose: form.purpose.trim()
    });

    form.purpose = '';
    form.numberOfPeople = 1;

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
      <div class="form-row">
        <label>Start time</label>

        <VueDatePicker
          :model-value="form.startTime"
          @update:model-value="onStartTimeChanged"
          :enable-time-picker="true"
          :time-picker-inline="true"
          :is-24="true"
          :minutes-increment="15"
          :min-date="new Date()"
          :clearable="false"
          :auto-apply="false"
          :teleport="true"
          format="dd MMM yyyy, HH:mm"
          placeholder="Select start date and time"
        />
      </div>

      <div class="form-row">
        <label>End time</label>

        <VueDatePicker
          :model-value="form.endTime"
          @update:model-value="onEndTimeChanged"
          :enable-time-picker="true"
          :time-picker-inline="true"
          :is-24="true"
          :minutes-increment="15"
          :min-date="form.startTime ?? new Date()"
          :clearable="false"
          :auto-apply="false"
          :teleport="true"
          format="dd MMM yyyy, HH:mm"
          placeholder="Select end date and time"
        />
      </div>
    </div>

    <div class="grid two">
      <div class="form-row">
        <label>Number of people</label>
        <input
          v-model.number="form.numberOfPeople"
          type="number"
          min="1"
        />
      </div>

      <div class="form-row">
        <label>Purpose</label>
        <input
          v-model="form.purpose"
          type="text"
          maxlength="250"
          placeholder="Team sync"
        />
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
