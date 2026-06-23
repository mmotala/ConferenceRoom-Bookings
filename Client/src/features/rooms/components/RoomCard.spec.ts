import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import RoomCard from './RoomCard.vue';

describe('RoomCard', () => {
  it('renders room details', () => {
    const wrapper = mount(RoomCard, {
      props: {
        room: {
          id: 'r1',
          name: 'Boardroom',
          capacity: 12,
          location: 'First Floor'
        }
      }
    });

    expect(wrapper.text()).toContain('Boardroom');
    expect(wrapper.text()).toContain('First Floor');
    expect(wrapper.text()).toContain('12 people');
  });
});
