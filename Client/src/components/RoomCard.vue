<script setup lang="ts">
import type { Room } from '@/types/room';

defineProps<{
  room: Room;
}>();
</script>

<template>
  <article class="room-card">
    <div>
      <h3>{{ room.name }}</h3>
      <p class="muted">{{ room.location }}</p>
    </div>

    <div class="capacity-badge">
      {{ room.capacity }} people
    </div>
  </article>
</template>
