import { getCurrentUser } from '@/stores/currentUserStore';

const baseUrl = import.meta.env.VITE_API_BASE_URL;

type ApiOptions = {
  method?: string;
  body?: unknown;
  includeAuth?: boolean;
};

export async function apiRequest<T>(
  path: string,
  options: ApiOptions = {}
): Promise<T> {
  const user = getCurrentUser();

  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };

  if (options.includeAuth !== false && user) {
    headers['X-User-Id'] = user.userId;
    headers['X-User-Role'] = user.role;
  }

  const response = await fetch(`${baseUrl}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined
  });

  if (!response.ok) {
    const errorBody = await safeReadJson(response);

    const message =
      errorBody?.detail ||
      errorBody?.title ||
      `Request failed with status ${response.status}`;

    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function safeReadJson(response: Response): Promise<any | null> {
  try {
    return await response.json();
  } catch {
    return null;
  }
}
