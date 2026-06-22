export type Room = {
  id: string;
  name: string;
  capacity: number;
  location: string;
};

export type CreateRoomRequest = {
  name: string;
  capacity: number;
  location: string;
};
