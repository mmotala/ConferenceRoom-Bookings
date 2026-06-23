import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import Toast from './Toast.vue';

describe('Toast', () => {
  it('renders the message and type class', () => {
    const wrapper = mount(Toast, {
      props: {
        message: 'Saved',
        type: 'success'
      }
    });

    expect(wrapper.text()).toContain('Saved');
    expect(wrapper.classes()).toContain('success');
  });
});
