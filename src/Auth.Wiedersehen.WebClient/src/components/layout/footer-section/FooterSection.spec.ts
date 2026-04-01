import { describe, it, expect } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import FooterSection from './FooterSection.vue'
import { FOOTER_LINK_GROUPS } from './constants'
import { LanguageSwitcherStub, ThemeToggleStub, SeparatorStub } from '@/lib/__mock__/stubs'

const stubs = {
  LanguageSwitcher: LanguageSwitcherStub,
  ThemeToggle: ThemeToggleStub,
  Separator: SeparatorStub,
}

describe('FooterSection (snapshot)', () => {
  it('renders correctly (snapshot)', () => {
    const wrapper = mount(FooterSection, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('FooterSection', () => {
  it('renders all footer link groups with correct hrefs', () => {
    const wrapper = mount(FooterSection, { global: { stubs } })

    for (const group of FOOTER_LINK_GROUPS) {
      for (const link of group.links) {
        const anchor = wrapper.find(`a[href="${link.url}"]`)
        expect(anchor.exists()).toBe(true)
      }
    }
  })
})
