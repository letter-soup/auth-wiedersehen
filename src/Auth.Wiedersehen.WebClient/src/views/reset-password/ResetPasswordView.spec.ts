import { describe, it, expect } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import ResetPasswordView from './ResetPasswordView.vue'

describe('ResetPasswordView (snapshot)', () => {
  it('renders correctly (snapshot)', () => {
    const wrapper = mount(ResetPasswordView)
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('ResetPasswordView', () => {
  it('displays Reset password text', () => {
    const wrapper = mount(ResetPasswordView)
    expect(wrapper.text()).toContain('Reset password')
  })
})
