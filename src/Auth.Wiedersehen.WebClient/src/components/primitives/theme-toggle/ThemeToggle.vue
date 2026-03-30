<script setup lang="ts">
import { Moon, Sun } from 'lucide-vue-next'
import { useColorMode } from '@vueuse/core'
import { Button } from '@/components/ui/button'

// Pass { disableTransition: false } to enable transitions
const mode = useColorMode()

const toggleTheme = () => {
  mode.value = mode.value === 'light' ? 'dark' : 'light'
}
</script>

<template>
  <Button variant="ghost" @click="toggleTheme()">
    <Moon
      class="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0"
    />
    <Sun
      class="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100"
    />
    <span class="sr-only">Toggle theme</span>
  </Button>
</template>
