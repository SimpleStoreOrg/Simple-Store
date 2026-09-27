import {
    Button,
    Popconfirm,
    Spin,
    message,
} from 'antd'
import {
    ShoppingOutlined,
    ShopOutlined,
    ClockCircleOutlined,
    HistoryOutlined,
    RightOutlined,
    WarningOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import { getOrderHistory } from '../../services/orderHistoryService'
import { getAllProducts } from '../../services/products/productService'
import { getAllMarkets } from '../../services/markets/marketService'
import { cancelOrderByCustomer } from '../../services/orderService'
import { getCurrentCustomer } from '../../services/customers/customerService'

import {
    isProfileComplete,
    getMissingProfileFields,
} from '../../utils/profile'

import type { Order } from '../../types/order'
import type { Product } from '../../types/product'
import type { Market } from '../../types/market'

import { OrderStatus } from '../../types/orderStatus'

const STATUS_STEPS = [
    'New',
    'Accepted',
    'Collecting',
    'Ready To Go',
] as const

function CustomerDashboard() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()

    const {
        data: ordersData,
        isLoading: ordersLoading,
        isError: ordersError,
        error: ordersErrorMessage,
    } = useQuery({
        queryKey: ['customer-order-history'],
        queryFn: () => getOrderHistory(),
        refetchInterval: 5000,
    })

    const {
        data: productsData,
        isLoading: productsLoading,
        isError: productsError,
    } = useQuery({
        queryKey: ['products'],
        queryFn: () => getAllProducts(),
    })

    const {
        data: marketsData,
        isLoading: marketsLoading,
        isError: marketsError,
    } = useQuery({
        queryKey: ['customer-markets'],
        queryFn: getAllMarkets,
    })

    const { data: currentCustomer } = useQuery({
        queryKey: ['current-customer'],
        queryFn: getCurrentCustomer,
    })

    const cancelOrderMutation = useMutation({
        mutationFn: (orderId: number) =>
            cancelOrderByCustomer(orderId),

        onSuccess: () => {
            message.success(
                'Order cancelled successfully.'
            )

            queryClient.invalidateQueries({
                queryKey: ['customer-order-history'],
            })

            queryClient.invalidateQueries({
                queryKey: ['customer-products'],
            })

            queryClient.invalidateQueries({
                queryKey: ['products'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to cancel order.'
            )
        },
    })

    if (
        ordersLoading ||
        productsLoading ||
        marketsLoading
    ) {
        return (
            <div className="flex justify-center items-center py-20">
                <Spin size="large" />
            </div>
        )
    }

    if (ordersError) {
        message.error(
            ordersErrorMessage instanceof Error
                ? ordersErrorMessage.message
                : 'Failed to load your orders.'
        )

        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Dashboard
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Failed to load your current
                        orders.
                    </p>
                </div>
            </div>
        )
    }

    if (productsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Dashboard
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Failed to load product
                        information.
                    </p>
                </div>
            </div>
        )
    }

    if (marketsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Dashboard
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Failed to load market
                        information.
                    </p>
                </div>
            </div>
        )
    }

    const orders: Order[] = ordersData?.items ?? []

    const products: Product[] =
        productsData?.items ?? []

    const markets: Market[] =
        marketsData?.items ?? []

    const productMap = new Map(
        products.map((product) => [
            product.id,
            product,
        ])
    )

    const marketMap = new Map(
        markets.map((market) => [
            market.id,
            market,
        ])
    )

    const activeStatuses: OrderStatus[] = [
        OrderStatus.New,
        OrderStatus.Accepted,
        OrderStatus.Collecting,
        OrderStatus.ReadyToGo,
    ]

    const activeOrders = orders
        .filter((order) =>
            activeStatuses.includes(order.status)
        )
        .sort((a, b) => b.id - a.id)

    const getStatusLabel = (
        status: OrderStatus
    ) => {
        switch (status) {
            case OrderStatus.New:
                return 'New'

            case OrderStatus.Accepted:
                return 'Accepted'

            case OrderStatus.Collecting:
                return 'Collecting'

            case OrderStatus.ReadyToGo:
                return 'Ready To Go'

            default:
                return 'Unknown'
        }
    }

    const getStatusBadge = (
        status: OrderStatus
    ) => {
        switch (status) {
            case OrderStatus.New:
                return {
                    bg: '#fffbeb',
                    text: '#d97706',
                    dot: '#d97706',
                }

            case OrderStatus.Accepted:
                return {
                    bg: '#eef2ff',
                    text: '#4f46e5',
                    dot: '#4f46e5',
                }

            case OrderStatus.Collecting:
                return {
                    bg: '#eef2ff',
                    text: '#4f46e5',
                    dot: '#4f46e5',
                }

            case OrderStatus.ReadyToGo:
                return {
                    bg: '#ecfdf5',
                    text: '#059669',
                    dot: '#059669',
                }

            default:
                return {
                    bg: '#f4f4f5',
                    text: '#6b7280',
                    dot: '#9ca3af',
                }
        }
    }

    const getStepIndex = (
        status: OrderStatus
    ) => {
        switch (status) {
            case OrderStatus.New:
                return 0

            case OrderStatus.Accepted:
                return 1

            case OrderStatus.Collecting:
                return 2

            case OrderStatus.ReadyToGo:
                return 3

            default:
                return 0
        }
    }

    const getMarketName = (order: Order) => {
        const marketId =
            order.items?.[0]?.marketId

        if (!marketId) {
            return 'Market information unavailable'
        }

        return (
            marketMap.get(marketId)?.name ??
            `Market #${marketId}`
        )
    }

    const handleCancelOrder = (
        orderId: number
    ) => {
        cancelOrderMutation.mutate(orderId)
    }

    const firstName =
        currentCustomer?.name?.split(' ')[0] ??
        'there'

    const profileComplete =
        isProfileComplete(currentCustomer)

    const missingFields =
        getMissingProfileFields(currentCustomer)

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Welcome back, {firstName}
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Track your orders and manage
                    your shopping.
                </p>
            </div>

            {/* Profile incomplete banner */}
            {!profileComplete && (
                <div className="surface-card p-4 mb-6 border-l-4 border-l-[#d97706]">
                    <div className="flex items-start gap-3">
                        <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-[#fffbeb] text-[#d97706] flex-shrink-0">
                            <WarningOutlined
                                style={{ fontSize: 16 }}
                            />
                        </div>

                        <div className="flex-1 min-w-0">
                            <div className="text-sm font-semibold text-[#0a0a0a]">
                                Complete your profile
                            </div>

                            <div className="text-xs text-[#6b7280] mt-1">
                                Add your name and phone
                                number to start placing
                                orders. Missing:{' '}
                                {missingFields.join(', ')}
                            </div>
                        </div>

                        <Button
                            type="primary"
                            size="small"
                            onClick={() =>
                                navigate(
                                    '/customer/profile'
                                )
                            }
                        >
                            Go to Profile
                        </Button>
                    </div>
                </div>
            )}

            {activeOrders.length === 0 ? (
                <div className="surface-card p-12 text-center mb-6">
                    <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-4">
                        <ShoppingOutlined
                            style={{ fontSize: 28 }}
                        />
                    </div>

                    <h2 className="text-lg font-semibold text-[#0a0a0a] mb-2">
                        No active orders
                    </h2>

                    <p className="text-[#6b7280] text-sm mb-6">
                        You don't have any orders in
                        progress right now.
                    </p>

                    <Button
                        type="primary"
                        onClick={() =>
                            navigate(
                                '/customer/products'
                            )
                        }
                    >
                        Browse Products
                    </Button>
                </div>
            ) : (
                <div className="space-y-5 mb-6">
                    {activeOrders.map((order) => {
                        const badge = getStatusBadge(
                            order.status
                        )

                        const stepIndex = getStepIndex(
                            order.status
                        )

                        return (
                            <div
                                key={order.id}
                                className="surface-card overflow-hidden"
                            >
                                <div className="flex flex-wrap items-center justify-between gap-3 px-6 py-4 border-b border-[#eaeaea]">
                                    <div className="flex items-center gap-3">
                                        <div className="text-xs uppercase tracking-wide text-[#9ca3af]">
                                            Active order
                                        </div>

                                        <div className="text-base font-semibold text-[#0a0a0a] tabular-nums">
                                            #{order.id}
                                        </div>
                                    </div>

                                    <div
                                        className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium"
                                        style={{
                                            background:
                                            badge.bg,
                                            color: badge.text,
                                        }}
                                    >
                                        <span
                                            className="w-1.5 h-1.5 rounded-full"
                                            style={{
                                                background:
                                                badge.dot,
                                            }}
                                        />
                                        {getStatusLabel(
                                            order.status
                                        )}
                                    </div>
                                </div>

                                <div className="p-6">
                                    <div className="flex items-center gap-2 mb-6">
                                        <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-[#f4f4f5] text-[#6b7280]">
                                            <ShopOutlined
                                                style={{
                                                    fontSize: 14,
                                                }}
                                            />
                                        </div>

                                        <div>
                                            <div className="text-xs text-[#9ca3af]">
                                                Pickup at
                                            </div>

                                            <div className="text-sm font-medium text-[#0a0a0a]">
                                                {getMarketName(
                                                    order
                                                )}
                                            </div>
                                        </div>
                                    </div>

                                    <div className="mb-8">
                                        <div className="flex items-center">
                                            {STATUS_STEPS.map(
                                                (
                                                    label,
                                                    index
                                                ) => {
                                                    const isComplete =
                                                        index <=
                                                        stepIndex

                                                    const isLast =
                                                        index ===
                                                        STATUS_STEPS.length -
                                                        1

                                                    return (
                                                        <div
                                                            key={
                                                                label
                                                            }
                                                            className="flex items-center flex-1 last:flex-none"
                                                        >
                                                            <div className="flex flex-col items-center gap-2">
                                                                <div
                                                                    className={`w-3 h-3 rounded-full transition-all ${
                                                                        isComplete
                                                                            ? 'bg-[#4f46e5] ring-4 ring-[#eef2ff]'
                                                                            : 'bg-[#eaeaea]'
                                                                    }`}
                                                                />

                                                                <span
                                                                    className={`text-xs font-medium whitespace-nowrap ${
                                                                        isComplete
                                                                            ? 'text-[#0a0a0a]'
                                                                            : 'text-[#9ca3af]'
                                                                    }`}
                                                                >
                                                                    {
                                                                        label
                                                                    }
                                                                </span>
                                                            </div>

                                                            {!isLast && (
                                                                <div
                                                                    className={`flex-1 h-0.5 mx-3 -mt-6 transition-all ${
                                                                        index <
                                                                        stepIndex
                                                                            ? 'bg-[#4f46e5]'
                                                                            : 'bg-[#eaeaea]'
                                                                    }`}
                                                                />
                                                            )}
                                                        </div>
                                                    )
                                                }
                                            )}
                                        </div>
                                    </div>

                                    <div className="mb-6">
                                        <div className="text-xs uppercase tracking-wide text-[#9ca3af] mb-3">
                                            Items
                                        </div>

                                        <div className="space-y-2">
                                            {order.items.map(
                                                (
                                                    item
                                                ) => (
                                                    <div
                                                        key={`${order.id}-${item.productId}-${item.marketId}`}
                                                        className="flex justify-between text-sm"
                                                    >
                                                        <div>
                                                            <span className="font-medium text-[#0a0a0a]">
                                                                {productMap.get(
                                                                        item.productId
                                                                    )
                                                                        ?.name ??
                                                                    `Product #${item.productId}`}
                                                            </span>

                                                            <span className="ml-2 text-[#6b7280]">
                                                                ×{' '}
                                                                {
                                                                    item.quantity
                                                                }
                                                            </span>
                                                        </div>

                                                        <span className="text-[#0a0a0a] tabular-nums">
                                                            $
                                                            {item.totalItemPrice.toFixed(
                                                                2
                                                            )}
                                                        </span>
                                                    </div>
                                                )
                                            )}
                                        </div>
                                    </div>

                                    <div className="pt-4 border-t border-[#eaeaea] space-y-3">
                                        <div className="flex justify-between items-baseline">
                                            <span className="text-sm text-[#6b7280]">
                                                Total
                                            </span>

                                            <span className="text-xl font-semibold text-[#0a0a0a] tabular-nums tracking-tight">
                                                $
                                                {order.totalPrice.toFixed(
                                                    2
                                                )}
                                            </span>
                                        </div>

                                        <div className="flex justify-between items-center">
                                            <span className="text-sm text-[#6b7280] flex items-center gap-2">
                                                <ClockCircleOutlined />
                                                Pickup by
                                            </span>

                                            <span className="text-sm text-[#0a0a0a]">
                                                {order.pickUpDeadline
                                                    ? new Date(
                                                        order.pickUpDeadline
                                                    ).toLocaleString()
                                                    : '-'}
                                            </span>
                                        </div>
                                    </div>

                                    {order.status ===
                                        OrderStatus.New && (
                                            <div className="flex justify-end mt-6 pt-4 border-t border-[#eaeaea]">
                                                <Popconfirm
                                                    title="Cancel this order?"
                                                    description="This order has not been accepted yet."
                                                    okText="Yes, Cancel"
                                                    cancelText="Keep Order"
                                                    okButtonProps={{
                                                        danger: true,
                                                    }}
                                                    onConfirm={() =>
                                                        handleCancelOrder(
                                                            order.id
                                                        )
                                                    }
                                                >
                                                    <Button
                                                        danger
                                                        loading={
                                                            cancelOrderMutation.isPending
                                                        }
                                                        disabled={
                                                            cancelOrderMutation.isPending
                                                        }
                                                    >
                                                        Cancel Order
                                                    </Button>
                                                </Popconfirm>
                                            </div>
                                        )}
                                </div>
                            </div>
                        )
                    })}
                </div>
            )}

            <div
                className="surface-card p-5 flex items-center justify-between cursor-pointer hover:shadow-md transition-shadow"
                onClick={() =>
                    navigate('/customer/order-history')
                }
            >
                <div className="flex items-center gap-3">
                    <div className="flex items-center justify-center w-10 h-10 rounded-xl bg-[#f4f4f5] text-[#6b7280]">
                        <HistoryOutlined
                            style={{ fontSize: 16 }}
                        />
                    </div>

                    <div>
                        <div className="text-sm font-semibold text-[#0a0a0a]">
                            My Orders
                        </div>

                        <div className="text-xs text-[#6b7280]">
                            View your completed and
                            cancelled orders
                        </div>
                    </div>
                </div>

                <RightOutlined
                    style={{
                        color: '#9ca3af',
                        fontSize: 12,
                    }}
                />
            </div>
        </div>
    )
}

export default CustomerDashboard