import { createI18n } from 'vue-i18n'
import enUS from '../assets/i18n/en-us.json'
import ruRU from '../assets/i18n/ru-ru.json'

export const locales = {
  'en-US': enUS,
  'ru-RU': ruRU,
}

export type TLocale = keyof typeof locales

export const locale: TLocale = 'en-US'
export const fallbackLocale: TLocale = 'en-US'

export const i18n = createI18n({
  legacy: false,
  locale,
  fallbackLocale,
  messages: locales,
})
