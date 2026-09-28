export interface TotalRevenueResponse {
    totalRevenue: number
    totalOrders: number
}

export interface TopProduct {
    productId: number
    productName: string | null
    price: number
    soldQuantity: number
}

export interface TopProductsByCategory {
    categoryId: number
    products: TopProduct[]
}

export interface TopProductsByCategoryResponse {
    items: TopProductsByCategory[]
    totalCount: number
    pageNumber: number | null
    pageSize: number | null
}
