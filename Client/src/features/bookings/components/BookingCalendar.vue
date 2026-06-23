<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import FullCalendar from '@fullcalendar/vue3';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';

import { getBookingCalendar } from '@/features/bookings/api/bookingsApi';
import type { CalendarBooking } from '@/features/bookings/types/booking';
import type { Room } from '@/features/rooms/types/room';

const props = defineProps<{
  rooms: Room[];
}>();

const emit = defineEmits<{
  error: [message: string];
}>();

const selectedRoomId = ref('');
const includeCancelled = ref(false);
const bookings = ref<CalendarBooking[]>([]);

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
  initialView: 'timeGridWeek',
  height: 'auto',
  nowIndicator: true,
  allDaySlot: false,
  events: bookings.value.map(booking => ({
    id: booking.bookingId,
    title: `${booking.roomName} - ${booking.purpose}`,
    start: booking.startTimeUtc,
    end: booking.endTimeUtc,
    extendedProps: {
      userName: booking.userName,
      status: booking.status,
      recurringBookingSeriesId: booking.recurringBookingSeriesId
    }
  }))
}));

onMounted(loadCalendar);

watch([selectedRoomId, includeCancelled], loadCalendar);

async function loadCalendar() {
  const now = new Date();
  const from = new Date(now);
  from.setDate(now.getDate() - 7);

  const to = new Date(now);
  to.setDate(now.getDate() + 30);

  try {
    bookings.value = await getBookingCalendar({
      fromUtc: from.toISOString(),
      toUtc: to.toISOString(),
      roomId: selectedRoomId.value || undefined,
      includeCancelled: includeCancelled.value
    });
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Failed to load calendar');
  }
}
</script>

<template>
  <section class="panel calendar-panel">
    <div class="section-header">
      <div>
        <p class="eyebrow">Calendar</p>
        <h2>Room schedule</h2>
        <p class="muted">
          See when rooms are already booked before creating a booking.
        </p>
      </div>
    </div>

    <div class="calendar-filters">
      <div class="form-row">
        <label>Room</label>
        <select v-model="selectedRoomId">
          <option value="">All rooms</option>
          <option v-for="room in props.rooms" :key="room.id" :value="room.id">
            {{ room.name }}
          </option>
        </select>
      </div>

      <label class="checkbox-row">
        <input v-model="includeCancelled" type="checkbox" />
        Show cancelled
      </label>
    </div>

    <FullCalendar :options="calendarOptions" />
  </section>
</template>
