export type ShopperAssistantPosition = 0 | 1 | 2

export interface ShopperAssistant {
    id: number
    marketId: number
    name: string
    surname: string
    email: string
    username: string
    role: number
    position: ShopperAssistantPosition
    phoneNumber: string
    createdAt: string
    updatedAt: string | null
    deletedAt: string | null
}

export interface ShopperAssistantsResponse {
    items: ShopperAssistant[]
    totalCount: number
    pageNumber: number | null
    pageSize: number | null
}

export interface CreateShopperAssistantRequest {
    name: string
    surname: string
    email: string
    username: string
    password: string
    position: ShopperAssistantPosition
    phoneNumber: string
}

export interface UpdateShopperAssistantRequest {
    name: string
    surname: string
    email: string
    username: string
    position: ShopperAssistantPosition
    phoneNumber: string
}

export const shopperAssistantPositionLabels: Record<
    ShopperAssistantPosition,
    string
> = {
    0: 'Cashier',
    1: 'Collector',
    2: 'Packer',
}