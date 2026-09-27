import { apiClient } from '../api/apiClient'
import type {
    CreateMarketRequest,
    UpdateMarketRequest,
} from '../../types/market'

const MARKET_SERVICE_URL = 'https://localhost:7004/api'

export async function getAllMarkets() {
    return apiClient(
        '/Market?pageNumber=1&pageSize=100',
        {},
        MARKET_SERVICE_URL
    )
}

export async function getMarketById(
    id: number
) {
    return apiClient(
        `/Market/${id}`,
        {},
        MARKET_SERVICE_URL
    )
}

export async function createMarket(
    request: CreateMarketRequest
) {
    return apiClient(
        '/Market',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        MARKET_SERVICE_URL
    )
}

export async function updateMarket(
    id: number,
    request: UpdateMarketRequest
) {
    return apiClient(
        `/Market/${id}`,
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        MARKET_SERVICE_URL
    )
}

export async function deleteMarket(id: number) {
    return apiClient(
        `/Market/${id}`,
        {
            method: 'DELETE',
        },
        MARKET_SERVICE_URL
    )
}