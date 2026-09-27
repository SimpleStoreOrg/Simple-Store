import { apiClient } from '../api/apiClient'

import type {
    Admin,
    CreateAdminRequest,
    UpdateAdminRequest,
} from '../../types/admin'

const USER_SERVICE_URL =
    'https://localhost:7003/api'

export async function getAllAdmins() {
    return apiClient(
        '/Admin?pageNumber=1&pageSize=100',
        {},
        USER_SERVICE_URL
    )
}

export async function getCurrentAdmin(): Promise<Admin> {
    return apiClient(
        '/Admin/me',
        {},
        USER_SERVICE_URL
    )
}

export async function updateCurrentAdmin(
    request: UpdateAdminRequest
): Promise<Admin> {
    return apiClient(
        '/Admin',
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function createMarketAdmin(
    request: CreateAdminRequest
) {
    return apiClient(
        '/Admin/marketadmin',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function createSuperAdmin(
    request: CreateAdminRequest
) {
    return apiClient(
        '/Admin/superadmin',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function updateAdmin(
    _id: number,
    request: UpdateAdminRequest
) {
    return apiClient(
        '/Admin',
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function deleteAdmin(id: number) {
    return apiClient(
        `/Admin/${id}`,
        {
            method: 'DELETE',
        },
        USER_SERVICE_URL
    )
}