export interface CreateUserRequest {
  email: string
  password: string
  termsAccepted: boolean
  clientId?: string
  redirectUri?: string
}

export interface CreateUserResponse {
  userId: string
  redirectUri?: string
}
