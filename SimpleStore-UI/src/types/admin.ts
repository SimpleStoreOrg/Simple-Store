export type AdminPosition = 0 | 1

export interface Admin {
    id: number
    marketId: number | null
    name: string
    surname: string
    email: string
    username: string
    role: number
    position: AdminPosition
    phoneNumber: string
    createdAt: string
    updatedAt: string | null
    deletedAt: string | null
}

export interface AdminsResponse {
    items: Admin[]
    totalCount: number
    pageNumber: number
    pageSize: number
}

export interface CreateAdminRequest {
    name: string
    surname: string
    email: string
    username: string
    password: string
    phoneNumber: string
}

export interface UpdateAdminRequest {
    name: string
    surname: string
    email: string
    username: string
    phoneNumber: string
}