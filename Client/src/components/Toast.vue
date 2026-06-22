<script setup lang="ts">
defineProps<{
  message: string;
  type: 'success' | 'error';
}>();
</script>

<template>
  <div class="toast" :class="type">
    {{ message }}
  </div>
</template>
