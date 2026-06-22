<script setup lang="ts">
import { reactive, ref } from 'vue';
import { createBooking } from '@/api/bookingsApi';
import type { Booking } from '@/types/booking';
import type { Room } from '@/types/room';

defineProps<{
  rooms: Room[];
}>();

const emit = defineEmits<{
  created: [booking: Booking];
  error: [message: string];
}>();

const isLoading = ref(false);

const form = reactive({
  roomId: '',
  startTime: '',
  endTime: '',
  purpose: ''
});

async function submit() {
  if (!form.roomId || !form.startTime || !form.endTime || !form.purpose) {
    emit('error', 'Please complete all booking fields');
    return;
  }

  isLoading.value = true;

  try {
    const booking = await createBooking({
      roomId: form.roomId,
      startTimeUtc: new Date(form.startTime).toISOString(),
      endTimeUtc: new Date(form.endTime).toISOString(),
      purpose: form.purpose
    });

    form.purpose = '';

    emit('created', booking);
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Booking failed');
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel">
    <div>
      <p class="eyebrow">Manual Booking</p>
      <h2>Choose a specific room</h2>
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
        <input v-model="form.startTime" type="datetime-local" />
      </div>

      <div class="form-row">
        <label>End</label>
        <input v-model="form.endTime" type="datetime-local" />
      </div>
    </div>

    <div class="form-row">
      <label>Purpose</label>
      <input v-model="form.purpose" type="text" placeholder="Planning session" />
    </div>

    <button class="secondary-button" :disabled="isLoading" @click="submit">
      {{ isLoading ? 'Creating...' : 'Create booking' }}
    </button>
  </section>
</template>
