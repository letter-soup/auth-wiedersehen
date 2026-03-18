export type SignUpStepType = 'email' | 'password' | 'tos'
export type SignUpStep = {
  step: number
  type: SignUpStepType
  description: string
}
