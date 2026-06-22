<script setup lang="ts">
import { reactive, ref } from 'vue';
import { createUser } from '@/api/authApi';
import type { UserRole } from '@/types/auth';
import { validateEmail, validateRequired } from '@/utils/validation';

const emit = defineEmits<{
  success: [message: string];
  error: [message: string];
}>();

const isLoading = ref(false);

const form = reactive({
  name: '',
  email: '',
  role: 'User' as UserRole
});

async function submit() {
  const errors = [
    validateRequired(form.name, 'Name'),
    validateEmail(form.email)
  ].filter(Boolean);

  if (errors.length > 0) {
    emit('error', errors[0]!);
    return;
  }

  isLoading.value = true;

  try {
    await createUser({
      name: form.name,
      email: form.email,
      role: form.role
    });

    form.name = '';
    form.email = '';
    form.role = 'User';

    emit('success', 'User created successfully');
  } catch (error) {
    emit('error', error instanceof Error ? error.message : 'Failed to create user');
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <section class="panel admin-panel">
    <div>
      <p class="eyebrow">Admin</p>
      <h2>Add user</h2>
      <p class="muted">
        Create demo users who can log in and create bookings.
      </p>
    </div>

    <div class="grid three">
      <div class="form-row">
        <label>Name</label>
        <input v-model="form.name" type="text" placeholder="Jane Doe" />
      </div>

      <div class="form-row">
        <label>Email</label>
        <input v-model="form.email" type="email" placeholder="jane@demo.com" />
      </div>

      <div class="form-row">
        <label>Role</label>
        <select v-model="form.role">
          <option value="User">User</option>
          <option value="Admin">Admin</option>
        </select>
      </div>
    </div>

    <button class="secondary-button" :disabled="isLoading" @click="submit">
      {{ isLoading ? 'Creating...' : 'Create user' }}
    </button>
  </section>
</template>
