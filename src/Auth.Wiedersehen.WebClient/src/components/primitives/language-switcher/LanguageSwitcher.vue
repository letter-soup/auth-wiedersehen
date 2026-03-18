<script setup lang="ts">
import { Select, SelectContent, SelectItem, SelectTrigger } from '@/components/ui/select'
import { Languages } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const { locale: selectedLocale, availableLocales } = useI18n()
</script>

<template>
  <Select
    v-model="selectedLocale"
    data-testid="language-switcher"
    :data-selected-locale="selectedLocale"
  >
    <SelectTrigger class="w-fit h-fit">
      <Languages :size="16" />
    </SelectTrigger>
    <SelectContent>
      <SelectItem v-for="locale in availableLocales" :key="locale" :value="locale">
        {{ $t(`language:${locale}`) }}
      </SelectItem>
    </SelectContent>
  </Select>
</template>

<style scoped></style>
