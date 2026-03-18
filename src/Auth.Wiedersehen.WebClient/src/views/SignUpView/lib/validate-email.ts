import type { TFormValidationCallback } from '@/lib/types'

export async function validateEmail(
  _: Record<string, string>, // values
  validate: TFormValidationCallback,
  nextStep: () => void,
  __: (field: string, error: string) => void, // setFieldError
  ___: (key: string) => string, // t
) {
  const validationResult = await validate()
  if (!validationResult.valid) {
    return
  }

  if (validationResult.valid) {
    nextStep()
  }
}
