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

  it('validates and creates quick, manual and recurring bookings', () => {
    cy.contains('button', 'Quick book').click();
    cy.contains('Purpose is required').should('be.visible');

    cy.contains('section', 'Find me a room').within(() => {
      cy.get('input[type="number"]').clear().type('5');
      cy.get('input[placeholder="Team sync"]').type('Quick team sync');
      cy.contains('button', 'Quick book').click();
    });
    cy.wait('@quickBook');
    cy.contains('Booking created successfully').should('be.visible');

    cy.contains('section', 'Create booking').within(() => {
      cy.get('select').select('r1');
      cy.get('input[placeholder="Project planning"]').type('Project planning');
      cy.contains('button', 'Create booking').click();
    });
    cy.wait('@createBooking');
    cy.contains('Booking created successfully').should('be.visible');

    cy.contains('section', 'Create recurring event').within(() => {
      cy.get('select').first().select('r1');
      cy.get('input[placeholder="Weekly team sync"]').type('Leadership sync');
      cy.contains('button', 'Create recurring booking').click();
    });
    cy.wait('@createRecurringBooking');
    cy.contains('Booking created successfully').should('be.visible');
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
