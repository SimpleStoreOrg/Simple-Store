import type { OrderStatus } from './orderStatus'

export interface OrderItem {
    marketId: number
    productId: number
    quantity: number
    price: number
    totalItemPrice: number
}

export interface Order {
    id: number
    customerId: number
    shopperAssistantId: number
    status: OrderStatus
    createdAt: string | null
    updatedAt: string | null
    deletedAt: string | null
    items: OrderItem[]
    totalPrice: number
    pickUpDeadline: string | null
}

export interface OrdersResponse {
    items: Order[]
    totalCount: number
    pageNumber: number | null
    pageSize: number | null
}

export interface AssignOrderRequest {
    shopperAssistantId: number
}

export interface UpdateOrderStatusRequest {
    status: OrderStatus
}

export interface PayOrderRequest {
    amountPaid: number
}