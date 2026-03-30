import { describe, expect, it, vi } from 'vitest'
import FooterSection from '@/components/layout/footer-section/FooterSection.vue'
import {mount} from "@/lib/__tests__/mount.ts";

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
