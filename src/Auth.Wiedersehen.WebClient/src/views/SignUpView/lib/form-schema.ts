import * as z from 'zod'
import { PASSWORD_REQUIREMENTS } from '@/views/SignUpView/lib/constants'

export const createFormSchema = (t: (key: string) => string) => {
  const translations = {
    weakPassword: t('error:sign-up:weak-password'),
    passwordMismatch: t('error:sign-up:password-mismatch'),
  }

  return [
    z.object({
      email: z.string().email(),
    }),
    z
      .object({
        password: z
          .string()
          .min(8, translations.weakPassword)
          .regex(PASSWORD_REQUIREMENTS.lowerCaseSymbols, translations.weakPassword)
          .regex(PASSWORD_REQUIREMENTS.upperCaseSymbols, translations.weakPassword)
          .regex(PASSWORD_REQUIREMENTS.numericSymbols, translations.weakPassword)
          .regex(PASSWORD_REQUIREMENTS.specialSymbols, translations.weakPassword),
        confirmPassword: z.string(),
      })
      .refine(
        (values) => {
          return values.password === values.confirmPassword
        },
        {
          message: translations.passwordMismatch,
          path: ['confirmPassword'],
        },
      ),
  ]
}
