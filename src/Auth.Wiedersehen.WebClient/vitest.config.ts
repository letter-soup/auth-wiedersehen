import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: 'vitest.setup.ts',
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      coverage: {
        provider: 'istanbul',
        reporter: ['html', 'cobertura'],
        reportOnFailure: true,
        exclude: [
          '**/assets/**',
          '**/e2e/**',
          '**/*.cjs',

          '**/src/App.vue',
          '**/src/main.ts',
          '**/src/router/',
          '**/src/components/ui/**',

          '**/src/utils/locale.ts',

          '**/*.constant.ts',
          '**/*.model.ts',
          '**/*.spec.ts',

          '**/__mock__/**',
          '**/__fixture__/**',
          '**/__temp__/**',
        ],
      }
    }
  })
)
