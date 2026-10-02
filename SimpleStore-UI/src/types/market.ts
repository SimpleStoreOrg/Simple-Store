export interface Market {
    id: number
    marketAdminId: number
    name: string
    location: string
    email: string
    phoneNumber: string
    createdAt: string
    updatedAt: string | null
    deletedAt: string | null
}

export interface MarketsResponse {
    items: Market[]
    totalCount: number
    pageNumber: number
    pageSize: number
}

export interface CreateMarketRequest {
    marketAdminId: number
    name: string
    location: string
    email: string
    phoneNumber: string
}

export interface UpdateMarketRequest {
    name: string
    location: string
    email: string
    phoneNumber: string
}