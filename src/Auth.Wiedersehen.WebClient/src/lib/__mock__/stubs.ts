import { createStub } from './component-stub'

export const LanguageSwitcherStub = createStub('LanguageSwitcher')
export const ThemeToggleStub = createStub('ThemeToggle')
export const FooterSectionStub = createStub('FooterSection')
export const RouterViewStub = createStub('RouterView')
export const ToasterStub = createStub('Toaster')
export const SeparatorStub = createStub('Separator', { template: '<hr />' })
export const ButtonStub = createStub('Button', { template: '<button><slot /></button>' })
export const InputStub = createStub('Input', {
  template: '<input v-bind="$attrs" />',
  inheritAttrs: true,
})
export const LabelStub = createStub('Label', { template: '<label><slot /></label>' })
export const FormMessageStub = createStub('FormMessage', { template: '<span />' })
