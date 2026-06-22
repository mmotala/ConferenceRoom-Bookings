<script setup lang="ts">
import { onMounted, ref } from 'vue';

import AppHeader from '@/components/AppHeader.vue';
import BookingCard from '@/components/BookingCard.vue';
import BookingForm from '@/components/BookingForm.vue';
import LoginPanel from '@/components/LoginPanel.vue';
import QuickBookingForm from '@/components/QuickBookingForm.vue';
import RoomCard from '@/components/RoomCard.vue';
import Toast from '@/components/Toast.vue';

import { cancelBooking, getBookings } from '@/api/bookingsApi';
import { getRooms } from '@/api/roomsApi';
import {
  clearCurrentUser,
  getCurrentUser
} from '@/stores/currentUserStore';

import type { Booking } from '@/types/booking';
import type { CurrentUser } from '@/types/auth';
import type { Room } from '@/types/room';
import AdminRoomsPanel from '@/components/AdminRoomsPanel.vue';
import AdminUsersPanel from '@/components/AdminUsersPanel.vue';

const currentUser = ref<CurrentUser | null>(getCurrentUser());
const rooms = ref<Room[]>([]);
const bookings = ref<Booking[]>([]);
const isLoading = ref(false);

const toast = ref<{
  message: string;
  type: 'success' | 'error';
} | null>(null);

onMounted(async () => {
  if (currentUser.value) {
    await loadDashboard();
  }
});

async function loadDashboard() {
  isLoading.value = true;

  try {
    const [roomsResponse, bookingsResponse] = await Promise.all([
      getRooms(),
      getBookings()
    ]);

    rooms.value = roomsResponse;
    bookings.value = bookingsResponse;
  } catch (error) {
    showError(error instanceof Error ? error.message : 'Failed to load dashboard');
  } finally {
    isLoading.value = false;
  }
}

async function onLoggedIn(user: CurrentUser) {
  currentUser.value = user;
  showSuccess(`Logged in as ${user.name}`);
  await loadDashboard();
}

function logout() {
  clearCurrentUser();
  currentUser.value = null;
  rooms.value = [];
  bookings.value = [];
  showSuccess('Logged out');
}

async function onBookingCreated() {
  showSuccess('Booking created successfully');
  await loadDashboard();
}

async function onCancelBooking(bookingId: string) {
  try {
    await cancelBooking(bookingId);
    showSuccess('Booking cancelled');
    await loadDashboard();
  } catch (error) {
    showError(error instanceof Error ? error.message : 'Failed to cancel booking');
  }
}

function showSuccess(message: string) {
  showToast(message, 'success');
}

function showError(message: string) {
  showToast(message, 'error');
}

function showToast(message: string, type: 'success' | 'error') {
  toast.value = { message, type };

  window.setTimeout(() => {
    toast.value = null;
  }, 3500);
}
</script>

<template>
  <main class="app-shell">
    <AppHeader :current-user="currentUser" @logout="logout" />

    <Toast v-if="toast" :message="toast.message" :type="toast.type" />

    <LoginPanel v-if="!currentUser" @logged-in="onLoggedIn" @error="showError" />

    <template v-else>
      <section class="dashboard-grid">
        <QuickBookingForm @created="onBookingCreated" @error="showError" />

        <BookingForm :rooms="rooms" @created="onBookingCreated" @error="showError" />
      </section>

      <AdminRoomsPanel v-if="currentUser?.role === 'Admin'" :rooms="rooms" @changed="loadDashboard"
        @success="showSuccess" @error="showError" />

      <AdminUsersPanel v-if="currentUser?.role === 'Admin'" @success="showSuccess" @error="showError" />

      <section class="content-grid">
        <section class="panel">
          <div class="section-header">
            <div>
              <p class="eyebrow">Rooms</p>
              <h2>Available rooms</h2>
            </div>

            <span class="count-pill">{{ rooms.length }}</span>
          </div>

          <div v-if="isLoading" class="empty-state">
            Loading rooms...
          </div>

          <div v-else class="cards-list">
            <RoomCard v-for="room in rooms" :key="room.id" :room="room" />
          </div>
        </section>

        <section class="panel">
          <div class="section-header">
            <div>
              <p class="eyebrow">Bookings</p>
              <h2>{{ currentUser?.role === 'Admin' ? 'All bookings' : 'My bookings' }}</h2>
            </div>

            <span class="count-pill">{{ bookings.length }}</span>
          </div>

          <div v-if="isLoading" class="empty-state">
            Loading bookings...
          </div>

          <div v-else-if="bookings.length === 0" class="empty-state">
            {{ currentUser?.role === 'Admin' ? 'No bookings found.' : 'You have no bookings yet.' }}
          </div>

          <div v-else class="cards-list">
            <BookingCard v-for="booking in bookings" :key="booking.id ?? booking.bookingId" :booking="booking"
              @cancel="onCancelBooking" />
          </div>
        </section>
      </section>
    </template>
  </main>
</template>
