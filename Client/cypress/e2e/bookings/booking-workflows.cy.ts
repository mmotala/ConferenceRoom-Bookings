import {
  loginAs,
  setupConferenceRoomApi
} from '../../support/conferenceRoomApi';

describe('booking workflows', () => {
  beforeEach(() => {
    setupConferenceRoomApi();
    cy.clearLocalStorage();
    loginAs();
  });

  it('shows rooms, calendar and active bookings', () => {
    cy.contains('h2', 'Room schedule').should('be.visible');
    cy.contains('h2', 'Available rooms').should('be.visible');
    cy.contains('Boardroom').should('be.visible');
    cy.contains('Focus Room').should('be.visible');
    cy.contains('Roadmap planning').should('be.visible');
  });

  it('filters and cancels bookings', () => {
    cy.contains('button', 'Cancelled').click();
    cy.wait('@getBookings');
    cy.contains('Cancelled interview').should('be.visible');
    cy.contains('Roadmap planning').should('not.exist');

    cy.contains('button', 'All').click();
    cy.wait('@getBookings');
    cy.contains('Roadmap planning').should('be.visible');
    cy.contains('Cancelled interview').should('be.visible');

    cy.contains('button', 'Cancel booking').click();
    cy.wait('@cancelBooking');
    cy.contains('Booking cancelled').should('be.visible');

    cy.contains('button', 'Cancel whole series').click();
    cy.wait('@cancelSeries');
    cy.contains('Recurring event cancelled').should('be.visible');
  });
});
