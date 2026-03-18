import { mount as vueMount, type ComponentMountingOptions } from '@vue/test-utils'
import { RouterLinkMock } from '@/lib/tests/__mock__/router-link.mock'
import merge from 'merge'
import type { Component } from 'vue'

/**
 * A wrapper around vue-tests mount that applies default options
 * to reduce boilerplate in tests.
 *
 * @param component The component to mount
 * @param options Options to pass to mount
 * @returns The mounted wrapper
 */
export function mount<T extends Component>(component: T, options?: ComponentMountingOptions<T>) {
  // Default options that will be applied to all tests
  const defaultOptions = {
    global: {
      stubs: {
        RouterLink: RouterLinkMock,
      },
      mocks: {
        $t: (key: string) => key, // Mock translation function
      },
    },
  }

  // Merge the default options with the user-provided options
  const mergedOptions = merge(defaultOptions, options || {})

  // Call the original mount function with the merged options
  return vueMount(component, mergedOptions)
}
