<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import { toast } from '@/components/ui/toast'

const signInHandler = () => {
  toast({
    title: 'You successfully signed in!',
  })
}
</script>

<template>
  <div class="w-full h-full flex items-center justify-center py-12">
    <div class="mx-auto grid w-[350px] gap-6">
      <div class="grid gap-2 text-center">
        <h1 class="text-3xl font-bold">
          {{ $t('sign-in:header') }}
        </h1>
        <p class="text-balance text-muted-foreground">
          {{ $t('sign-in:description') }}
        </p>
      </div>
      <div class="grid gap-4">
        <div class="grid gap-2">
          <Label for="email">{{ $t('sign-in:email') }}</Label>
          <Input id="email" type="email" placeholder="m@example.com" required />
        </div>
        <div class="grid gap-2">
          <div class="flex items-center">
            <Label for="password">{{ $t('sign-in:password') }}</Label>
            <RouterLink
              to="/reset-password"
              class="ml-auto inline-block text-sm underline text-muted-foreground hover:text-primary"
            >
              {{ $t('sign-in:password-reset') }}
            </RouterLink>
          </div>
          <Input id="password" type="password" required />
        </div>
        <Button type="submit" class="w-full" @click="signInHandler">
          {{ $t('sign-in:cta') }}
        </Button>
      </div>

      <Separator class="mt-2" :label="$t('sign-in:no-account')" orientation="horizontal" />
      <Button variant="link">
        <RouterLink to="/sign-up">
          {{ $t('sign-in:sign-up') }}
        </RouterLink>
      </Button>
    </div>
  </div>
</template>
