import { apiClient } from '../api/apiClient'

const PRODUCT_SERVICE_URL = 'https://localhost:7002/api'

export interface GetAllProductsParams {
    pageNumber?: number
    pageSize?: number
    isAvailable?: boolean
    minPrice?: number
    maxPrice?: number
    categoryIds?: number[]
    productIds?: number[]
    marketId?: number
}

export async function getAllProducts(
    params: GetAllProductsParams = {}
) {
    const query = new URLSearchParams()

    query.append(
        'pageNumber',
        (params.pageNumber ?? 1).toString()
    )

    query.append(
        'pageSize',
        (params.pageSize ?? 100).toString()
    )

    if (params.isAvailable !== undefined) {
        query.append(
            'isAvailable',
            params.isAvailable.toString()
        )
    }

    if (params.minPrice !== undefined) {
        query.append(
            'minPrice',
            params.minPrice.toString()
        )
    }

    if (params.maxPrice !== undefined) {
        query.append(
            'maxPrice',
            params.maxPrice.toString()
        )
    }

    if (params.marketId !== undefined) {
        query.append(
            'marketId',
            params.marketId.toString()
        )
    }

    if (params.categoryIds) {
        params.categoryIds.forEach((id) => {
            query.append('categoryIds', id.toString())
        })
    }

    if (params.productIds) {
        params.productIds.forEach((id) => {
            query.append('productIds', id.toString())
        })
    }

    return apiClient(
        `/Product?${query.toString()}`,
        {},
        PRODUCT_SERVICE_URL
    )
}

export async function createProduct(request: {
    name: string
    price: number
    stock: number
    categoryId: number
}) {
    return apiClient(
        '/Product',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        PRODUCT_SERVICE_URL
    )
}

export async function updateProduct(
    id: number,
    request: {
        name: string
        price: number
        stock: number
        categoryId: number
    }
) {
    return apiClient(
        `/Product/${id}`,
        {
            method: 'PUT',
            body: JSON.stringify(request),
        },
        PRODUCT_SERVICE_URL
    )
}

export async function deleteProduct(id: number) {
    return apiClient(
        `/Product/${id}`,
        {
            method: 'DELETE',
        },
        PRODUCT_SERVICE_URL
    )
}