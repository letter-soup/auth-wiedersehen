<script setup lang="ts">
import ThemeToggle from '@/components/primitives/theme-toggle/ThemeToggle.vue'
import { Toaster } from '@/components/ui/sonner'
import { Button } from '@/components/ui/button'
import { toast } from 'vue-sonner'
</script>

<template>
  <ThemeToggle />
  <h1 class="text-3xl font-bold underline">Hello world!</h1>
  <Button @click="() => toast('My first toast')">Click me</Button>
  <Toaster />
</template>

<style scoped></style>
