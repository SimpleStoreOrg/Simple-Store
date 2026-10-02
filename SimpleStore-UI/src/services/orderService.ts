import { apiClient } from './api/apiClient'

import type {
    AssignOrderRequest,
    Order,
    OrdersResponse,
    PayOrderRequest,
    UpdateOrderStatusRequest,
} from '../types/order'

const ORDER_SERVICE_URL =
    import.meta.env.VITE_ORDER_SERVICE_URL ??
    'https://localhost:7001/api'

export interface PaymentResponse {
    total: number
    paid: number
    change: number
}

export async function getAllOrders(
    pageNumber = 1,
    pageSize = 100,
    statuses?: number,
    shopperAssistantId?: number
): Promise<OrdersResponse> {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
    })

    if (statuses !== undefined) {
        params.append(
            'statuses',
            statuses.toString()
        )
    }

    if (shopperAssistantId !== undefined) {
        params.append(
            'shopperAssistantIds',
            shopperAssistantId.toString()
        )
    }

    return apiClient(
        `/Order?${params.toString()}`,
        {},
        ORDER_SERVICE_URL
    )
}

export async function getOrderById(
    id: number
): Promise<Order> {
    return apiClient(
        `/Order/${id}`,
        {},
        ORDER_SERVICE_URL
    )
}

export async function createOrder(): Promise<Order> {
    return apiClient(
        '/Order',
        {
            method: 'POST',
        },
        ORDER_SERVICE_URL
    )
}

export async function cancelOrderByCustomer(
    id: number
): Promise<null> {
    return apiClient(
        `/Order/${id}/cancelbycustomer`,
        {
            method: 'POST',
        },
        ORDER_SERVICE_URL
    )
}

export async function assignOrder(
    id: number,
    request: AssignOrderRequest
): Promise<void> {
    await apiClient(
        `/Order/${id}/assign-assistant`,
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        ORDER_SERVICE_URL
    )
}

export async function updateOrderStatus(
    id: number,
    request: UpdateOrderStatusRequest
): Promise<void> {
    await apiClient(
        `/Order/${id}/update-orderstatus`,
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        ORDER_SERVICE_URL
    )
}

export async function payOrder(
    id: number,
    request: PayOrderRequest
): Promise<PaymentResponse> {
    return apiClient(
        `/Order/${id}/pay`,
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        ORDER_SERVICE_URL
    )
}

export async function cancelOrderByMarket(
    id: number
): Promise<null> {
    return apiClient(
        `/Order/${id}/cancelbymarket`,
        {
            method: 'POST',
        },
        ORDER_SERVICE_URL
    )
}