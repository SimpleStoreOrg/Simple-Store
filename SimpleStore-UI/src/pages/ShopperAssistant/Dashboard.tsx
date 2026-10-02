import {
    Button,
    Spin,
    Table,
    message,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
    ShoppingCartOutlined,
    ReloadOutlined,
    RightOutlined,
    InboxOutlined,
    CheckCircleOutlined,
    ClockCircleOutlined,
    StopOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import { getAllOrders } from '../../services/orderService'
import { OrderStatus } from '../../types/orderStatus'
import type { Order } from '../../types/order'
import { useAuthStore } from '../../stores/authStore'
import { getCurrentShopperAssistant } from '../../services/shopperAssistants/shopperAssistantService'

const statusLabels: Record<OrderStatus, string> = {
    [OrderStatus.New]: 'New',
    [OrderStatus.Accepted]: 'Accepted',
    [OrderStatus.Collecting]: 'Collecting',
    [OrderStatus.ReadyToGo]: 'Ready To Go',
    [OrderStatus.Completed]: 'Completed',
    [OrderStatus.CancelledByCustomer]:
        'Cancelled By Customer',
    [OrderStatus.CancelledByShop]:
        'Cancelled By Shop',
}

const statusBadge: Record<
    OrderStatus,
    { bg: string; text: string; dot: string }
> = {
    [OrderStatus.New]: {
        bg: '#fffbeb',
        text: '#d97706',
        dot: '#d97706',
    },
    [OrderStatus.Accepted]: {
        bg: '#eef2ff',
        text: '#4f46e5',
        dot: '#4f46e5',
    },
    [OrderStatus.Collecting]: {
        bg: '#eef2ff',
        text: '#4f46e5',
        dot: '#4f46e5',
    },
    [OrderStatus.ReadyToGo]: {
        bg: '#ecfdf5',
        text: '#059669',
        dot: '#059669',
    },
    [OrderStatus.Completed]: {
        bg: '#ecfdf5',
        text: '#059669',
        dot: '#059669',
    },
    [OrderStatus.CancelledByCustomer]: {
        bg: '#fef2f2',
        text: '#dc2626',
        dot: '#dc2626',
    },
    [OrderStatus.CancelledByShop]: {
        bg: '#fef2f2',
        text: '#dc2626',
        dot: '#dc2626',
    },
}

function ShopperAssistantDashboard() {
    const navigate = useNavigate()

    const userId = useAuthStore(
        (state) => state.userId
    )

    const {
        data,
        isLoading,
        isError,
        error,
        refetch,
        isFetching,
    } = useQuery({
        queryKey: [
            'shopper-assistant-dashboard-orders',
            userId,
        ],
        queryFn: () =>
            getAllOrders(1, 100, undefined),
        enabled: userId !== null,
    })

    const { data: currentAssistant } = useQuery({
        queryKey: ['current-shopper-assistant'],
        queryFn: getCurrentShopperAssistant,
    })

    if (isLoading) {
        return (
            <div className="flex justify-center items-center py-20">
                <Spin size="large" />
            </div>
        )
    }

    if (isError) {
        message.error(
            error instanceof Error
                ? error.message
                : 'Failed to load dashboard.'
        )

        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Dashboard
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280] mb-4">
                        Failed to load dashboard data.
                    </p>

                    <Button
                        onClick={() => refetch()}
                    >
                        Try Again
                    </Button>
                </div>
            </div>
        )
    }

    const orders = data?.items ?? []

    const acceptedCount = orders.filter(
        (order) =>
            order.status === OrderStatus.Accepted &&
            order.shopperAssistantId === userId
    ).length

    const collectingCount = orders.filter(
        (order) =>
            order.status === OrderStatus.Collecting &&
            order.shopperAssistantId === userId
    ).length

    const readyToGoCount = orders.filter(
        (order) =>
            order.status === OrderStatus.ReadyToGo &&
            order.shopperAssistantId === userId
    ).length

    const newOrdersCount = orders.filter(
        (order) =>
            order.status === OrderStatus.New &&
            order.shopperAssistantId === 0
    ).length

    const completedCount = orders.filter(
        (order) =>
            order.status === OrderStatus.Completed &&
            order.shopperAssistantId === userId
    ).length

    const cancelledCount = orders.filter(
        (order) =>
            (order.status ===
                OrderStatus.CancelledByCustomer ||
                order.status ===
                OrderStatus.CancelledByShop) &&
            order.shopperAssistantId === userId
    ).length

    const activeOrders = orders.filter(
        (order) =>
            order.shopperAssistantId === userId &&
            order.status !== OrderStatus.Completed &&
            order.status !==
            OrderStatus.CancelledByCustomer &&
            order.status !==
            OrderStatus.CancelledByShop
    )

    const firstName =
        currentAssistant?.name?.split(' ')[0] ?? ''

    const columns: ColumnsType<Order> = [
        {
            title: 'Order',
            dataIndex: 'id',
            key: 'id',
            render: (id: number) => (
                <span className="font-semibold text-[#0a0a0a] tabular-nums">
                    #{id}
                </span>
            ),
        },
        {
            title: 'Status',
            dataIndex: 'status',
            key: 'status',
            render: (status: OrderStatus) => {
                const badge = statusBadge[status]

                return (
                    <span
                        className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium"
                        style={{
                            background: badge.bg,
                            color: badge.text,
                        }}
                    >
                        <span
                            className="w-1.5 h-1.5 rounded-full"
                            style={{
                                background: badge.dot,
                            }}
                        />
                        {statusLabels[status]}
                    </span>
                )
            },
        },
        {
            title: 'Total',
            dataIndex: 'totalPrice',
            key: 'totalPrice',
            render: (totalPrice: number) => (
                <span className="tabular-nums font-medium text-[#0a0a0a]">
                    ${totalPrice.toFixed(2)}
                </span>
            ),
        },
        {
            title: 'Created',
            dataIndex: 'createdAt',
            key: 'createdAt',
            render: (createdAt: string | null) => (
                <span className="text-[#6b7280] text-xs">
                    {createdAt
                        ? new Date(
                            createdAt
                        ).toLocaleString()
                        : '—'}
                </span>
            ),
        },
        {
            title: '',
            key: 'actions',
            width: 80,
            render: () => (
                <Button
                    type="text"
                    icon={<RightOutlined />}
                    onClick={() =>
                        navigate(
                            '/shopper-assistant/orders'
                        )
                    }
                />
            ),
        },
    ]

    const statCards = [
        {
            label: 'New orders',
            value: newOrdersCount,
            icon: <InboxOutlined />,
            accent: '#d97706',
            bg: '#fffbeb',
        },
        {
            label: 'Accepted',
            value: acceptedCount,
            icon: <CheckCircleOutlined />,
            accent: '#4f46e5',
            bg: '#eef2ff',
        },
        {
            label: 'Collecting',
            value: collectingCount,
            icon: <ClockCircleOutlined />,
            accent: '#4f46e5',
            bg: '#eef2ff',
        },
        {
            label: 'Ready to go',
            value: readyToGoCount,
            icon: <CheckCircleOutlined />,
            accent: '#059669',
            bg: '#ecfdf5',
        },
    ]

    return (
        <div>
            {/* Greeting */}
            <div className="mb-6">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Welcome back
                    {firstName ? `, ${firstName}` : ''}
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Here's what's happening in your
                    market today.
                </p>
            </div>

            {/* Stat cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                {statCards.map((card) => (
                    <div
                        key={card.label}
                        className="surface-card p-5"
                    >
                        <div className="flex items-center justify-between mb-3">
                            <span className="text-xs font-medium text-[#6b7280] uppercase tracking-wide">
                                {card.label}
                            </span>

                            <span
                                className="flex items-center justify-center w-9 h-9 rounded-xl"
                                style={{
                                    background: card.bg,
                                    color: card.accent,
                                    fontSize: 16,
                                }}
                            >
                                {card.icon}
                            </span>
                        </div>

                        <div className="text-3xl font-semibold text-[#0a0a0a] tabular-nums tracking-tight">
                            {card.value}
                        </div>
                    </div>
                ))}
            </div>

            {/* Secondary stats */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
                <div className="surface-card p-5">
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="text-xs font-medium text-[#6b7280] uppercase tracking-wide mb-1">
                                Completed
                            </div>

                            <div className="text-2xl font-semibold text-[#0a0a0a] tabular-nums">
                                {completedCount}
                            </div>
                        </div>

                        <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-[#ecfdf5] text-[#059669]">
                            <CheckCircleOutlined
                                style={{ fontSize: 16 }}
                            />
                        </div>
                    </div>
                </div>

                <div className="surface-card p-5">
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="text-xs font-medium text-[#6b7280] uppercase tracking-wide mb-1">
                                Cancelled
                            </div>

                            <div className="text-2xl font-semibold text-[#0a0a0a] tabular-nums">
                                {cancelledCount}
                            </div>
                        </div>

                        <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-[#fef2f2] text-[#dc2626]">
                            <StopOutlined
                                style={{ fontSize: 16 }}
                            />
                        </div>
                    </div>
                </div>
            </div>

            {/* Active orders */}
            <div className="surface-card overflow-hidden">
                <div className="flex items-center justify-between px-6 py-4 border-b border-[#eaeaea]">
                    <div className="flex items-center gap-2">
                        <ShoppingCartOutlined
                            style={{ color: '#6b7280' }}
                        />

                        <span className="text-sm font-medium text-[#0a0a0a]">
                            My active orders
                        </span>

                        <span className="text-xs text-[#9ca3af] ml-2">
                            {activeOrders.length}
                        </span>
                    </div>

                    <div className="flex items-center gap-2">
                        <Button
                            icon={<ReloadOutlined />}
                            onClick={() => refetch()}
                            loading={isFetching}
                            size="small"
                        >
                            Refresh
                        </Button>

                        <Button
                            type="primary"
                            size="small"
                            onClick={() =>
                                navigate(
                                    '/shopper-assistant/orders'
                                )
                            }
                        >
                            View Orders
                        </Button>
                    </div>
                </div>

                <Table
                    rowKey="id"
                    columns={columns}
                    dataSource={activeOrders}
                    pagination={{
                        pageSize: 5,
                        hideOnSinglePage: true,
                    }}
                    locale={{
                        emptyText: (
                            <div className="py-12">
                                <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-3">
                                    <CheckCircleOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    No active orders
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    New orders assigned to
                                    you will appear here.
                                </div>
                            </div>
                        ),
                    }}
                    scroll={{ x: 700 }}
                />
            </div>
        </div>
    )
}

export default ShopperAssistantDashboard