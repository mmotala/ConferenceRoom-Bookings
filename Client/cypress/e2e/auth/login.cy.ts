import {
  adminUser,
  loginAs,
  regularUser,
  setupConferenceRoomApi
} from '../../support/conferenceRoomApi';

describe('login', () => {
  beforeEach(() => {
    setupConferenceRoomApi();
    cy.clearLocalStorage();
  });

  it('logs in as an admin and supports logout', () => {
    loginAs(adminUser.email);

    cy.contains('Logged in as Ada Admin').should('be.visible');
    cy.contains('h2', 'All bookings').should('be.visible');
    cy.contains('h2', 'Manage rooms').should('be.visible');

    cy.contains('button', 'Logout').click();

    cy.contains('Choose a user').should('be.visible');
    cy.contains('Logged out').should('be.visible');
  });

  it('hides admin-only areas for a regular user', () => {
    loginAs(regularUser.email);

    cy.contains('Logged in as Uma User').should('be.visible');
    cy.contains('h2', 'My bookings').should('be.visible');
    cy.contains('h2', 'Manage rooms').should('not.exist');
    cy.contains('h2', 'Add user').should('not.exist');
    cy.contains('View and manage your upcoming bookings.').should('be.visible');
  });
});
