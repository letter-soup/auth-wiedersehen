import type { SignUpStep } from '@/views/sign-up/lib/types'

export const PASSWORD_REQUIREMENTS = {
  lowerCaseSymbols: /[a-z]+/,
  upperCaseSymbols: /[A-Z]+/,
  numericSymbols: /[0-9]+/,
  specialSymbols: /[!@#$%^&*()\-_+.]+/,
}

export const SIGN_UP_STEPS: SignUpStep[] = [
  {
    step: 1,
    type: 'email',
    description: 'sign-up:step-email:description',
  },
  {
    step: 2,
    type: 'password',
    description: 'sign-up:step-password:description',
  },
]

export function getSignUpStep(step: number): SignUpStep {
  const stepObj = SIGN_UP_STEPS.find((s) => s.step === step) ?? null
  if (stepObj == null) {
    throw new RangeError('Invalid step')
  }

  return stepObj
}
