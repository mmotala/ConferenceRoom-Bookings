<script setup lang="ts">
import { ref } from 'vue';

import BookingCalendar from '@/features/bookings/components/BookingCalendar.vue';
import BookingCard from '@/features/bookings/components/BookingCard.vue';
import BookingForm from '@/features/bookings/components/BookingForm.vue';
import QuickBookingForm from '@/features/bookings/components/QuickBookingForm.vue';
import RecurringBookingForm from '@/features/bookings/components/RecurringBookingForm.vue';
import { cancelBooking, cancelRecurringSeries } from '@/features/bookings/api/bookingsApi';
import { useDashboard } from '@/features/bookings/composables/useDashboard';
import type { Booking } from '@/features/bookings/types/booking';
import AdminRoomsPanel from '@/features/rooms/components/AdminRoomsPanel.vue';
import RoomCard from '@/features/rooms/components/RoomCard.vue';
import AdminUsersPanel from '@/features/users/components/AdminUsersPanel.vue';
import LoginPanel from '@/features/users/components/LoginPanel.vue';
import {
  clearCurrentUser,
  getCurrentUser
} from '@/features/users/stores/currentUserStore';
import type { CurrentUser } from '@/features/users/types/auth';
import AppHeader from '@/shared/components/AppHeader.vue';
import Toast from '@/shared/components/Toast.vue';
import { useToast } from '@/shared/composables/useToast';

const currentUser = ref<CurrentUser | null>(getCurrentUser());
const editingBooking = ref<Booking | null>(null);

const { toast, showSuccess, showError } = useToast();
const {
  rooms,
  bookings,
  isLoading,
  bookingStatusFilter,
  loadDashboard,
  clearDashboard
} = useDashboard(currentUser, showError);

async function onLoggedIn(user: CurrentUser) {
  currentUser.value = user;
  showSuccess(`Logged in as ${user.name}`);
  await loadDashboard();
}

function logout() {
  clearCurrentUser();
  currentUser.value = null;
  clearDashboard();
  showSuccess('Logged out');
}

async function onBookingCreated() {
  showSuccess('Booking created successfully');
  await loadDashboard();
}

async function onBookingUpdated() {
  showSuccess('Booking updated successfully');
  editingBooking.value = null;
  await loadDashboard();
}

function onEditBooking(booking: Booking) {
  editingBooking.value = booking;
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

async function onCancelSeries(seriesId: string) {
  try {
    await cancelRecurringSeries(seriesId);
    showSuccess('Recurring event cancelled');
    await loadDashboard();
  } catch (error) {
    showError(error instanceof Error ? error.message : 'Failed to cancel recurring event');
  }
}
</script>

<template>
  <main class="app-shell">
    <AppHeader :current-user="currentUser" @logout="logout" />

    <Toast v-if="toast" :message="toast.message" :type="toast.type" />

    <LoginPanel v-if="!currentUser" @logged-in="onLoggedIn" @error="showError" />

    <template v-else>
      <section class="dashboard-grid">
        <BookingCalendar :rooms="rooms" @error="showError" />
        <QuickBookingForm @created="onBookingCreated" @error="showError" />

        <BookingForm :rooms="rooms" @created="onBookingCreated" @error="showError" />
        <RecurringBookingForm :rooms="rooms" @created="onBookingCreated" @error="showError" />
      </section>

      <BookingForm
        v-if="editingBooking"
        :rooms="rooms"
        :booking="editingBooking"
        @updated="onBookingUpdated"
        @cancel-edit="editingBooking = null"
        @error="showError"
      />

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
              <p class="eyebrow">
                {{ currentUser?.role === 'Admin' ? 'Admin View' : 'Your Schedule' }}
              </p>

              <h2>{{ currentUser?.role === 'Admin' ? 'All bookings' : 'My bookings' }}</h2>

              <p class="muted">
                {{
                  currentUser?.role === 'Admin'
                    ? 'View and manage bookings for all users.'
                    : 'View and manage your upcoming bookings.'
                }}
              </p>
            </div>
          </div>
          <div class="booking-filter">
            <button class="filter-button" :class="{ active: bookingStatusFilter === 'Active' }"
              @click="bookingStatusFilter = 'Active'">
              Active
            </button>

            <button class="filter-button" :class="{ active: bookingStatusFilter === 'Cancelled' }"
              @click="bookingStatusFilter = 'Cancelled'">
              Cancelled
            </button>

            <button class="filter-button" :class="{ active: bookingStatusFilter === undefined }"
              @click="bookingStatusFilter = undefined">
              All
            </button>
          </div>

          <div v-if="isLoading" class="empty-state">
            Loading bookings...
          </div>

          <div v-else-if="bookings.length === 0" class="empty-state">
            {{ currentUser?.role === 'Admin' ? 'No bookings found.' : 'You have no bookings yet.' }}
          </div>

          <div v-else class="cards-list">
            <BookingCard v-for="booking in bookings" :key="booking.id ?? booking.bookingId" :booking="booking"
              @edit="onEditBooking" @cancel="onCancelBooking" @cancel-series="onCancelSeries" />
          </div>
        </section>
      </section>
    </template>
  </main>
</template>
