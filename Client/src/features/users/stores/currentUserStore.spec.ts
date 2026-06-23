import { beforeEach, describe, expect, it } from 'vitest';

import {
  clearCurrentUser,
  getCurrentUser,
  setCurrentUser
} from './currentUserStore';

const user = {
  userId: 'u1',
  name: 'Ada Admin',
  email: 'ada@example.com',
  role: 'Admin' as const
};

describe('current user store', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('stores, reads and clears the current user', () => {
    expect(getCurrentUser()).toBeNull();

    setCurrentUser(user);

    expect(getCurrentUser()).toEqual(user);

    clearCurrentUser();

    expect(getCurrentUser()).toBeNull();
  });

  it('clears invalid JSON from storage', () => {
    localStorage.setItem('conference-room-current-user', '{bad json');

    expect(getCurrentUser()).toBeNull();
    expect(localStorage.getItem('conference-room-current-user')).toBeNull();
  });
});
