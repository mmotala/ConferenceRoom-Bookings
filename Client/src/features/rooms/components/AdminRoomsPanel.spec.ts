import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const roomsApi = vi.hoisted(() => ({
  createRoom: vi.fn(),
  deleteRoom: vi.fn()
}));

vi.mock('@/features/rooms/api/roomsApi', () => roomsApi);

import AdminRoomsPanel from './AdminRoomsPanel.vue';

const room = {
  id: 'r1',
  name: 'Boardroom',
  capacity: 12,
  location: 'First Floor'
};

describe('AdminRoomsPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    roomsApi.createRoom.mockResolvedValue(room);
    roomsApi.deleteRoom.mockResolvedValue(undefined);
  });

  it('validates and creates rooms', async () => {
    const wrapper = mount(AdminRoomsPanel, {
      props: {
        rooms: [room]
      }
    });

    await wrapper.get('button.secondary-button').trigger('click');

    expect(wrapper.emitted('error')).toEqual([['Please complete all room fields']]);

    await wrapper.get('input[placeholder="Focus Room"]').setValue('Workshop Room');
    await wrapper.get('input[type="number"]').setValue(8);
    await wrapper.get('input[placeholder="First Floor"]').setValue('Third Floor');
    await wrapper.get('button.secondary-button').trigger('click');
    await vi.dynamicImportSettled();

    expect(roomsApi.createRoom).toHaveBeenCalledWith({
      name: 'Workshop Room',
      capacity: 8,
      location: 'Third Floor'
    });
    expect(wrapper.emitted('success')).toContainEqual(['Room created']);
    expect(wrapper.emitted('changed')).toHaveLength(1);
  });

  it('deletes rooms after confirmation', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    const wrapper = mount(AdminRoomsPanel, {
      props: {
        rooms: [room]
      }
    });

    await wrapper.get('button.danger-button').trigger('click');
    await vi.dynamicImportSettled();

    expect(roomsApi.deleteRoom).toHaveBeenCalledWith('r1');
    expect(wrapper.emitted('success')).toContainEqual(['Room deleted']);
  });
});
