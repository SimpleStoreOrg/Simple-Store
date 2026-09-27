export interface Customer {
    id: number
    name: string | null
    surname: string | null
    role: number
    username: string | null
    email: string | null
    phoneNumber: string | null
    createdAt: string
    updatedAt: string | null
    deletedAt: string | null
}