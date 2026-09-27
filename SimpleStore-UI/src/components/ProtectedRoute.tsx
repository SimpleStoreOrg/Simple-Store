import {
    Navigate,
    Outlet,
    useLocation,
} from 'react-router-dom'
import { useAuthStore } from '../stores/authStore'

interface ProtectedRouteProps {
    allowedRoles?: string[]
    allowedAdminPositions?: string[]
}

function ProtectedRoute({
                            allowedRoles,
                            allowedAdminPositions,
                        }: ProtectedRouteProps) {
    const accessToken = useAuthStore(
        (state) => state.accessToken
    )

    const role = useAuthStore(
        (state) => state.role
    )

    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const location = useLocation()

    if (!accessToken) {
        return <Navigate to="/login" replace />
    }

    if (
        allowedRoles &&
        allowedRoles.length > 0 &&
        (!role || !allowedRoles.includes(role))
    ) {
        return (
            <Navigate
                to="/"
                replace
                state={{ from: location }}
            />
        )
    }

    if (
        allowedAdminPositions &&
        allowedAdminPositions.length > 0
    ) {
        if (
            !adminPosition ||
            !allowedAdminPositions.includes(
                adminPosition
            )
        ) {
            return (
                <Navigate
                    to="/admin"
                    replace
                    state={{ from: location }}
                />
            )
        }
    }

    return <Outlet />
}

export default ProtectedRoute