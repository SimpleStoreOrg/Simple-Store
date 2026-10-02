export interface Product {
    id: number
    name: string
    price: number
    stock: number
    categoryId: number
    marketId: number
    createdAt: string
    updatedAt: string
}

export interface ProductsResponse {
    items: Product[]
    totalCount: number
    pageNumber: number
    pageSize: number
}