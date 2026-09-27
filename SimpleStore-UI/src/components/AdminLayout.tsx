import {
    Layout,
    Menu,
    Avatar,
    Dropdown,
} from 'antd'
import {
    DashboardOutlined,
    UserOutlined,
    TeamOutlined,
    ShoppingOutlined,
    ShoppingCartOutlined,
    ShopOutlined,
    SafetyOutlined,
    AppstoreOutlined,
    LogoutOutlined,
    DownOutlined,
} from '@ant-design/icons'
import {
    Outlet,
    useNavigate,
    useLocation,
} from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../stores/authStore'
import { getCurrentAdmin } from '../services/admins/adminService'

const { Header, Sider, Content } = Layout

function AdminLayout() {
    const navigate = useNavigate()
    const location = useLocation()

    const logout = useAuthStore(
        (state) => state.logout
    )

    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const isSuperAdmin =
        adminPosition === 'SuperAdmin'

    const isMarketAdmin =
        adminPosition === 'MarketAdmin'

    const { data: currentAdmin } = useQuery({
        queryKey: ['current-admin'],
        queryFn: getCurrentAdmin,
    })

    const fullName = currentAdmin
        ? `${currentAdmin.name ?? ''} ${
            currentAdmin.surname ?? ''
        }`.trim()
        : 'Admin'

    const initials = fullName
        .split(' ')
        .map((part) => part[0])
        .filter(Boolean)
        .slice(0, 2)
        .join('')
        .toUpperCase()

    const roleLabel = isSuperAdmin
        ? 'Super Admin'
        : isMarketAdmin
            ? 'Market Admin'
            : 'Admin'

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    const getSelectedKey = () => {
        if (location.pathname === '/admin') {
            return 'dashboard'
        }

        if (
            location.pathname.startsWith(
                '/admin/customers'
            )
        ) {
            return 'customers'
        }

        if (
            location.pathname.startsWith(
                '/admin/shopper-assistants'
            )
        ) {
            return 'shopper-assistants'
        }

        if (
            location.pathname.startsWith(
                '/admin/products'
            )
        ) {
            return 'products'
        }

        if (
            location.pathname.startsWith(
                '/admin/orders'
            )
        ) {
            return 'orders'
        }

        if (
            location.pathname.startsWith(
                '/admin/markets'
            )
        ) {
            return 'markets'
        }

        if (
            location.pathname.startsWith(
                '/admin/categories'
            )
        ) {
            return 'categories'
        }

        if (
            location.pathname.startsWith(
                '/admin/my-market'
            )
        ) {
            return 'my-market'
        }

        if (
            location.pathname.startsWith(
                '/admin/admins'
            )
        ) {
            return 'admins'
        }

        if (
            location.pathname.startsWith(
                '/admin/profile'
            )
        ) {
            return 'profile'
        }

        return 'dashboard'
    }

    const menuItems = [
        {
            key: 'dashboard',
            icon: <DashboardOutlined />,
            label: 'Dashboard',
            onClick: () => navigate('/admin'),
        },
        {
            key: 'customers',
            icon: <UserOutlined />,
            label: 'Customers',
            onClick: () =>
                navigate('/admin/customers'),
        },
        {
            key: 'shopper-assistants',
            icon: <TeamOutlined />,
            label: 'Shopper Assistants',
            onClick: () =>
                navigate(
                    '/admin/shopper-assistants'
                ),
        },
        {
            key: 'products',
            icon: <ShoppingOutlined />,
            label: 'Products',
            onClick: () =>
                navigate('/admin/products'),
        },
        {
            key: 'orders',
            icon: <ShoppingCartOutlined />,
            label: 'Orders',
            onClick: () =>
                navigate('/admin/orders'),
        },

        ...(isSuperAdmin
            ? [
                {
                    key: 'markets',
                    icon: <ShopOutlined />,
                    label: 'Markets',
                    onClick: () =>
                        navigate(
                            '/admin/markets'
                        ),
                },
                {
                    key: 'categories',
                    icon: (
                        <AppstoreOutlined />
                    ),
                    label: 'Categories',
                    onClick: () =>
                        navigate(
                            '/admin/categories'
                        ),
                },
                {
                    key: 'admins',
                    icon: <SafetyOutlined />,
                    label: 'Admins',
                    onClick: () =>
                        navigate(
                            '/admin/admins'
                        ),
                },
            ]
            : []),

        ...(isMarketAdmin
            ? [
                {
                    key: 'my-market',
                    icon: <ShopOutlined />,
                    label: 'My Market',
                    onClick: () =>
                        navigate(
                            '/admin/my-market'
                        ),
                },
            ]
            : []),

        {
            key: 'profile',
            icon: <UserOutlined />,
            label: 'Profile',
            onClick: () =>
                navigate('/admin/profile'),
        },
    ]

    return (
        <Layout className="min-h-screen">
            <Sider
                width={260}
                style={{
                    position: 'fixed',
                    height: '100vh',
                    left: 0,
                    top: 0,
                    overflow: 'auto',
                }}
            >
                <div className="flex flex-col h-full">
                    <div className="flex items-center gap-3 px-6 py-6">
                        <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-[#4f46e5] text-white font-bold text-sm shadow-lg shadow-indigo-500/30">
                            S
                        </div>

                        <div className="text-white text-base font-semibold tracking-tight">
                            SimpleStore
                        </div>
                    </div>

                    <div className="mx-6 mb-4 h-px bg-white/10" />

                    <div className="flex-1 overflow-auto">
                        <Menu
                            theme="dark"
                            mode="inline"
                            selectedKeys={[
                                getSelectedKey(),
                            ]}
                            items={menuItems}
                            style={{
                                background: 'transparent',
                                borderInlineEnd:
                                    'none',
                            }}
                        />
                    </div>

                    <div className="p-4 mt-4">
                        <Dropdown
                            trigger={['click']}
                            menu={{
                                items: [
                                    {
                                        key: 'logout',
                                        icon: (
                                            <LogoutOutlined />
                                        ),
                                        label: 'Sign out',
                                        danger: true,
                                        onClick:
                                        handleLogout,
                                    },
                                ],
                            }}
                        >
                            <button
                                type="button"
                                className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl bg-white/5 hover:bg-white/10 transition-colors text-left"
                            >
                                <Avatar
                                    size={36}
                                    style={{
                                        background:
                                            '#4f46e5',
                                        fontWeight: 600,
                                    }}
                                >
                                    {initials || 'A'}
                                </Avatar>

                                <div className="flex-1 min-w-0">
                                    <div className="text-white text-sm font-medium truncate">
                                        {fullName}
                                    </div>

                                    <div className="text-white/50 text-xs truncate">
                                        {roleLabel}
                                    </div>
                                </div>

                                <DownOutlined
                                    style={{
                                        fontSize: 10,
                                        color: 'rgba(255,255,255,0.4)',
                                    }}
                                />
                            </button>
                        </Dropdown>
                    </div>
                </div>
            </Sider>

            <Layout
                style={{
                    marginLeft: 260,
                }}
            >
                <Header
                    className="flex justify-end items-center"
                    style={{
                        background: '#ffffff',
                        borderBottom:
                            '1px solid #eaeaea',
                        padding: '0 32px',
                    }}
                >
                    <span className="text-sm text-[#6b7280]">
                        {roleLabel}
                    </span>
                </Header>

                <Content
                    style={{
                        background: '#fafafa',
                        padding: 32,
                        minHeight:
                            'calc(100vh - 64px)',
                    }}
                >
                    <Outlet />
                </Content>
            </Layout>
        </Layout>
    )
}

export default AdminLayout