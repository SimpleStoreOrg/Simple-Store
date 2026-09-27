import { apiClient } from './api/apiClient'
import type { OrdersResponse } from '../types/order'

const ORDER_SERVICE_URL = 'https://localhost:7001/api'

export async function getOrderHistory(
    pageNumber = 1,
    pageSize = 100
): Promise<OrdersResponse> {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
    })

    return apiClient(
        `/Order/orderhistory?${params.toString()}`,
        {},
        ORDER_SERVICE_URL
    )
}