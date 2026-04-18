<script setup lang="ts">
import { toTypedSchema } from '@vee-validate/zod'
import { computed, type ComputedRef, type Ref, ref, watch } from 'vue'
import { onBeforeRouteUpdate, useRoute, useRouter } from 'vue-router'
import { Stepper, StepperItem, StepperTrigger } from '@/components/ui/stepper'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { toast } from 'vue-sonner'
import { Separator } from '@/components/ui/separator'
import { getSignUpStep, SIGN_UP_STEPS } from '@/views/sign-up/lib/constants'
import { useI18n } from 'vue-i18n'
import type { TFormValidationCallback } from '@/lib/types'
import { createFormSchema } from '@/views/sign-up/lib/form-schema'
import { validateEmail } from '@/views/sign-up/lib/validate-email'
import { createUser } from '@/lib/api/endpoints.ts'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

const clientId = computed(() => (route.query.client_id as string) || undefined)
const redirectUri = computed(() => (route.query.redirect_uri as string) || undefined)

const initialStep: number = 1
const stepIndex: Ref<number> = ref(initialStep)
const stepDescription: ComputedRef<string> = computed(
  () => getSignUpStep(stepIndex.value).description,
)
const formSchema: ComputedRef = computed(() => createFormSchema(t))

onBeforeRouteUpdate((to, from, next) => {
  if (to.name !== from.name) {
    return next()
  }

  const toStep = parseInt(to.query.step as string) || initialStep
  if (toStep < stepIndex.value) {
    stepIndex.value = toStep
  }
  return next()
})

watch(
  () => stepIndex.value,
  (value) => {
    const query: Record<string, string | number> = { step: value }
    if (clientId.value) query.client_id = clientId.value
    if (redirectUri.value) query.redirect_uri = redirectUri.value
    router.push({
      ...router.currentRoute.value,
      query,
    })
  },
)

const signUpAcknowledgementMap: ComputedRef<Record<string, string>> = computed(() => ({
  termsOfService: `<a target="_blank" href="/terms" class="underline underline-offset-4 hover:text-primary">${t('sign-up:terms-of-service')}</a>`,
  privacyPolicy: `<a target="_blank" href="/privacy" class="underline underline-offset-4 hover:text-primary">${t('sign-up:privacy-policy')}</a>`,
}))

async function onSubmit(
  e: Event,
  validate: TFormValidationCallback,
  values: Record<string, string>,
) {
  const validationResult = await validate()

  if (stepIndex.value === SIGN_UP_STEPS.length && validationResult.valid) {
    try {
      const response = await createUser(
        values.email,
        values.password,
        true,
        clientId.value,
        redirectUri.value,
      )
      toast(t('sign-up:success'))
      if (response.redirectUri) {
        window.location.href = response.redirectUri
      } else {
        await router.push('/')
      }
    } catch (error) {
      if (error instanceof Response) {
        toast(t('sign-up:error'), { description: error.statusText })
      } else {
        toast(t('sign-up:error'))
      }
    }
  }
}
</script>

<template>
  <div class="w-full h-full flex items-center justify-center py-12">
    <div class="mx-auto grid w-87.5 gap-6">
      <div class="grid gap-2 text-center">
        <h1 class="text-3xl font-bold">
          {{ $t('sign-up:header') }}
        </h1>
        <p class="text-balance text-muted-foreground">
          {{ $t(stepDescription) }}
        </p>
      </div>
      <Form
        v-slot="{ values, validate, setFieldError }"
        keep-values
        :validation-schema="toTypedSchema(formSchema[stepIndex - 1])"
      >
        <Stepper v-slot="{ nextStep }" v-model="stepIndex" class="block w-full">
          <form @submit.prevent="(e) => onSubmit(e, validate, values)">
            <div class="flex w-full flex-start gap-2">
              <StepperItem
                v-for="step in SIGN_UP_STEPS"
                :key="step.step"
                class="hidden"
                :step="step.step"
              >
                <StepperTrigger as-child>
                  <div />
                </StepperTrigger>
              </StepperItem>
            </div>

            <div
              class="flex flex-col gap-4"
              v-if="stepIndex === 1"
              data-testid="sign-up-step-email"
            >
              <FormField v-slot="{ componentField }" name="email">
                <FormItem>
                  <FormLabel>{{ $t('sign-up:email') }}</FormLabel>
                  <FormControl>
                    <Input
                      id="email"
                      type="email"
                      v-bind="componentField"
                      @keydown.enter.prevent="
                        validateEmail(values, validate, nextStep, setFieldError, $t)
                      "
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>

              <Button
                id="cta"
                class="w-full"
                @click="validateEmail(values, validate, nextStep, setFieldError, $t)"
                type="button"
              >
                {{ $t('sign-up:step:cta') }}
              </Button>
            </div>

            <div
              class="flex flex-col gap-4"
              v-if="stepIndex === 2"
              data-testid="sign-up-step-password"
            >
              <FormField v-slot="{ componentField }" name="password">
                <FormItem>
                  <FormLabel>{{ $t('sign-up:password') }}</FormLabel>
                  <FormControl>
                    <Input id="password" type="password" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>

              <FormField v-slot="{ componentField }" name="confirmPassword">
                <FormItem>
                  <FormLabel>{{ $t('sign-up:confirm-password') }}</FormLabel>
                  <FormControl>
                    <Input id="confirm-password" type="password" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>

              <Button type="submit" class="w-full">
                {{ $t('sign-up:cta') }}
              </Button>

              <p
                class="px-8 text-center text-sm text-muted-foreground"
                v-html="$t('sign-up:cta-acknowledgement', signUpAcknowledgementMap)"
              />
            </div>
          </form>
        </Stepper>
      </Form>

      <Separator class="mt-2" :label="$t('sign-up:has-account')" orientation="horizontal" />
      <Button variant="link">
        <RouterLink to="/sign-in">
          {{ $t('sign-up:sign-in') }}
        </RouterLink>
      </Button>
    </div>
  </div>
</template>
