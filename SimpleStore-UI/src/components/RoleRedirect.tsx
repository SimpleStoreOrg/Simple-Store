import { Navigate } from 'react-router-dom'
import { useAuthStore } from '../stores/authStore'

function RoleRedirect() {
    const role = useAuthStore(
        (state) => state.role
    )

    if (!role) {
        return <Navigate to="/login" replace />
    }

    switch (role) {
        case 'Admin':
            return <Navigate to="/admin" replace />

        case 'ShopperAssistant':
            return (
                <Navigate
                    to="/shopper-assistant"
                    replace
                />
            )

        case 'Customer':
            return <Navigate to="/customer" replace />

        default:
            return <Navigate to="/login" replace />
    }
}

export default RoleRedirect