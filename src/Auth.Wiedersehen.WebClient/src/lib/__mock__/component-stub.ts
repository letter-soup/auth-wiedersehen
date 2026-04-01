function toKebabCase(name: string): string {
  return name
    .replace(/([a-z])([A-Z])/g, '$1-$2')
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1-$2')
    .toLowerCase()
}

interface StubOptions {
  template?: string
  props?: string[]
  inheritAttrs?: boolean
}

export function createStub(name: string, options?: StubOptions) {
  const testId = `${toKebabCase(name)}-stub`
  const template = options?.template ?? `<div data-testid="${testId}"><slot /></div>`

  return {
    template,
    ...(options?.props ? { props: options.props } : {}),
    ...(options?.inheritAttrs !== undefined ? { inheritAttrs: options.inheritAttrs } : {}),
  }
}
