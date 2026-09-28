import {
    Button,
    Spin,
    message,
} from 'antd'
import {
    HistoryOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useState } from 'react'

import { getOrderHistory } from '../../services/orderHistoryService'
import { getAllProducts } from '../../services/products/productService'

import type { Order } from '../../types/order'
import type { Product } from '../../types/product'

import { OrderStatus } from '../../types/orderStatus'

import ReviewModal from '../../components/ReviewModal'

interface ReviewTarget {
    orderId: number
    productId: number
    productName: string
}

function OrderHistory() {
    const navigate = useNavigate()

    const [reviewTarget, setReviewTarget] =
        useState<ReviewTarget | null>(null)

    const {
        data: ordersData,
        isLoading: ordersLoading,
        isError: ordersError,
        error: ordersErrorMessage,
    } = useQuery({
        queryKey: ['customer-order-history'],
        queryFn: () => getOrderHistory(),
    })

    const {
        data: productsData,
        isLoading: productsLoading,
        isError: productsError,
    } = useQuery({
        queryKey: ['products'],
        queryFn: () => getAllProducts(),
    })

    if (ordersLoading || productsLoading) {
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
                : 'Failed to load order history.'
        )

        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    My Orders
                </h1>

                <p className="text-[#6b7280]">
                    Failed to load your order history.
                </p>
            </div>
        )
    }

    if (productsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    My Orders
                </h1>

                <p className="text-[#6b7280]">
                    Failed to load product information.
                </p>
            </div>
        )
    }

    const orders: Order[] =
        ordersData?.items ?? []

    const completedOrCancelledOrders = orders
        .filter(
            (order) =>
                order.status ===
                OrderStatus.Completed ||
                order.status ===
                OrderStatus.CancelledByCustomer ||
                order.status ===
                OrderStatus.CancelledByShop
        )
        .sort((a, b) => b.id - a.id)

    const products: Product[] =
        productsData?.items ?? []

    const productMap = new Map(
        products.map((product) => [
            product.id,
            product,
        ])
    )

    const isFromToday = (createdAt: string | null) => {
        if (!createdAt) {
            return false
        }

        const orderDate = new Date(createdAt)

        const today = new Date()

        return (
            orderDate.getUTCFullYear() ===
                today.getUTCFullYear() &&
            orderDate.getUTCMonth() ===
                today.getUTCMonth() &&
            orderDate.getUTCDate() ===
                today.getUTCDate()
        )
    }

    const getStatusLabel = (
        status: OrderStatus
    ) => {
        switch (status) {
            case OrderStatus.Completed:
                return 'Completed'

            case OrderStatus.CancelledByCustomer:
                return 'Cancelled by you'

            case OrderStatus.CancelledByShop:
                return 'Cancelled by shop'

            default:
                return 'Unknown'
        }
    }

    const getStatusBadge = (
        status: OrderStatus
    ) => {
        switch (status) {
            case OrderStatus.Completed:
                return {
                    bg: '#ecfdf5',
                    text: '#059669',
                    dot: '#059669',
                }

            case OrderStatus.CancelledByCustomer:
            case OrderStatus.CancelledByShop:
                return {
                    bg: '#fef2f2',
                    text: '#dc2626',
                    dot: '#dc2626',
                }

            default:
                return {
                    bg: '#f4f4f5',
                    text: '#6b7280',
                    dot: '#9ca3af',
                }
        }
    }

    return (
        <div>
            {/* Page header */}
            <div className="mb-8">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    My Orders
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Your completed and cancelled
                    orders.
                </p>
            </div>

            {completedOrCancelledOrders.length === 0 ? (
                <div className="surface-card p-12 text-center">
                    <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-4">
                        <HistoryOutlined
                            style={{ fontSize: 28 }}
                        />
                    </div>

                    <h2 className="text-lg font-semibold text-[#0a0a0a] mb-2">
                        No orders yet
                    </h2>

                    <p className="text-[#6b7280] text-sm mb-6">
                        Once you complete or cancel
                        an order, it will appear
                        here.
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
                <div className="space-y-4">
                    {completedOrCancelledOrders.map(
                        (order) => {
                            const badge =
                                getStatusBadge(
                                    order.status
                                )

                            const canReview =
                                order.status ===
                                    OrderStatus.Completed &&
                                isFromToday(order.createdAt)

                            return (
                                <div
                                    key={order.id}
                                    className="surface-card overflow-hidden"
                                >
                                    {/* Header */}
                                    <div className="flex flex-wrap items-center justify-between gap-3 px-6 py-4 border-b border-[#eaeaea]">
                                        <div className="flex items-center gap-3">
                                            <div className="text-xs uppercase tracking-wide text-[#9ca3af]">
                                                Order
                                            </div>

                                            <div className="text-base font-semibold text-[#0a0a0a] tabular-nums">
                                                #{order.id}
                                            </div>
                                        </div>

                                        <div className="flex items-center gap-4">
                                            <span className="text-xs text-[#9ca3af]">
                                                {order.createdAt
                                                    ? new Date(
                                                        order.createdAt
                                                    ).toLocaleString()
                                                    : '-'}
                                            </span>

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
                                    </div>

                                    {/* Body */}
                                    <div className="p-6">
                                        {/* Items */}
                                        <div className="mb-5">
                                            <div className="text-xs uppercase tracking-wide text-[#9ca3af] mb-3">
                                                Items
                                            </div>

                                            <div className="space-y-3">
                                                {order.items.map(
                                                    (
                                                        item
                                                    ) => {
                                                        const productName =
                                                            productMap.get(
                                                                    item.productId
                                                                )
                                                                    ?.name ??
                                                            `Product #${item.productId}`

                                                        return (
                                                            <div
                                                                key={`${order.id}-${item.productId}-${item.marketId}`}
                                                                className="flex justify-between items-center text-sm gap-4"
                                                            >
                                                                <div className="flex-1 min-w-0">
                                                                    <span className="font-medium text-[#0a0a0a]">
                                                                        {
                                                                            productName
                                                                        }
                                                                    </span>

                                                                    <span className="ml-2 text-[#6b7280]">
                                                                        ×{' '}
                                                                        {
                                                                            item.quantity
                                                                        }
                                                                    </span>
                                                                </div>

                                                                <div className="flex items-center gap-3">
                                                                    <span className="text-[#0a0a0a] tabular-nums">
                                                                        $
                                                                        {item.totalItemPrice.toFixed(
                                                                            2
                                                                        )}
                                                                    </span>

                                                                    {canReview && (
                                                                        <Button
                                                                            size="small"
                                                                            type="primary"
                                                                            ghost
                                                                            onClick={() =>
                                                                                setReviewTarget(
                                                                                    {
                                                                                        orderId:
                                                                                            order.id,
                                                                                        productId:
                                                                                            item.productId,
                                                                                        productName,
                                                                                    }
                                                                                )
                                                                            }
                                                                        >
                                                                            Leave a Review
                                                                        </Button>
                                                                    )}
                                                                </div>
                                                            </div>
                                                        )
                                                    }
                                                )}
                                            </div>
                                        </div>

                                        {/* Totals */}
                                        <div className="pt-4 border-t border-[#eaeaea] flex justify-between items-baseline">
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
                                    </div>
                                </div>
                            )
                        }
                    )}
                </div>
            )}

            {reviewTarget && (
                <ReviewModal
                    open={true}
                    orderId={reviewTarget.orderId}
                    productId={reviewTarget.productId}
                    productName={reviewTarget.productName}
                    onClose={() =>
                        setReviewTarget(null)
                    }
                />
            )}
        </div>
    )
}

export default OrderHistory
