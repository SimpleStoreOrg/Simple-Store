export type RoleStatus = 0 | 1 | 2

export interface LoginRequest {
    username: string
    email: string
    password: string
    role: RoleStatus
}

export interface TokenResponse {
    accessToken: string
    refreshToken: string
    expiresAt: string
}