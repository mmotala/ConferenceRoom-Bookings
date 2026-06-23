<script setup lang="ts">
import type { Booking } from '@/features/bookings/types/booking';
import { formatBookingDate } from '@/shared/utils/dateUtils';

const props = defineProps<{
  booking: Booking;
}>();

const emit = defineEmits<{
  edit: [booking: Booking];
  cancel: [bookingId: string];
  cancelSeries: [seriesId: string];
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

    <div class="booking-actions" v-if="booking.status === 'Active'">
      <button class="secondary-button compact" @click="emit('edit', booking)">
        Update booking
      </button>

      <button class="danger-button" @click="emit('cancel', getBookingId())">
        Cancel booking
      </button>

      <button v-if="booking.recurringBookingSeriesId" class="danger-button"
        @click="emit('cancelSeries', booking.recurringBookingSeriesId)">
        Cancel whole series
      </button>
    </div>
  </article>
</template>
