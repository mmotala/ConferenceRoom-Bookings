import {
  loginAs,
  setupConferenceRoomApi
} from '../../support/conferenceRoomApi';

describe('admin workflows', () => {
  beforeEach(() => {
    setupConferenceRoomApi();
    cy.clearLocalStorage();
    loginAs();
  });

  it('creates and deletes rooms', () => {
    cy.contains('section', 'Manage rooms').within(() => {
      cy.get('input[placeholder="Focus Room"]').type('Workshop Room');
      cy.get('input[type="number"]').clear().type('8');
      cy.get('input[placeholder="First Floor"]').type('Third Floor');
      cy.contains('button', 'Create room').click();
    });
    cy.wait('@createRoom');
    cy.contains('Room created').should('be.visible');

    cy.window().then(win => {
      cy.stub(win, 'confirm').returns(true);
    });
    cy.contains('.admin-room-row', 'Focus Room').contains('button', 'Delete').click();
    cy.wait('@deleteRoom');
    cy.contains('Room deleted').should('be.visible');
  });

  it('creates users', () => {
    cy.contains('section', 'Add user').within(() => {
      cy.get('input[placeholder="Jane Doe"]').type('New User');
      cy.get('input[placeholder="jane@demo.com"]').type('new.user@example.com');
      cy.get('select').select('User');
      cy.contains('button', 'Create user').click();
    });
    cy.wait('@createUser');
    cy.contains('User created successfully').should('be.visible');
  });
});
