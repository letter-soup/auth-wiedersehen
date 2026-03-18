import type { TFormValidationCallback } from '@/lib/types'

export async function validateEmail(
  values: Record<string, string>,
  validate: TFormValidationCallback,
  nextStep: () => void,
  setFieldError: (field: string, error: string) => void,
  t: (key: string) => string,
) {
  const validationResult = await validate()
  if (!validationResult.valid) {
    return
  }

  if (validationResult.valid) {
    nextStep()
  }
}
