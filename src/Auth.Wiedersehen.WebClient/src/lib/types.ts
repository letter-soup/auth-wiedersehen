import type { FormValidationResult, GenericObject } from 'vee-validate'

export type TFormValidationCallback = () => Promise<
  FormValidationResult<GenericObject, GenericObject>
>
