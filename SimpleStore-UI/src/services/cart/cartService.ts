import { apiClient } from '../api/apiClient'

const ORDER_SERVICE_URL = 'https://localhost:7001/api'

export interface CartItemRequest {
    productId: number
    quantity: number
}

export interface CreateCartRequest {
    cartItems: CartItemRequest[]
}

export interface UpdateCartItemRequest {
    quantity: number
}

export interface CartItem {
    productId: number
    marketId: number
    quantity: number
    price: number
    totalItemPrice: number
}

export interface Cart {
    id: number
    customerId: number
    items: CartItem[]
    totalPrice: number
    createdAt: string | null
    updatedAt: string | null
    deletedAt: string | null
}

export async function getMyCart(): Promise<Cart> {
    return apiClient(
        '/Cart/my-cart',
        {},
        ORDER_SERVICE_URL
    )
}

export async function createCart(): Promise<Cart> {
    return apiClient(
        '/Cart',
        {
            method: 'POST',
        },
        ORDER_SERVICE_URL
    )
}

export async function addItemsToCart(
    request: CreateCartRequest
): Promise<Cart> {
    return apiClient(
        '/Cart/additemtocart',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        ORDER_SERVICE_URL
    )
}

export async function updateCartItem(
    productId: number,
    request: UpdateCartItemRequest
): Promise<Cart> {
    return apiClient(
        `/Cart/items/${productId}`,
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        ORDER_SERVICE_URL
    )
}

export async function removeCartItem(
    productId: number
): Promise<Cart> {
    return apiClient(
        `/Cart/items/${productId}`,
        {
            method: 'DELETE',
        },
        ORDER_SERVICE_URL
    )
}