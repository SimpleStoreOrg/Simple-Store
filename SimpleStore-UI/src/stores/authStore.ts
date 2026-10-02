import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { jwtDecode } from 'jwt-decode'

interface JwtPayload {
    sub?: string
    nameid?: string

    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string

    role?: string
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string

    AdminPosition?: string
    MarketId?: string
}

interface AuthState {
    accessToken: string | null
    refreshToken: string | null

    userId: number | null
    role: string | null
    adminPosition: string | null
    marketId: number | null

    setTokens: (
        accessToken: string,
        refreshToken: string
    ) => void

    logout: () => void
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            accessToken: null,
            refreshToken: null,

            userId: null,
            role: null,
            adminPosition: null,
            marketId: null,

            setTokens: (
                accessToken,
                refreshToken
            ) => {
                const decoded =
                    jwtDecode<JwtPayload>(
                        accessToken
                    )

                const userIdClaim =
                    decoded[
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                        ] ??
                    decoded.nameid ??
                    decoded.sub

                const role =
                    decoded.role ??
                    decoded[
                        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                        ]

                const adminPosition =
                    decoded.AdminPosition ?? null

                const marketIdRaw =
                    decoded.MarketId ?? null

                const userId = userIdClaim
                    ? Number(userIdClaim)
                    : null

                const marketId = marketIdRaw
                    ? Number(marketIdRaw)
                    : null

                set({
                    accessToken,
                    refreshToken,
                    userId,
                    role: role ?? null,
                    adminPosition,
                    marketId,
                })
            },

            logout: () => {
                set({
                    accessToken: null,
                    refreshToken: null,
                    userId: null,
                    role: null,
                    adminPosition: null,
                    marketId: null,
                })
            },
        }),
        {
            name: 'simplestore-auth',
        }
    )
)