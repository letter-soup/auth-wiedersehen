import { createStub } from './component-stub'

export const LanguagesIcon = createStub('LanguagesIcon', {
  template: '<span data-testid="languages-icon" />',
})

export const MoonIcon = createStub('MoonIcon', {
  template: '<span data-testid="moon-icon" />',
})

export const SunIcon = createStub('SunIcon', {
  template: '<span data-testid="sun-icon" />',
})

export const lucideVueNextMock: Record<string, unknown> = {
  Languages: LanguagesIcon,
  Moon: MoonIcon,
  Sun: SunIcon,
}
