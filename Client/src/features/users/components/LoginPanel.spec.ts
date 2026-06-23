import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const authApi = vi.hoisted(() => ({
  dummyLogin: vi.fn(),
  getDummyUsers: vi.fn()
}));

const currentUserStore = vi.hoisted(() => ({
  setCurrentUser: vi.fn()
}));

vi.mock('@/features/users/api/authApi', () => authApi);
vi.mock('@/features/users/stores/currentUserStore', () => currentUserStore);

import LoginPanel from './LoginPanel.vue';

describe('LoginPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    authApi.getDummyUsers.mockResolvedValue([
      {
        userId: 'u1',
        name: 'Ada Admin',
        email: 'ada@example.com',
        role: 'Admin'
      }
    ]);
    authApi.dummyLogin.mockResolvedValue({
      userId: 'u1',
      name: 'Ada Admin',
      email: 'ada@example.com',
      role: 'Admin'
    });
  });

  it('loads users, logs in and stores the selected user', async () => {
    const wrapper = mount(LoginPanel);
    await vi.dynamicImportSettled();

    expect(wrapper.find('option').text()).toContain('Ada Admin');

    await wrapper.get('button').trigger('click');
    await vi.dynamicImportSettled();

    expect(authApi.dummyLogin).toHaveBeenCalledWith({ email: 'ada@example.com' });
    expect(currentUserStore.setCurrentUser).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Ada Admin' })
    );
    expect(wrapper.emitted('loggedIn')?.[0]?.[0]).toMatchObject({ name: 'Ada Admin' });
  });

  it('emits an error when user loading fails', async () => {
    authApi.getDummyUsers.mockRejectedValue(new Error('No users'));

    const wrapper = mount(LoginPanel);
    await vi.dynamicImportSettled();

    expect(wrapper.emitted('error')).toEqual([['No users']]);
  });
});
