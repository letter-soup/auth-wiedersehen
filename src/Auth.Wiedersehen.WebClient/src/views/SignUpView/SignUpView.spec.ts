import '@/views/SignUpView/lib/__mock__/validate-email.mock'
import { describe, expect, it } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mount } from '@/lib/tests/mount'
import SignUpView from '@/views/SignUpView/SignUpView.vue'

describe('SignUpView (snapshot)', () => {
  it('should render email step by default', async () => {
    const wrapper = mount(SignUpView)
    expect(wrapper).toBeTruthy()

    expect(wrapper.find('[data-testid=sign-up-step-email]').exists()).toBeTruthy()
    expect(wrapper.find('[data-testid=sign-up-step-password]').exists()).toBeFalsy()

    expect(wrapper.html()).toMatchSnapshot()
  })

  it('given valid email, should render password step on cta click', async () => {
    const wrapper = mount(SignUpView)
    expect(wrapper).toBeTruthy()

    await wrapper.find('[id=email]').setValue('valid@email.com')
    await wrapper.find('[id=cta]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid=sign-up-step-email]').exists()).toBeFalsy()
    expect(wrapper.find('[data-testid=sign-up-step-password]').exists()).toBeTruthy()

    expect(wrapper.html()).toMatchSnapshot()
  })
})
