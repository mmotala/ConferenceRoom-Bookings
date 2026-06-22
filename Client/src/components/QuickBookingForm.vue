<script setup lang="ts">
import { reactive, ref } from 'vue';
import { quickBookRoom } from '@/api/bookingsApi';
import type { Booking } from '@/types/booking';
import { getDefaultStartTime, getDefaultEndTime, toUtcIsoString } from '@/utils/dateUtils';

const emit = defineEmits<{
  created: [booking: Booking];
  error: [message: string];
}>();

const isLoading = ref(false);

const form = reactive({
  startTime: getDefaultStartTime(),
  endTime: getDefaultEndTime(),
  numberOfPeople: 1,
  purpose: ''
});

async function submit() {
  if (!form.startTime || !form.endTime || !form.purpose) {
    emit('error', 'Please complete all quick booking fields');
    return;
  }

  isLoading.value = true;

  try {
    const booking = await quickBookRoom({
      startTimeUtc: toUtcIsoString(form.startTime),
      endTimeUtc: toUtcIsoString(form.endTime),
      numberOfPeople: Number(form.numberOfPeople),
      purpose: form.purpose
    });

    form.purpose = '';
    form.numberOfPeople = 1;

    emit('created', booking);
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Quick booking failed');
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel accent-panel">
    <div>
      <p class="eyebrow">Quick Booking</p>
      <h2>Let us choose the best room</h2>
      <p class="muted">
        Enter a time and number of people. The system picks the closest available room.
      </p>
    </div>

    <div class="grid two">
      <div class="form-row">
        <label>Start</label>
        <input v-model="form.startTime" type="datetime-local" />
      </div>

      <div class="form-row">
        <label>End</label>
        <input v-model="form.endTime" type="datetime-local" />
      </div>
    </div>

    <div class="grid two">
      <div class="form-row">
        <label>People</label>
        <input v-model="form.numberOfPeople" type="number" min="1" />
      </div>

      <div class="form-row">
        <label>Purpose</label>
        <input v-model="form.purpose" type="text" placeholder="Team catch-up" />
      </div>
    </div>

    <button class="primary-button" :disabled="isLoading" @click="submit">
      {{ isLoading ? 'Booking...' : 'Quick book room' }}
    </button>
  </section>
</template>
