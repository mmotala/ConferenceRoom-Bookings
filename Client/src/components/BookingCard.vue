<script setup lang="ts">
import type { Booking } from '@/types/booking';
import { formatBookingDate } from '@/utils/dateUtils';

const props = defineProps<{
  booking: Booking;
}>();

const emit = defineEmits<{
  cancel: [bookingId: string];
}>();

function formatDate(value: string) {
  return formatBookingDate(value);
}

function getBookingId() {
  return props.booking.id ?? props.booking.bookingId;
}
</script>

<template>
  <article class="booking-card">
    <div class="booking-main">
      <div>
        <h3>{{ booking.purpose }}</h3>
        <p class="muted">
          {{ booking.roomName ?? 'Room' }}
        </p>
      </div>

      <span class="status-badge" :class="booking.status.toLowerCase()">
        {{ booking.status }}
      </span>
    </div>

    <div class="booking-time">
      <span>{{ formatDate(booking.startTimeUtc) }}</span>
      <span>→</span>
      <span>{{ formatDate(booking.endTimeUtc) }}</span>
    </div>

    <button
      v-if="booking.status === 'Active'"
      class="danger-button"
      @click="emit('cancel', getBookingId())"
    >
      Cancel booking
    </button>
  </article>
</template>
