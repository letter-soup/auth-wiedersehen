import { describe, it, expect, vi } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import ThemeToggle from './ThemeToggle.vue'
import { ButtonStub } from '@/lib/__mock__/stubs'
import { MoonIcon, SunIcon } from '@/lib/__mock__/lucide-vue-next.mock'

const mockMode = { value: 'light' }
vi.mock('@vueuse/core', () => ({
  useColorMode: () => mockMode,
}))

const stubs = {
  Moon: MoonIcon,
  Sun: SunIcon,
  Button: ButtonStub,
}

describe('ThemeToggle (snapshot)', () => {
  it('renders correctly (snapshot)', () => {
    const wrapper = mount(ThemeToggle, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('ThemeToggle', () => {
  it('displays sr-only text', () => {
    const wrapper = mount(ThemeToggle, { global: { stubs } })
    expect(wrapper.find('.sr-only').text()).toBe('theme:toggle')
  })

  it.each([
    ['light', 'dark'],
    ['dark', 'light'],
  ])('given %s theme should toggle color mode to %s on click', async (theme, expected) => {
    mockMode.value = theme

    const wrapper = mount(ThemeToggle, { global: { stubs } })
    await wrapper.find('button').trigger('click')
    expect(mockMode.value).toBe(expected)
  })
})
