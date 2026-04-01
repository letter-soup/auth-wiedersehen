import { describe, it, expect, vi } from 'vitest'
import { mount } from '@/lib/__tests__/mount'
import LanguageSwitcher from './LanguageSwitcher.vue'
import { createStub } from '@/lib/__mock__/component-stub'

const { mockLocale, mockAvailableLocales, vueI18nMock } = await vi.hoisted(
  () => import('@/lib/__mock__/vue-i18n.mock'),
)
const { lucideVueNextMock } = await vi.hoisted(() => import('@/lib/__mock__/lucide-vue-next.mock'))

vi.mock('vue-i18n', () => vueI18nMock)
vi.mock('lucide-vue-next', () => lucideVueNextMock)

const stubs = {
  Select: createStub('Select', {
    template:
      '<div data-testid="language-switcher" :data-selected-locale="$attrs[\'data-selected-locale\']"><slot /></div>',
    props: ['modelValue'],
  }),
  SelectTrigger: createStub('SelectTrigger', { template: '<button><slot /></button>' }),
  SelectContent: createStub('SelectContent', { template: '<div><slot /></div>' }),
  SelectItem: createStub('SelectItem', {
    template: '<div data-testid="select-item"><slot /></div>',
    props: ['value'],
  }),
}

describe('LanguageSwitcher (snapshot)', () => {
  it('renders correctly (snapshot)', () => {
    const wrapper = mount(LanguageSwitcher, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('LanguageSwitcher', () => {
  it('renders all available locales', () => {
    const wrapper = mount(LanguageSwitcher, { global: { stubs } })
    const items = wrapper.findAll('[data-testid="select-item"]')
    expect(items).toHaveLength(mockAvailableLocales.length)
  })

  it.each(mockAvailableLocales)(
    'displays data-selected-locale matching current locale',
    (locale) => {
      mockLocale.value = locale
      const wrapper = mount(LanguageSwitcher, { global: { stubs } })
      const select = wrapper.find('[data-testid="language-switcher"]')
      expect(select.attributes('data-selected-locale')).toBe(locale)
    },
  )
})
