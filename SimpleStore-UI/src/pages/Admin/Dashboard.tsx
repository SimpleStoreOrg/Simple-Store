import {
    Col,
    Row,
    Spin,
    Statistic,
    Typography,
} from 'antd'
import {
    UserOutlined,
    TeamOutlined,
    ShoppingOutlined,
    ShoppingCartOutlined,
    ShopOutlined,
    EnvironmentOutlined,
    MailOutlined,
    PhoneOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'

import { useAuthStore } from '../../stores/authStore'

import { getCurrentAdmin } from '../../services/admins/adminService'
import { getAllCustomers } from '../../services/customers/customerService'
import { getAllShopperAssistants } from '../../services/shopperAssistants/shopperAssistantService'
import { getAllProducts } from '../../services/products/productService'
import { getAllOrders } from '../../services/orderService'
import {
    getAllMarkets,
    getMarketById,
} from '../../services/markets/marketService'

const { Text } = Typography

function getCount(data: unknown): number {
    if (!data || typeof data !== 'object') {
        return 0
    }

    const obj = data as {
        totalCount?: number
        items?: unknown[]
    }

    if (typeof obj.totalCount === 'number') {
        return obj.totalCount
    }

    if (Array.isArray(obj.items)) {
        return obj.items.length
    }

    return 0
}

function Dashboard() {
    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const marketId = useAuthStore(
        (state) => state.marketId
    )

    const isSuperAdmin =
        adminPosition === 'SuperAdmin'

    const isMarketAdmin =
        adminPosition === 'MarketAdmin'

    const { data: currentAdmin } = useQuery({
        queryKey: ['current-admin'],
        queryFn: getCurrentAdmin,
    })

    const {
        data: customersData,
        isLoading: loadingCustomers,
    } = useQuery({
        queryKey: ['dashboard-customers'],
        queryFn: getAllCustomers,
        enabled: isSuperAdmin,
    })

    const {
        data: shopperAssistantsData,
        isLoading: loadingShopperAssistants,
    } = useQuery({
        queryKey: ['dashboard-shopper-assistants'],
        queryFn: () => getAllShopperAssistants(1, 100),
    })

    const {
        data: productsData,
        isLoading: loadingProducts,
    } = useQuery({
        queryKey: ['dashboard-products'],
        queryFn: () => getAllProducts(),
    })

    const {
        data: marketsData,
        isLoading: loadingMarkets,
    } = useQuery({
        queryKey: ['dashboard-markets'],
        queryFn: getAllMarkets,
        enabled: isSuperAdmin,
    })

    const {
        data: ordersData,
        isLoading: loadingOrders,
    } = useQuery({
        queryKey: ['dashboard-orders'],
        queryFn: () => getAllOrders(1, 100),
    })

    const {
        data: myMarket,
        isLoading: loadingMyMarket,
    } = useQuery({
        queryKey: ['dashboard-my-market', marketId],
        queryFn: () => getMarketById(marketId!),
        enabled: isMarketAdmin && marketId !== null,
    })

    const fullName = currentAdmin
        ? `${currentAdmin.name ?? ''} ${
            currentAdmin.surname ?? ''
        }`.trim()
        : ''

    if (isSuperAdmin) {
        const isLoadingAny =
            loadingCustomers ||
            loadingShopperAssistants ||
            loadingProducts ||
            loadingMarkets ||
            loadingOrders

        return (
            <div>
                <div className="mb-6">
                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Welcome back
                        {fullName
                            ? `, ${fullName}`
                            : ''}
                    </h1>

                    <Text type="secondary">
                        Super Administrator
                    </Text>
                </div>

                {isLoadingAny ? (
                    <div className="flex justify-center items-center py-20">
                        <Spin size="large" />
                    </div>
                ) : (
                    <Row gutter={[16, 16]}>
                        <Col xs={24} sm={12} lg={6}>
                            <div className="surface-card p-5">
                                <Statistic
                                    title="Customers"
                                    value={getCount(
                                        customersData
                                    )}
                                    prefix={
                                        <UserOutlined />
                                    }
                                />
                            </div>
                        </Col>

                        <Col xs={24} sm={12} lg={6}>
                            <div className="surface-card p-5">
                                <Statistic
                                    title="Shopper Assistants"
                                    value={getCount(
                                        shopperAssistantsData
                                    )}
                                    prefix={
                                        <TeamOutlined />
                                    }
                                />
                            </div>
                        </Col>

                        <Col xs={24} sm={12} lg={6}>
                            <div className="surface-card p-5">
                                <Statistic
                                    title="Products"
                                    value={getCount(
                                        productsData
                                    )}
                                    prefix={
                                        <ShoppingOutlined />
                                    }
                                />
                            </div>
                        </Col>

                        <Col xs={24} sm={12} lg={6}>
                            <div className="surface-card p-5">
                                <Statistic
                                    title="Markets"
                                    value={getCount(
                                        marketsData
                                    )}
                                    prefix={
                                        <ShopOutlined />
                                    }
                                />
                            </div>
                        </Col>

                        <Col xs={24} sm={12} lg={6}>
                            <div className="surface-card p-5">
                                <Statistic
                                    title="Orders"
                                    value={getCount(
                                        ordersData
                                    )}
                                    prefix={
                                        <ShoppingCartOutlined />
                                    }
                                />
                            </div>
                        </Col>
                    </Row>
                )}
            </div>
        )
    }

    if (isMarketAdmin) {
        const isLoadingAny =
            loadingShopperAssistants ||
            loadingProducts ||
            loadingOrders ||
            loadingMyMarket

        return (
            <div>
                <div className="mb-6">
                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Welcome back
                        {fullName
                            ? `, ${fullName}`
                            : ''}
                    </h1>

                    <Text type="secondary">
                        Market Administrator
                    </Text>
                </div>

                {isLoadingAny ? (
                    <div className="flex justify-center items-center py-20">
                        <Spin size="large" />
                    </div>
                ) : (
                    <>
                        <div className="surface-card p-6 mb-6">
                            <div className="flex items-start gap-4">
                                <div
                                    className="flex items-center justify-center rounded-xl bg-[#eef2ff]"
                                    style={{
                                        width: 56,
                                        height: 56,
                                        flexShrink: 0,
                                    }}
                                >
                                    <ShopOutlined
                                        style={{
                                            fontSize: 28,
                                            color: '#4f46e5',
                                        }}
                                    />
                                </div>

                                <div className="flex-1">
                                    <Text
                                        type="secondary"
                                        className="text-xs uppercase tracking-wide"
                                    >
                                        Your Market
                                    </Text>

                                    <h2 className="text-2xl font-bold mt-1 mb-3 text-[#0a0a0a]">
                                        {myMarket?.name ??
                                            'Not available'}
                                    </h2>

                                    <div className="flex flex-col gap-1">
                                        {myMarket?.location && (
                                            <div className="flex items-center gap-2 text-[#6b7280] text-sm">
                                                <EnvironmentOutlined />
                                                <span>
                                                    {
                                                        myMarket.location
                                                    }
                                                </span>
                                            </div>
                                        )}

                                        {myMarket?.email && (
                                            <div className="flex items-center gap-2 text-[#6b7280] text-sm">
                                                <MailOutlined />
                                                <span>
                                                    {
                                                        myMarket.email
                                                    }
                                                </span>
                                            </div>
                                        )}

                                        {myMarket?.phoneNumber && (
                                            <div className="flex items-center gap-2 text-[#6b7280] text-sm">
                                                <PhoneOutlined />
                                                <span>
                                                    {
                                                        myMarket.phoneNumber
                                                    }
                                                </span>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>

                        <Row gutter={[16, 16]}>
                            <Col xs={24} sm={12} lg={8}>
                                <div className="surface-card p-5">
                                    <Statistic
                                        title="Shopper Assistants"
                                        value={getCount(
                                            shopperAssistantsData
                                        )}
                                        prefix={
                                            <TeamOutlined />
                                        }
                                    />
                                </div>
                            </Col>

                            <Col xs={24} sm={12} lg={8}>
                                <div className="surface-card p-5">
                                    <Statistic
                                        title="Products"
                                        value={getCount(
                                            productsData
                                        )}
                                        prefix={
                                            <ShoppingOutlined />
                                        }
                                    />
                                </div>
                            </Col>

                            <Col xs={24} sm={12} lg={8}>
                                <div className="surface-card p-5">
                                    <Statistic
                                        title="Orders"
                                        value={getCount(
                                            ordersData
                                        )}
                                        prefix={
                                            <ShoppingCartOutlined />
                                        }
                                    />
                                </div>
                            </Col>
                        </Row>
                    </>
                )}
            </div>
        )
    }

    return (
        <div>
            <h1 className="text-3xl font-semibold tracking-tight mb-6">
                Dashboard
            </h1>

            <div className="surface-card p-6">
                <p>Access denied.</p>
            </div>
        </div>
    )
}

export default Dashboard