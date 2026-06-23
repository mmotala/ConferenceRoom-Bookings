import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/shared/api/apiClient', () => ({
  apiRequest: vi.fn()
}));

import { apiRequest } from '@/shared/api/apiClient';
import { createUser, dummyLogin, getDummyUsers } from './authApi';

const mockedApiRequest = vi.mocked(apiRequest);

describe('auth api', () => {
  beforeEach(() => {
    mockedApiRequest.mockResolvedValue(undefined);
  });

  it('calls auth endpoints with the expected options', async () => {
    await getDummyUsers();
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/auth/dummy-users', {
      includeAuth: false
    });

    await dummyLogin({ email: 'ada@example.com' });
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/auth/dummy-login', {
      method: 'POST',
      body: { email: 'ada@example.com' },
      includeAuth: false
    });

    await createUser({ name: 'New User', email: 'new@example.com', role: 'User' });
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/admin/users', {
      method: 'POST',
      body: { name: 'New User', email: 'new@example.com', role: 'User' }
    });
  });
});
