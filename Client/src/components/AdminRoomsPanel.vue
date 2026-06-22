<script setup lang="ts">
import { reactive, ref } from 'vue';
import { createRoom, deleteRoom } from '@/api/roomsApi';
import type { Room } from '@/types/room';

const props = defineProps<{
  rooms: Room[];
}>();

const emit = defineEmits<{
  changed: [];
  error: [message: string];
  success: [message: string];
}>();

const isLoading = ref(false);

const form = reactive({
  name: '',
  capacity: 4,
  location: ''
});

async function addRoom() {
  if (!form.name || !form.capacity || !form.location) {
    emit('error', 'Please complete all room fields');
    return;
  }

  isLoading.value = true;

  try {
    await createRoom({
      name: form.name,
      capacity: Number(form.capacity),
      location: form.location
    });

    form.name = '';
    form.capacity = 4;
    form.location = '';

    emit('success', 'Room created');
    emit('changed');
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Failed to create room');
  } finally {
    isLoading.value = false;
  }
}

async function removeRoom(roomId: string) {
  if (!confirm('Delete this room? Existing bookings will remain, but the room will no longer appear.')) {
    return;
  }

  try {
    await deleteRoom(roomId);
    emit('success', 'Room deleted');
    emit('changed');
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Failed to delete room');
  }
}
</script>

<template>
  <section class="panel admin-panel">
    <div>
      <p class="eyebrow">Admin</p>
      <h2>Manage rooms</h2>
      <p class="muted">Create or soft-delete rooms.</p>
    </div>

    <div class="grid three">
      <div class="form-row">
        <label>Name</label>
        <input v-model="form.name" type="text" placeholder="Focus Room" />
      </div>

      <div class="form-row">
        <label>Capacity</label>
        <input v-model="form.capacity" type="number" min="1" />
      </div>

      <div class="form-row">
        <label>Location</label>
        <input v-model="form.location" type="text" placeholder="First Floor" />
      </div>
    </div>

    <button class="secondary-button" :disabled="isLoading" @click="addRoom">
      {{ isLoading ? 'Creating...' : 'Create room' }}
    </button>

    <div class="admin-room-list">
      <div v-for="room in props.rooms" :key="room.id" class="admin-room-row">
        <div>
          <strong>{{ room.name }}</strong>
          <p class="muted">{{ room.location }} · {{ room.capacity }} people</p>
        </div>

        <button class="danger-button small" @click="removeRoom(room.id)">
          Delete
        </button>
      </div>
    </div>
  </section>
</template>
