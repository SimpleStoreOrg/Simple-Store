import { useAuthStore } from '../../stores/authStore'

const USER_SERVICE_URL =
    import.meta.env.VITE_USER_SERVICE_URL ??
    'https://localhost:7003/api'

export class ApiError extends Error {
    status: number

    constructor(
        message: string,
        status: number
    ) {
        super(message)
        this.name = 'ApiError'
        this.status = status
    }
}

export async function apiClient(
    endpoint: string,
    options: RequestInit = {},
    baseUrl: string = USER_SERVICE_URL
) {
    const accessToken =
        useAuthStore.getState().accessToken

    const response = await fetch(
        `${baseUrl}${endpoint}`,
        {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
                Authorization: `Bearer ${accessToken}`,
            },
        }
    )

    if (!response.ok) {
        let errorMessage =
            `Request failed: ${response.status}`

        try {
            const errorBody =
                await response.json()

            if (errorBody.detail) {
                errorMessage =
                    errorBody.detail
            } else if (errorBody.title) {
                errorMessage =
                    errorBody.title
            }
        } catch {
            // Response did not contain JSON.
        }

        throw new ApiError(
            errorMessage,
            response.status
        )
    }

    if (response.status === 204) {
        return null
    }

    const responseText =
        await response.text()

    if (!responseText) {
        return null
    }

    try {
        return JSON.parse(responseText)
    } catch {
        // Backend returned a non-JSON body (e.g. a plain string).
        // Return it as-is rather than crashing.
        return responseText
    }
}