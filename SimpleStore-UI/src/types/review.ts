export interface Review {
    id: number
    orderId: number
    productId: number
    marketId: number
    customerId: number
    rating: number
    message: string | null
    createdAt: string
}

export interface ReviewsByProductSummary {
    productId: number
    averageRating: number
    reviewCount: number
    reviews: Review[]
}

export interface ReviewProductRequest {
    orderId: number
    productId: number
    rating: number
    message?: string
}

export interface ReviewsResponse {
    items: Review[]
    totalCount: number
    pageNumber: number | null
    pageSize: number | null
}
