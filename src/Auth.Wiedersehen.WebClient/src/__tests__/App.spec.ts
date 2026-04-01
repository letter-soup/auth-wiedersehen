import { describe, it, expect } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import App from '../App.vue'
import { RouterViewStub, ToasterStub, FooterSectionStub } from '@/lib/__mock__/stubs'

const stubs = {
  RouterView: RouterViewStub,
  Toaster: ToasterStub,
  FooterSection: FooterSectionStub,
}

describe('App (snapshot)', () => {
  it('renders correctly (snapshot)', () => {
    const wrapper = mount(App, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('App', () => {
  it('renders main element with RouterView', () => {
    const wrapper = mount(App, { global: { stubs } })
    const main = wrapper.find('main')
    expect(main.exists()).toBe(true)
    expect(main.find('[data-testid="router-view-stub"]').exists()).toBe(true)
  })

  it('renders FooterSection', () => {
    const wrapper = mount(App, { global: { stubs } })
    expect(wrapper.find('[data-testid="footer-section-stub"]').exists()).toBe(true)
  })
})
