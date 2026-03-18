import { describe, expect, it, vi } from 'vitest'
import { mount } from '@/lib/tests/mount'
import LanguageSwitcher from '@/components/primitives/language-switcher/LanguageSwitcher.vue'

vi.mock('lucide-vue-next', () => ({
  Languages: 'languages-icon',
  ChevronDown: 'chevron-down-icon',
  Check: 'check-icon',
}))

describe('LanguageSwitcher (snapshot)', () => {
  it('renders properly', async () => {
    const wrapper = mount(LanguageSwitcher)
    await wrapper.vm.$nextTick()

    expect(wrapper).toBeTruthy()
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('LanguageSwitcher', () => {
  it('given locale:en-US when locale:ru-RU selected then updates locale', async () => {
    const wrapper = mount(LanguageSwitcher)
    await wrapper.vm.$nextTick()

    const select = wrapper.find('[data-testid=language-switcher]')
    expect(select.attributes()['data-selected-locale']).toBe('en-US')
    await select.setValue('ru-RU')
    expect(select.attributes()['data-selected-locale']).toBe('ru-RU')
  })
})
