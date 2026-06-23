export const adminUser = {
  userId: 'u-admin',
  name: 'Ada Admin',
  email: 'ada@example.com',
  role: 'Admin'
};

export const regularUser = {
  userId: 'u-user',
  name: 'Uma User',
  email: 'uma@example.com',
  role: 'User'
};

export const rooms = [
  {
    id: 'r1',
    name: 'Boardroom',
    capacity: 12,
    location: 'First Floor'
  },
  {
    id: 'r2',
    name: 'Focus Room',
    capacity: 4,
    location: 'Second Floor'
  }
];

export const activeBookings = [
  {
    id: 'b1',
    roomId: 'r1',
    roomName: 'Boardroom',
    userId: 'u-admin',
    userName: 'Ada Admin',
    startTimeUtc: '2030-01-10T09:00:00.000Z',
    endTimeUtc: '2030-01-10T10:00:00.000Z',
    purpose: 'Roadmap planning',
    status: 'Active',
    recurringBookingSeriesId: 'series-1'
  }
];

export const cancelledBookings = [
  {
    id: 'b2',
    roomId: 'r2',
    roomName: 'Focus Room',
    userId: 'u-user',
    userName: 'Uma User',
    startTimeUtc: '2030-01-11T11:00:00.000Z',
    endTimeUtc: '2030-01-11T12:00:00.000Z',
    purpose: 'Cancelled interview',
    status: 'Cancelled',
    recurringBookingSeriesId: null
  }
];

export function setupConferenceRoomApi() {
  cy.intercept(
    { method: 'GET', pathname: '/api/auth/dummy-users' },
    [adminUser, regularUser]
  ).as('getUsers');

  cy.intercept({ method: 'POST', pathname: '/api/auth/dummy-login' }, req => {
    const email = req.body?.email;
    req.reply(email === regularUser.email ? regularUser : adminUser);
  }).as('login');

  cy.intercept({ method: 'GET', pathname: '/api/rooms' }, rooms).as('getRooms');

  cy.intercept({ method: 'GET', pathname: '/api/bookings/calendar' }, [
    {
      bookingId: 'calendar-1',
      roomId: 'r1',
      roomName: 'Boardroom',
      userId: 'u-admin',
      userName: 'Ada Admin',
      startTimeUtc: '2030-01-10T09:00:00.000Z',
      endTimeUtc: '2030-01-10T10:00:00.000Z',
      purpose: 'Roadmap planning',
      status: 'Active',
      recurringBookingSeriesId: 'series-1'
    }
  ]).as('getCalendar');

  cy.intercept({ method: 'GET', pathname: '/api/bookings' }, req => {
    const url = new URL(req.url);
    const status = url.searchParams.get('status');

    if (status === 'Cancelled') {
      req.reply(cancelledBookings);
      return;
    }

    if (status === 'Active') {
      req.reply(activeBookings);
      return;
    }

    req.reply([...activeBookings, ...cancelledBookings]);
  }).as('getBookings');

  cy.intercept({ method: 'POST', pathname: '/api/bookings/quick' }, {
    ...activeBookings[0],
    id: 'b-quick',
    purpose: 'Quick team sync'
  }).as('quickBook');

  cy.intercept(
    { method: 'POST', pathname: '/api/bookings/recurring' },
    {}
  ).as('createRecurringBooking');

  cy.intercept({ method: 'POST', pathname: '/api/bookings' }, {
    ...activeBookings[0],
    id: 'b-manual',
    purpose: 'Project planning'
  }).as('createBooking');

  cy.intercept(
    { method: 'PUT', pathname: '/api/bookings/b1' },
    { ...activeBookings[0], purpose: 'Updated planning' }
  ).as('updateBooking');

  cy.intercept(
    { method: 'POST', pathname: '/api/bookings/b1/cancel' },
    { statusCode: 204 }
  ).as('cancelBooking');

  cy.intercept(
    { method: 'POST', pathname: '/api/bookings/series/series-1/cancel' },
    { statusCode: 204 }
  ).as('cancelSeries');

  cy.intercept({ method: 'POST', pathname: '/api/rooms' }, {
    id: 'r3',
    name: 'Workshop Room',
    capacity: 8,
    location: 'Third Floor'
  }).as('createRoom');

  cy.intercept(
    { method: 'DELETE', pathname: '/api/rooms/r2' },
    { statusCode: 204 }
  ).as('deleteRoom');

  cy.intercept({ method: 'POST', pathname: '/api/admin/users' }, {
    userId: 'u-new',
    name: 'New User',
    email: 'new.user@example.com',
    role: 'User'
  }).as('createUser');
}

export function loginAs(email = adminUser.email) {
  cy.visit('/', {
    onBeforeLoad(window) {
      window.localStorage.clear();
    }
  });

  cy.contains('h2', 'Choose a user').should('be.visible');
  cy.get('select#user option').should('have.length.greaterThan', 0);
  cy.get('select#user').select(email);
  cy.contains('button', 'Login').click();
  cy.wait('@login');
  cy.wait('@getRooms');
  cy.wait('@getBookings');
  cy.wait('@getCalendar');
}
