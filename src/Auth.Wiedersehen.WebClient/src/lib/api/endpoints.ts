import type { CreateUserRequest, CreateUserResponse } from '@/lib/api/schema.ts'

export async function checkEmailAvailability(email: string): Promise<void> {
  const response = await fetch(`/api/v1/email/is-available?email=${encodeURIComponent(email)}`)

  if (!response.ok) {
    throw response
  }
}

export async function createUser(
  email: string,
  password: string,
  termsAccepted: boolean,
  clientId?: string,
  redirectUri?: string,
): Promise<CreateUserResponse> {
  const body: CreateUserRequest = { email, password, termsAccepted }
  if (clientId) body.clientId = clientId
  if (redirectUri) body.redirectUri = redirectUri

  const response = await fetch('/api/v1/user', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw response
  }

  return (await response.json()) as CreateUserResponse
}
