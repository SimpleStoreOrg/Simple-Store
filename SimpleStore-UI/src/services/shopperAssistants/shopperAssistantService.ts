import { apiClient } from '../api/apiClient'

import type {
    CreateShopperAssistantRequest,
    ShopperAssistantsResponse,
    ShopperAssistant,
    UpdateShopperAssistantRequest,
} from '../../types/shopperAssistant'

const USER_SERVICE_URL =
    import.meta.env.VITE_USER_SERVICE_URL ??
    'https://localhost:7003/api'

export async function getAllShopperAssistants(
    pageNumber = 1,
    pageSize = 100,
    position?: number
): Promise<ShopperAssistantsResponse> {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
    })

    if (position !== undefined) {
        params.append('positions', position.toString())
    }

    return apiClient(
        `/ShopperAssistant?${params.toString()}`,
        {},
        USER_SERVICE_URL
    )
}

export async function getShopperAssistantById(
    id: number
): Promise<ShopperAssistant> {
    return apiClient(
        `/ShopperAssistant/${id}`,
        {},
        USER_SERVICE_URL
    )
}

export async function getCurrentShopperAssistant(): Promise<ShopperAssistant> {
    return apiClient(
        '/ShopperAssistant/me',
        {},
        USER_SERVICE_URL
    )
}

export async function updateCurrentShopperAssistant(
    id: number,
    request: UpdateShopperAssistantRequest
): Promise<ShopperAssistant> {
    return apiClient(
        `/ShopperAssistant/${id}`,
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function createShopperAssistant(
    request: CreateShopperAssistantRequest
): Promise<ShopperAssistant> {
    return apiClient(
        '/ShopperAssistant',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function updateShopperAssistant(
    id: number,
    request: UpdateShopperAssistantRequest
): Promise<ShopperAssistant> {
    return apiClient(
        `/ShopperAssistant/${id}`,
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        USER_SERVICE_URL
    )
}

export async function deleteShopperAssistant(
    id: number
): Promise<null> {
    return apiClient(
        `/ShopperAssistant/${id}`,
        {
            method: 'DELETE',
        },
        USER_SERVICE_URL
    )
}