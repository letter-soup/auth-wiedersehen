import type { TFormValidationCallback } from '@/lib/types'
import { checkEmailAvailability } from '@/lib/api'

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

  try {
    await checkEmailAvailability(values.email)
    nextStep()
  } catch (error) {
    if (error instanceof Response && error.status === 409) {
      setFieldError('email', t('error:email-already-taken'))
    } else {
      throw error
    }
  }
}
