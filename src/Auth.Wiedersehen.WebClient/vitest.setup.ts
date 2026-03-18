import { config } from '@vue/test-utils'
import { i18n } from './src/lib/locale'
import { beforeEach } from 'vitest'
import {
  VueRouterMock,
  createRouterMock,
  injectRouterMock,
} from 'vue-router-mock'

// create one router per test file
const router = createRouterMock()
beforeEach(() => {
  router.reset() // reset the router state
  injectRouterMock(router)
})

// Add properties to the wrapper
config.plugins.VueWrapper.install(VueRouterMock)
config.global.plugins = [i18n]