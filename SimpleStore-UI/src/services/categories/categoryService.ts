import { apiClient } from '../api/apiClient'

import type {
    CategoriesResponse,
    Category,
} from '../../types/category'

const PRODUCT_SERVICE_URL =
    import.meta.env.VITE_PRODUCT_SERVICE_URL ??
    'https://localhost:7002/api'

export async function getAllCategories(): Promise<CategoriesResponse> {
    return apiClient(
        '/Category?pageNumber=1&pageSize=100',
        {},
        PRODUCT_SERVICE_URL
    )
}

export async function getCategoryById(
    id: number
): Promise<Category> {
    return apiClient(
        `/Category/${id}`,
        {},
        PRODUCT_SERVICE_URL
    )
}

export interface CreateCategoryRequest {
    name: string
    parentCategoryId: number | null
}

export async function createCategory(
    request: CreateCategoryRequest
): Promise<Category> {
    return apiClient(
        '/Category',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        PRODUCT_SERVICE_URL
    )
}

export interface UpdateCategoryRequest {
    name: string
    parentCategoryId: number | null
}

export async function updateCategory(
    id: number,
    request: UpdateCategoryRequest
): Promise<Category> {
    return apiClient(
        `/Category/${id}`,
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        PRODUCT_SERVICE_URL
    )
}

export async function deleteCategory(
    id: number
): Promise<null> {
    return apiClient(
        `/Category/${id}`,
        {
            method: 'DELETE',
        },
        PRODUCT_SERVICE_URL
    )
}