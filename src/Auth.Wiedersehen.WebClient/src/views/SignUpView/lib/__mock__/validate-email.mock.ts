import { vi } from 'vitest'
import type { TFormValidationCallback } from '@/lib/types'

vi.mock('@/views/SignUpView/lib/validate-email', () => ({
  validateEmail: async (
    values: Record<string, string>,
    validate: TFormValidationCallback,
    nextStep: () => void,
    setFieldError: (field: string, error: string) => void,
    t: (key: string) => string,
  ) => {
    nextStep()
  },
}))
