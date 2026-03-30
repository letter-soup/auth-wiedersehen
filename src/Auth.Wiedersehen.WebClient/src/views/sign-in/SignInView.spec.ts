import { describe, expect, it } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import SignInView from '@/views/sign-in/SignInView.vue'

describe('SignInView (snapshot)', () => {
  it('renders properly', async () => {
    const wrapper = mount(SignInView)

    expect(wrapper).toBeTruthy()
    expect(wrapper.html()).toMatchSnapshot()
  })
})
