import { apiClient } from '../api/apiClient'
import type { Customer } from '../../types/customer'

export interface UpdateCustomerRequest {
    name: string
    surname: string
    email: string
    username: string
    phoneNumber: string
}

export async function getCurrentCustomer(): Promise<Customer> {
    return apiClient('/Customer/me')
}

export async function updateCurrentCustomer(
    request: UpdateCustomerRequest
): Promise<Customer> {
    return apiClient('/Customer', {
        method: 'PUT',
        body: JSON.stringify(request),
    })
}

export async function getAllCustomers(): Promise<{
    items: Customer[]
}> {
    return apiClient('/Customer')
}