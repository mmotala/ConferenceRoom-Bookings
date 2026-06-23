import type { CurrentUser } from '@/features/users/types/auth';

const storageKey = 'conference-room-current-user';

export function getCurrentUser(): CurrentUser | null {
  const raw = localStorage.getItem(storageKey);

  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as CurrentUser;
  } catch {
    localStorage.removeItem(storageKey);
    return null;
  }
}

export function setCurrentUser(user: CurrentUser): void {
  localStorage.setItem(storageKey, JSON.stringify(user));
}

export function clearCurrentUser(): void {
  localStorage.removeItem(storageKey);
}
