import { apiClient } from '../api/apiClient'

import type {
    TopProductsByCategoryResponse,
    TotalRevenueResponse,
} from '../../types/report'

const ORDER_SERVICE_URL =
    import.meta.env.VITE_ORDER_SERVICE_URL ??
    'https://localhost:7001/api'

export async function getTotalRevenue(
    from?: string,
    to?: string
): Promise<TotalRevenueResponse> {
    const params = new URLSearchParams()

    if (from) {
        params.append('from', from)
    }

    if (to) {
        params.append('to', to)
    }

    const query = params.toString()

    return apiClient(
        `/Reports/totalrevenue${query ? `?${query}` : ''}`,
        {},
        ORDER_SERVICE_URL
    )
}

export async function getTopProductsByCategory(
    pageNumber = 1,
    pageSize = 20,
    categoryIds?: number[]
): Promise<TopProductsByCategoryResponse> {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
    })

    if (categoryIds && categoryIds.length > 0) {
        categoryIds.forEach((id) =>
            params.append('categoryIds', id.toString())
        )
    }

    return apiClient(
        `/Reports/topproductsbycategory?${params.toString()}`,
        {},
        ORDER_SERVICE_URL
    )
}
