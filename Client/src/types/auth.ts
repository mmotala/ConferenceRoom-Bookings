export type UserRole = 'Admin' | 'User';

export type DummyUser = {
  userId: string;
  name: string;
  email: string;
  role: UserRole;
};

export type DummyLoginRequest = {
  email: string;
};

export type CreateUserRequest = {
  name: string;
  email: string;
  role: UserRole;
};

export type CurrentUser = DummyUser;


