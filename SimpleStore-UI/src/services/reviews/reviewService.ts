import { apiClient } from '../api/apiClient'

import type {
    ReviewProductRequest,
    ReviewsByProductSummary,
    ReviewsResponse,
} from '../../types/review'

const ORDER_SERVICE_URL =
    import.meta.env.VITE_ORDER_SERVICE_URL ??
    'https://localhost:7001/api'

export async function submitReview(
    request: ReviewProductRequest
): Promise<void> {
    await apiClient(
        '/Order/reviewproduct',
        {
            method: 'POST',
            body: JSON.stringify(request),
        },
        ORDER_SERVICE_URL
    )
}

export async function getReviewsByProduct(
    productId: number
): Promise<ReviewsByProductSummary> {
    return apiClient(
        `/Order/reviewsbyproduct/${productId}`,
        {},
        ORDER_SERVICE_URL
    )
}

export async function getAllReviews(
    pageNumber = 1,
    pageSize = 100,
    reviewsFrom?: string,
    reviewsTo?: string
): Promise<ReviewsResponse> {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
    })

    if (reviewsFrom) {
        params.append('reviewsFrom', reviewsFrom)
    }

    if (reviewsTo) {
        params.append('reviewsTo', reviewsTo)
    }

    return apiClient(
        `/Reports/reviewedproducts?${params.toString()}`,
        {},
        ORDER_SERVICE_URL
    )
}
