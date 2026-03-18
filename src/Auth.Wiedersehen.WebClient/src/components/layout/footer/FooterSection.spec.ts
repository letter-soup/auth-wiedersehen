import { describe, expect, it, vi } from 'vitest'
import { mount } from '@/lib/tests/mount'
import FooterSection from '@/components/layout/footer/FooterSection.vue'

// Mock LanguageSwitcher component
vi.mock('@/components/primitives/language-switcher/LanguageSwitcher.vue', () => ({
  default: 'language-switcher',
}))

describe('FooterSection (snapshot)', () => {
  it('renders properly', async () => {
    const wrapper = mount(FooterSection)

    expect(wrapper).toBeTruthy()
    expect(wrapper.html()).toMatchSnapshot()
  })
})
