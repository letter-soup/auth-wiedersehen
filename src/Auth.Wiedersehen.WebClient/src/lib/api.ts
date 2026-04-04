export async function checkEmailAvailability(email: string): Promise<void> {
  const response = await fetch(`/api/v1/email/is-available?Email=${encodeURIComponent(email)}`)

  if (!response.ok) {
    throw response
  }
}

export interface CreateUserRequest {
  email: string
  password: string
  termsAccepted: boolean
}

export async function createUser(
  email: string,
  password: string,
  termsAccepted: boolean,
): Promise<void> {
  const body: CreateUserRequest = { email, password, termsAccepted }

  const response = await fetch('/api/v1/user', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw response
  }
}
