import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const authApi = vi.hoisted(() => ({
  createUser: vi.fn()
}));

vi.mock('@/features/users/api/authApi', () => authApi);

import AdminUsersPanel from './AdminUsersPanel.vue';

describe('AdminUsersPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    authApi.createUser.mockResolvedValue({
      userId: 'u2',
      name: 'New User',
      email: 'new@example.com',
      role: 'User'
    });
  });

  it('validates and creates users', async () => {
    const wrapper = mount(AdminUsersPanel);

    await wrapper.get('button').trigger('click');

    expect(wrapper.emitted('error')).toEqual([['Name is required.']]);

    await wrapper.get('input[placeholder="Jane Doe"]').setValue('New User');
    await wrapper.get('input[placeholder="jane@demo.com"]').setValue('new@example.com');
    await wrapper.get('select').setValue('Admin');
    await wrapper.get('button').trigger('click');
    await vi.dynamicImportSettled();

    expect(authApi.createUser).toHaveBeenCalledWith({
      name: 'New User',
      email: 'new@example.com',
      role: 'Admin'
    });
    expect(wrapper.emitted('success')).toEqual([['User created successfully']]);
  });
});
