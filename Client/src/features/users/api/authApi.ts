import { apiRequest } from '@/shared/api/apiClient';
import type {
  CreateUserRequest,
  DummyLoginRequest,
  DummyUser
} from '@/features/users/types/auth';

export function getDummyUsers(): Promise<DummyUser[]> {
  return apiRequest<DummyUser[]>('/api/auth/dummy-users', {
    includeAuth: false
  });
}

export function dummyLogin(request: DummyLoginRequest): Promise<DummyUser> {
  return apiRequest<DummyUser>('/api/auth/dummy-login', {
    method: 'POST',
    body: request,
    includeAuth: false
  });
}

export function createUser(request: CreateUserRequest): Promise<DummyUser> {
  return apiRequest<DummyUser>('/api/admin/users', {
    method: 'POST',
    body: request
  });
}
