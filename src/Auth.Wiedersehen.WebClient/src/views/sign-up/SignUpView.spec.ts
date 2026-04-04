import { describe, it, expect, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mount } from '@/lib/__tests__/mount'
import SignUpView from './SignUpView.vue'
import { InputStub, SeparatorStub, FormMessageStub } from '@/lib/__mock__/stubs'
import { createStub } from '@/lib/__mock__/component-stub'

vi.mock('vue-sonner', () => ({
  toast: vi.fn(),
}))

vi.mock('@/lib/api', () => ({
  checkEmailAvailability: vi.fn().mockResolvedValue(undefined),
  createUser: vi.fn().mockResolvedValue(undefined),
}))

vi.mock('@/views/sign-up/lib/validate-email', () => ({
  validateEmail: async (
    _values: Record<string, string>,
    _validate: unknown,
    nextStep: () => void,
  ) => {
    nextStep()
  },
}))

const stubs = {
  Button: createStub('Button', { template: '<button type="button"><slot /></button>' }),
  Input: InputStub,
  Separator: SeparatorStub,
  FormMessage: FormMessageStub,
}

describe('SignUpView (snapshot)', () => {
  it('renders step 1 correctly (snapshot)', () => {
    const wrapper = mount(SignUpView, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('SignUpView', () => {
  it('step 1 shows email input and CTA, step 2 hidden', () => {
    const wrapper = mount(SignUpView, { global: { stubs } })
    expect(wrapper.find('[data-testid="sign-up-step-email"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="sign-up-step-password"]').exists()).toBe(false)
    expect(wrapper.find('#email').exists()).toBe(true)
    expect(wrapper.find('#cta').exists()).toBe(true)
  })

  it('entering valid email and clicking CTA advances to step 2', async () => {
    const wrapper = mount(SignUpView, { global: { stubs } })

    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="sign-up-step-email"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="sign-up-step-password"]').exists()).toBe(true)
    expect(wrapper.find('#password').exists()).toBe(true)
    expect(wrapper.find('#confirm-password').exists()).toBe(true)
  })

  it('step 2 shows submit button and acknowledgement text with terms/privacy links', async () => {
    const wrapper = mount(SignUpView, { global: { stubs } })

    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('sign-up:cta')
    expect(wrapper.html()).toContain('sign-up:cta-acknowledgement')
  })
})
