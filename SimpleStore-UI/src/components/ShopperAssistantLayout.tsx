import {
    Layout,
    Menu,
    Avatar,
    Dropdown,
} from 'antd'
import {
    DashboardOutlined,
    ShoppingCartOutlined,
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
import { getCurrentShopperAssistant } from '../services/shopperAssistants/shopperAssistantService'

const { Header, Sider, Content } = Layout

function ShopperAssistantLayout() {
    const navigate = useNavigate()
    const location = useLocation()

    const logout = useAuthStore(
        (state) => state.logout
    )

    const {
        data: currentAssistant,
    } = useQuery({
        queryKey: ['current-shopper-assistant'],
        queryFn: getCurrentShopperAssistant,
    })

    const fullName =
        currentAssistant
            ? `${currentAssistant.name ?? ''} ${
                currentAssistant.surname ?? ''
            }`.trim()
            : 'Shopper Assistant'

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
        if (
            location.pathname ===
            '/shopper-assistant'
        ) {
            return 'dashboard'
        }

        if (
            location.pathname.startsWith(
                '/shopper-assistant/orders'
            )
        ) {
            return 'orders'
        }

        if (
            location.pathname.startsWith(
                '/shopper-assistant/profile'
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
                                            '/shopper-assistant'
                                        ),
                                },
                                {
                                    key: 'orders',
                                    icon: (
                                        <ShoppingCartOutlined />
                                    ),
                                    label: 'Orders',
                                    onClick: () =>
                                        navigate(
                                            '/shopper-assistant/orders'
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
                                            '/shopper-assistant/profile'
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
                                    {initials || 'S'}
                                </Avatar>

                                <div className="flex-1 min-w-0">
                                    <div className="text-white text-sm font-medium truncate">
                                        {fullName}
                                    </div>

                                    <div className="text-white/50 text-xs truncate">
                                        Shopper Assistant
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
                        Shopper Assistant
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

export default ShopperAssistantLayout