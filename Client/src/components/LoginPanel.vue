<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { dummyLogin, getDummyUsers } from '@/api/authApi';
import { setCurrentUser } from '@/stores/currentUserStore';
import type { DummyUser } from '@/types/auth';

const emit = defineEmits<{
  loggedIn: [user: DummyUser];
  error: [message: string];
}>();

const users = ref<DummyUser[]>([]);
const selectedEmail = ref('');
const isLoading = ref(false);

onMounted(async () => {
  try {
    users.value = await getDummyUsers();

    if (users.value.length > 0) {
      selectedEmail.value = users.value[0].email;
    }
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Failed to load users');
  }
});

async function login() {
  if (!selectedEmail.value) {
    emit('error', 'Please select a user');
    return;
  }

  isLoading.value = true;

  try {
    const user = await dummyLogin({ email: selectedEmail.value });

    setCurrentUser(user);

    emit('loggedIn', user);
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Login failed');
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel login-panel">
    <div>
      <p class="eyebrow">Demo Login</p>
      <h2>Choose a user</h2>
      <p class="muted">
        This uses dummy login and sends the selected user as request headers.
      </p>
    </div>

    <div class="form-row">
      <label for="user">User</label>
      <select id="user" v-model="selectedEmail">
        <option v-for="user in users" :key="user.userId" :value="user.email">
          {{ user.name }} — {{ user.role }}
        </option>
      </select>
    </div>

    <button class="primary-button" :disabled="isLoading" @click="login">
      {{ isLoading ? 'Logging in...' : 'Login' }}
    </button>
  </section>
</template>
