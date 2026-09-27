import {
    Layout,
    Menu,
    Avatar,
    Dropdown,
} from 'antd'
import {
    DashboardOutlined,
    ShoppingOutlined,
    ShoppingCartOutlined,
    FileTextOutlined,
    UserOutlined,
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
import { getCurrentCustomer } from '../services/customers/customerService'

const { Header, Sider, Content } = Layout

function CustomerLayout() {
    const navigate = useNavigate()
    const location = useLocation()

    const logout = useAuthStore(
        (state) => state.logout
    )

    const {
        data: currentCustomer,
    } = useQuery({
        queryKey: ['current-customer'],
        queryFn: getCurrentCustomer,
    })

    const fullName =
        currentCustomer
            ? `${currentCustomer.name ?? ''} ${
                currentCustomer.surname ?? ''
            }`.trim()
            : 'Customer'

    const initials = fullName
        .split(' ')
        .map((part) => part[0])
        .filter(Boolean)
        .slice(0, 2)
        .join('')
        .toUpperCase()

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    const getSelectedKey = () => {
        if (location.pathname === '/customer') {
            return 'dashboard'
        }

        if (
            location.pathname.startsWith(
                '/customer/products'
            )
        ) {
            return 'products'
        }

        if (
            location.pathname.startsWith(
                '/customer/cart'
            )
        ) {
            return 'cart'
        }

        if (
            location.pathname.startsWith(
                '/customer/order-history'
            )
        ) {
            return 'orders'
        }

        if (
            location.pathname.startsWith(
                '/customer/profile'
            )
        ) {
            return 'profile'
        }

        return 'dashboard'
    }

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
                    {/* Brand */}
                    <div className="flex items-center gap-3 px-6 py-6">
                        <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-[#4f46e5] text-white font-bold text-sm shadow-lg shadow-indigo-500/30">
                            S
                        </div>

                        <div className="text-white text-base font-semibold tracking-tight">
                            SimpleStore
                        </div>
                    </div>

                    {/* Divider */}
                    <div className="mx-6 mb-4 h-px bg-white/10" />

                    {/* Menu */}
                    <div className="flex-1 overflow-auto">
                        <Menu
                            theme="dark"
                            mode="inline"
                            selectedKeys={[
                                getSelectedKey(),
                            ]}
                            items={[
                                {
                                    key: 'dashboard',
                                    icon: (
                                        <DashboardOutlined />
                                    ),
                                    label: 'Dashboard',
                                    onClick: () =>
                                        navigate(
                                            '/customer'
                                        ),
                                },
                                {
                                    key: 'products',
                                    icon: (
                                        <ShoppingOutlined />
                                    ),
                                    label: 'Products',
                                    onClick: () =>
                                        navigate(
                                            '/customer/products'
                                        ),
                                },
                                {
                                    key: 'cart',
                                    icon: (
                                        <ShoppingCartOutlined />
                                    ),
                                    label: 'Cart',
                                    onClick: () =>
                                        navigate(
                                            '/customer/cart'
                                        ),
                                },
                                {
                                    key: 'orders',
                                    icon: (
                                        <FileTextOutlined />
                                    ),
                                    label: 'My Orders',
                                    onClick: () =>
                                        navigate(
                                            '/customer/order-history'
                                        ),
                                },
                                {
                                    key: 'profile',
                                    icon: (
                                        <UserOutlined />
                                    ),
                                    label: 'Profile',
                                    onClick: () =>
                                        navigate(
                                            '/customer/profile'
                                        ),
                                },
                            ]}
                            style={{
                                background: 'transparent',
                                borderInlineEnd:
                                    'none',
                            }}
                        />
                    </div>

                    {/* User block */}
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
                                    {initials || 'C'}
                                </Avatar>

                                <div className="flex-1 min-w-0">
                                    <div className="text-white text-sm font-medium truncate">
                                        {fullName}
                                    </div>

                                    <div className="text-white/50 text-xs truncate">
                                        Customer
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
                        Customer
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

export default CustomerLayout