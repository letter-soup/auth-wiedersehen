import { ref } from 'vue'
import {locale, locales, type TLocale} from "@/lib/locale.ts";

export const mockLocale = ref(locale)
export const mockAvailableLocales = Object.keys(locales) as TLocale[]

export const vueI18nMock = {
  useI18n: () => ({
    locale: mockLocale,
    availableLocales: mockAvailableLocales,
  }),
}
