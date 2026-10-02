export interface Category {
    id: number
    name: string
    parentCategoryId: number | null
}

export interface CategoriesResponse {
    items: Category[]
    totalCount: number
    pageNumber: number
    pageSize: number
}