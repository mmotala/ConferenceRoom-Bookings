import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/shared/api/apiClient', () => ({
  apiRequest: vi.fn()
}));

import { apiRequest } from '@/shared/api/apiClient';
import { createRoom, deleteRoom, getRooms } from './roomsApi';

const mockedApiRequest = vi.mocked(apiRequest);

describe('rooms api', () => {
  beforeEach(() => {
    mockedApiRequest.mockResolvedValue(undefined);
  });

  it('calls room endpoints with the expected options', async () => {
    await getRooms();
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/rooms');

    await createRoom({ name: 'Boardroom', capacity: 12, location: 'First Floor' });
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/rooms', {
      method: 'POST',
      body: { name: 'Boardroom', capacity: 12, location: 'First Floor' }
    });

    await deleteRoom('r1');
    expect(mockedApiRequest).toHaveBeenLastCalledWith('/api/rooms/r1', {
      method: 'DELETE'
    });
  });
});
