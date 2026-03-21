<script setup lang="ts">
import ThemeToggle from '@/components/primitives/theme-toggle/ThemeToggle.vue'
import { Toaster } from '@/components/ui/sonner'
import { Button } from '@/components/ui/button'
import { toast } from 'vue-sonner'
import LanguageSwitcher from '@/components/primitives/language-switcher/LanguageSwitcher.vue'
</script>

<template>
  <ThemeToggle />
  <h1 class="text-3xl font-bold underline">Hello world!</h1>
  <Button @click="() => toast('My first toast')">{{ $t('sign-in:header') }}</Button>
  <LanguageSwitcher />
  <Toaster />
</template>

<style scoped></style>
