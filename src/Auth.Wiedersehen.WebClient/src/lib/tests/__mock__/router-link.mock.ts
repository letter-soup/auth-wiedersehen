// Mock RouterLink component
export const RouterLinkMock = {
  name: 'RouterLink',
  props: ['to'],
  template: '<a :href="to"><slot /></a>',
}
