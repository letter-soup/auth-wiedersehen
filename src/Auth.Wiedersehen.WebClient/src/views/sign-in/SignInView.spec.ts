import { describe, it, expect, vi } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import SignInView from './SignInView.vue'
import { ButtonStub, InputStub, LabelStub, SeparatorStub } from '@/lib/__mock__/stubs'

const mockToast = vi.fn()
vi.mock('vue-sonner', () => ({
  toast: (...args: unknown[]) => mockToast(...args),
}))

const stubs = {
  Button: ButtonStub,
  Input: InputStub,
  Label: LabelStub,
  Separator: SeparatorStub,
}

describe('SignInView (snapshot)', () => {
  it('renders correctly (snapshot)', () => {
    const wrapper = mount(SignInView, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

// describe('SignInView', () => {
//
// })
