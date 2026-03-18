<script setup lang="ts">
import { FOOTER_LINK_GROUPS } from '@/components/layout/footer/constants'
import { Separator } from '@/components/ui/separator'
import LanguageSwitcher from '@/components/primitives/language-switcher/LanguageSwitcher.vue'
</script>

<template>
  <footer class="grid grid-cols-12 gap-2 pb-4">
    <div
      class="col-start-3 col-span-8 lg:col-start-4 lg:col-span-6 max-sm:col-start-2 max-sm:col-span-10"
    >
      <div class="grid gap-6 grid-cols-3">
        <div v-for="{ key, links } in FOOTER_LINK_GROUPS" :key class="flex justify-center">
          <div class="w-fit">
            <h2 class="mb-4 text-sm font-semibold text-gray-900 uppercase dark:text-white">
              {{ $t(key) }}
            </h2>
            <ul class="text-gray-600 dark:text-gray-400 space-y-2">
              <li v-for="link in links" :key="link.label">
                <a class="hover:underline" target="_blank" :href="link.url">
                  {{ $t(link.label) }}
                </a>
              </li>
            </ul>
          </div>
        </div>
      </div>
      <Separator orientation="horizontal" class="my-5" />
      <div class="flex justify-between items-center">
        <span class="text-sm text-gray-500 sm:text-center dark:text-gray-400">
          © 2025 <RouterLink to="/" class="hover:underline">Auth Wiedersehen™</RouterLink>.
          {{ $t('footer:all-rights-reserved') }}
        </span>
        <LanguageSwitcher />
      </div>
    </div>
  </footer>
</template>

<style scoped></style>
