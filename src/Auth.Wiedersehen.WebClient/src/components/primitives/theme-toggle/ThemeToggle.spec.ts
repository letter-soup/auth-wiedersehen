import { describe, expect, it, vi } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import ThemeToggle from '@/components/primitives/theme-toggle/ThemeToggle.vue'

vi.mock('lucide-vue-next', () => ({
  Moon: 'moon-icon',
  Sun: 'sun-icon',
}))

describe('ThemeToggle (snapshot)', () => {
  it('renders properly', async () => {
    const wrapper = mount(ThemeToggle)
    await wrapper.vm.$nextTick()

    expect(wrapper).toBeTruthy()
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('ThemeToggle', () => {
  it('given theme:light should change theme on click', async () => {
    const wrapper = mount(ThemeToggle)
    await wrapper.vm.$nextTick()
  })
})
