import { onMounted, ref, watch, type Ref } from 'vue';

import { getBookings } from '@/features/bookings/api/bookingsApi';
import type { Booking, BookingStatus } from '@/features/bookings/types/booking';
import { getRooms } from '@/features/rooms/api/roomsApi';
import type { Room } from '@/features/rooms/types/room';
import type { CurrentUser } from '@/features/users/types/auth';

export function useDashboard(
  currentUser: Ref<CurrentUser | null>,
  onError: (message: string) => void
) {
  const rooms = ref<Room[]>([]);
  const bookings = ref<Booking[]>([]);
  const isLoading = ref(false);
  const bookingStatusFilter = ref<BookingStatus | undefined>('Active');

  async function loadDashboard() {
    if (!currentUser.value) {
      return;
    }

    isLoading.value = true;

    try {
      const [roomsResponse, bookingsResponse] = await Promise.all([
        getRooms(),
        getBookings(bookingStatusFilter.value)
      ]);

      rooms.value = roomsResponse;
      bookings.value = bookingsResponse;
    } catch (error) {
      onError(error instanceof Error ? error.message : 'Failed to load dashboard');
    } finally {
      isLoading.value = false;
    }
  }

  function clearDashboard() {
    rooms.value = [];
    bookings.value = [];
  }

  onMounted(async () => {
    if (currentUser.value) {
      await loadDashboard();
    }
  });

  watch(bookingStatusFilter, loadDashboard);

  return {
    rooms,
    bookings,
    isLoading,
    bookingStatusFilter,
    loadDashboard,
    clearDashboard
  };
}
