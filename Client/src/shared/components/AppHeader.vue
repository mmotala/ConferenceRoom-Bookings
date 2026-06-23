<script setup lang="ts">
import type { CurrentUser } from '@/features/users/types/auth';

defineProps<{
  currentUser: CurrentUser | null;
}>();

const emit = defineEmits<{
  logout: [];
}>();
</script>

<template>
  <header class="app-header">
    <div>
      <p class="eyebrow">Conference Room Booking</p>
      <h1>Meeting Room Manager</h1>
    </div>

    <div v-if="currentUser" class="user-pill">
      <div>
        <strong>{{ currentUser.name }}</strong>
        <span>{{ currentUser.role }}</span>
      </div>

      <button class="ghost-button" @click="emit('logout')">
        Logout
      </button>
    </div>
  </header>
</template>
