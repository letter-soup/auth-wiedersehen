<script setup lang="ts">
import { RouterView } from 'vue-router'
import FooterSection from '@/components/layout/footer/FooterSection.vue'
import 'vue-sonner/style.css'
import { Toaster } from '@/components/ui/sonner'
</script>

<template>
  <main class="h-full">
    <RouterView />
  </main>
  <Toaster />
  <FooterSection />
</template>

<style scoped></style>
