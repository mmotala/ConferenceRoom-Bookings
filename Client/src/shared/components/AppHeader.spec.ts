import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import AppHeader from './AppHeader.vue';

describe('AppHeader', () => {
  it('renders the signed-in user and emits logout', async () => {
    const wrapper = mount(AppHeader, {
      props: {
        currentUser: {
          userId: 'u1',
          name: 'Ada Admin',
          email: 'ada@example.com',
          role: 'Admin'
        }
      }
    });

    expect(wrapper.text()).toContain('Meeting Room Manager');
    expect(wrapper.text()).toContain('Ada Admin');
    expect(wrapper.text()).toContain('Admin');

    await wrapper.get('button').trigger('click');

    expect(wrapper.emitted('logout')).toHaveLength(1);
  });
});
