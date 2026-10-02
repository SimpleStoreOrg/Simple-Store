import { apiClient } from './api/apiClient'

export interface LoginRequest {
    username: string
    email: string
    password: string
    role: number
}

export interface LoginResponse {
    accessToken: string
    refreshToken: string
}

export interface RegisterCustomerRequest {
    username: string
    email: string
    password: string
}

export async function login(
    request: LoginRequest
): Promise<LoginResponse> {
    return apiClient(
        '/Authentication/login',
        {
            method: 'POST',
            body: JSON.stringify(request),
        }
    )
}

export async function registerCustomer(
    request: RegisterCustomerRequest
): Promise<null> {
    return apiClient(
        '/Authentication/registercustomer',
        {
            method: 'POST',
            body: JSON.stringify(request),
        }
    )
}