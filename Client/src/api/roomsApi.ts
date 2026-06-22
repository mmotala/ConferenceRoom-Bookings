import { apiRequest } from '@/api/apiClient';
import type { CreateRoomRequest, Room } from '@/types/room';

export function getRooms(): Promise<Room[]> {
  return apiRequest<Room[]>('/api/rooms');
}

export function createRoom(request: CreateRoomRequest): Promise<Room> {
  return apiRequest<Room>('/api/rooms', {
    method: 'POST',
    body: request
  });
}

export function deleteRoom(roomId: string): Promise<void> {
  return apiRequest<void>(`/api/rooms/${roomId}`, {
    method: 'DELETE'
  });
}
