import {
    Button,
    Descriptions,
    Form,
    InputNumber,
    Modal,
    Popconfirm,
    Select,
    Table,
    message,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
    ReloadOutlined,
    ShoppingCartOutlined,
    CheckCircleOutlined,
    DollarOutlined,
    StopOutlined,
    EditOutlined,
    RightOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
    assignOrder,
    cancelOrderByMarket,
    getAllOrders,
    getOrderById,
    payOrder,
    updateOrderStatus,
} from '../../services/orderService'

import { getAllCustomers } from '../../services/customers/customerService'
import { getAllProducts } from '../../services/products/productService'
import { getAllMarkets } from '../../services/markets/marketService'

import { OrderStatus } from '../../types/orderStatus'

import type { Customer } from '../../types/customer'

import type {
    Order,
    OrderItem,
} from '../../types/order'

import { useAuthStore } from '../../stores/authStore'

interface Product {
    id: number
    name: string
}

interface Market {
    id: number
    name: string
}

interface PaymentResult {
    totalPrice: number
    amountPaid: number
    change: number
}

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

const statusFilterOptions = [
    { value: OrderStatus.New, label: 'New' },
    { value: OrderStatus.Accepted, label: 'Accepted' },
    { value: OrderStatus.Collecting, label: 'Collecting' },
    { value: OrderStatus.ReadyToGo, label: 'Ready To Go' },
    { value: OrderStatus.Completed, label: 'Completed' },
    {
        value: OrderStatus.CancelledByCustomer,
        label: 'Cancelled By Customer',
    },
    {
        value: OrderStatus.CancelledByShop,
        label: 'Cancelled By Shop',
    },
]

function formatDate(date: string | null) {
    if (!date) {
        return '—'
    }

    return new Date(date).toLocaleString()
}

function getNextStatus(
    currentStatus: OrderStatus
): OrderStatus | null {
    switch (currentStatus) {
        case OrderStatus.Accepted:
            return OrderStatus.Collecting

        case OrderStatus.Collecting:
            return OrderStatus.ReadyToGo

        default:
            return null
    }
}

function Orders() {
    const queryClient = useQueryClient()

    const userId = useAuthStore(
        (state) => state.userId
    )

    const [statusFilter, setStatusFilter] =
        useState<number | undefined>(undefined)

    const [selectedOrderId, setSelectedOrderId] =
        useState<number | null>(null)

    const [paymentOrderId, setPaymentOrderId] =
        useState<number | null>(null)

    const [assignOrderId, setAssignOrderId] =
        useState<number | null>(null)

    const [statusOrderId, setStatusOrderId] =
        useState<number | null>(null)

    const [paymentResult, setPaymentResult] =
        useState<PaymentResult | null>(null)

    const [paymentForm] = Form.useForm()

    const {
        data,
        isLoading,
        isFetching,
        refetch,
    } = useQuery({
        queryKey: [
            'shopper-assistant-orders',
            statusFilter,
        ],
        queryFn: () =>
            getAllOrders(
                1,
                100,
                statusFilter
            ),
    })

    const {
        data: customersData,
        isLoading: isLoadingCustomers,
    } = useQuery({
        queryKey: ['customers'],
        queryFn: getAllCustomers,
    })

    const {
        data: productsData,
        isLoading: isLoadingProducts,
    } = useQuery({
        queryKey: ['products'],
        queryFn: () => getAllProducts(),
    })

    const {
        data: marketsData,
        isLoading: isLoadingMarkets,
    } = useQuery({
        queryKey: ['markets'],
        queryFn: getAllMarkets,
    })

    const {
        data: selectedOrder,
        isLoading: isLoadingOrder,
    } = useQuery({
        queryKey: ['order', selectedOrderId],
        queryFn: () =>
            getOrderById(selectedOrderId!),
        enabled: selectedOrderId !== null,
    })

    const customerMap = useMemo(() => {
        const map = new Map<number, string>()

        const customers =
            customersData?.items ?? []

        customers.forEach(
            (customer: Customer) => {
                const fullName = `${
                    customer.name ?? ''
                } ${customer.surname ?? ''}`.trim()

                map.set(
                    customer.id,
                    fullName ||
                    `Customer #${customer.id}`
                )
            }
        )

        return map
    }, [customersData])

    const productMap = useMemo(() => {
        const map = new Map<number, string>()

        const products =
            productsData?.items ?? []

        products.forEach(
            (product: Product) => {
                map.set(product.id, product.name)
            }
        )

        return map
    }, [productsData])

    const marketMap = useMemo(() => {
        const map = new Map<number, string>()

        const markets = marketsData?.items ?? []

        markets.forEach((market: Market) => {
            map.set(market.id, market.name)
        })

        return map
    }, [marketsData])

    const getCustomerName = (customerId: number) => {
        return (
            customerMap.get(customerId) ??
            `Customer #${customerId}`
        )
    }

    const getProductName = (productId: number) => {
        return (
            productMap.get(productId) ??
            `Product #${productId}`
        )
    }

    const getMarketName = (marketId: number) => {
        return (
            marketMap.get(marketId) ??
            `Market #${marketId}`
        )
    }

    const assignMutation = useMutation({
        mutationFn: async (orderId: number) => {
            if (!userId) {
                throw new Error(
                    'User ID was not found.'
                )
            }

            await assignOrder(orderId, {
                shopperAssistantId: userId,
            })
        },

        onSuccess: async () => {
            message.success(
                'Order assigned to you.'
            )

            await queryClient.invalidateQueries({
                queryKey: [
                    'shopper-assistant-orders',
                ],
            })

            await queryClient.invalidateQueries({
                queryKey: ['order', assignOrderId],
            })

            setAssignOrderId(null)
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to assign order.'
            )
        },
    })

    const statusMutation = useMutation({
        mutationFn: async ({
                               orderId,
                               status,
                           }: {
            orderId: number
            status: OrderStatus
        }) => {
            await updateOrderStatus(orderId, {
                status,
            })
        },

        onSuccess: async () => {
            message.success(
                'Order status updated.'
            )

            await queryClient.invalidateQueries({
                queryKey: [
                    'shopper-assistant-orders',
                ],
            })

            await queryClient.invalidateQueries({
                queryKey: ['order', statusOrderId],
            })

            setStatusOrderId(null)
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to update order status.'
            )
        },
    })

    const cancelMutation = useMutation({
        mutationFn: async (orderId: number) => {
            await cancelOrderByMarket(orderId)
        },

        onSuccess: async () => {
            message.success('Order cancelled.')

            await queryClient.invalidateQueries({
                queryKey: [
                    'shopper-assistant-orders',
                ],
            })

            await queryClient.invalidateQueries({
                queryKey: ['order', selectedOrderId],
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

    const payMutation = useMutation({
        mutationFn: async ({
                               orderId,
                               amountPaid,
                           }: {
            orderId: number
            amountPaid: number
        }) => {
            return payOrder(orderId, {
                amountPaid,
            })
        },

        onSuccess: async (result) => {
            setPaymentResult({
                totalPrice: result.total,
                amountPaid: result.paid,
                change: result.change,
            })

            message.success(
                'Order paid successfully.'
            )

            await queryClient.invalidateQueries({
                queryKey: [
                    'shopper-assistant-orders',
                ],
            })

            await queryClient.invalidateQueries({
                queryKey: ['order', paymentOrderId],
            })

            paymentForm.resetFields()
            setPaymentOrderId(null)
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to pay order.'
            )
        },
    })

    const orderColumns: ColumnsType<Order> = [
        {
            title: 'ID',
            dataIndex: 'id',
            key: 'id',
            render: (id: number) => (
                <span className="font-semibold text-[#0a0a0a] tabular-nums">
                    #{id}
                </span>
            ),
        },
        {
            title: 'Customer',
            dataIndex: 'customerId',
            key: 'customerId',
            render: (customerId: number) => (
                <span className="text-[#0a0a0a]">
                    {getCustomerName(customerId)}
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
                    {formatDate(createdAt)}
                </span>
            ),
        },
        {
            title: '',
            key: 'actions',
            width: 260,
            render: (_: unknown, order: Order) => {
                const isTerminal =
                    order.status ===
                    OrderStatus.Completed ||
                    order.status ===
                    OrderStatus.CancelledByCustomer ||
                    order.status ===
                    OrderStatus.CancelledByShop

                const isUnassigned =
                    order.shopperAssistantId === 0

                const isAssignedToMe =
                    order.shopperAssistantId === userId

                const nextStatus = getNextStatus(
                    order.status
                )

                const canAssign =
                    order.status === OrderStatus.New &&
                    isUnassigned

                const canChangeStatus =
                    nextStatus !== null &&
                    isAssignedToMe

                const canPay =
                    order.status ===
                    OrderStatus.ReadyToGo &&
                    isAssignedToMe

                const canCancel =
                    !isTerminal && isAssignedToMe

                return (
                    <div className="flex items-center justify-end gap-1">
                        <Button
                            type="text"
                            icon={<RightOutlined />}
                            onClick={() =>
                                setSelectedOrderId(
                                    order.id
                                )
                            }
                        >
                            View
                        </Button>

                        {canAssign && (
                            <Button
                                type="primary"
                                size="small"
                                icon={<CheckCircleOutlined />}
                                onClick={() =>
                                    setAssignOrderId(
                                        order.id
                                    )
                                }
                            >
                                Assign
                            </Button>
                        )}

                        {canChangeStatus && (
                            <Button
                                size="small"
                                icon={<EditOutlined />}
                                onClick={() =>
                                    setStatusOrderId(
                                        order.id
                                    )
                                }
                            >
                                Status
                            </Button>
                        )}

                        {canPay && (
                            <Button
                                type="primary"
                                size="small"
                                icon={<DollarOutlined />}
                                onClick={() => {
                                    setPaymentResult(
                                        null
                                    )
                                    setPaymentOrderId(
                                        order.id
                                    )
                                }}
                            >
                                Pay
                            </Button>
                        )}

                        {canCancel && (
                            <Popconfirm
                                title="Cancel this order?"
                                description="This will cancel the order and restore product stock. Continue?"
                                okText="Yes, Cancel"
                                cancelText="Keep Order"
                                okButtonProps={{
                                    danger: true,
                                }}
                                onConfirm={() =>
                                    cancelMutation.mutate(
                                        order.id
                                    )
                                }
                            >
                                <Button
                                    type="text"
                                    danger
                                    size="small"
                                    icon={<StopOutlined />}
                                    loading={
                                        cancelMutation.isPending
                                    }
                                    disabled={
                                        cancelMutation.isPending
                                    }
                                />
                            </Popconfirm>
                        )}
                    </div>
                )
            },
        },
    ]

    const itemColumns: ColumnsType<OrderItem> = [
        {
            title: 'Product',
            dataIndex: 'productId',
            key: 'productId',
            render: (productId: number) => (
                <span className="font-medium text-[#0a0a0a]">
                    {getProductName(productId)}
                </span>
            ),
        },
        {
            title: 'Market',
            dataIndex: 'marketId',
            key: 'marketId',
            render: (marketId: number) => (
                <span className="text-[#6b7280]">
                    {getMarketName(marketId)}
                </span>
            ),
        },
        {
            title: 'Quantity',
            dataIndex: 'quantity',
            key: 'quantity',
            render: (quantity: number) => (
                <span className="tabular-nums">
                    {quantity}
                </span>
            ),
        },
        {
            title: 'Price',
            dataIndex: 'price',
            key: 'price',
            render: (price: number) => (
                <span className="tabular-nums text-[#6b7280]">
                    ${price.toFixed(2)}
                </span>
            ),
        },
        {
            title: 'Total',
            dataIndex: 'totalItemPrice',
            key: 'totalItemPrice',
            render: (totalItemPrice: number) => (
                <span className="tabular-nums font-medium text-[#0a0a0a]">
                    ${totalItemPrice.toFixed(2)}
                </span>
            ),
        },
    ]

    const handlePayment = async (values: {
        amountPaid: number
    }) => {
        if (!paymentOrderId) {
            return
        }

        payMutation.mutate({
            orderId: paymentOrderId,
            amountPaid: values.amountPaid,
        })
    }

    const statusOrder = data?.items?.find(
        (order) => order.id === statusOrderId
    )

    const nextStatus = statusOrder
        ? getNextStatus(statusOrder.status)
        : null

    const isLoadingNames =
        isLoadingCustomers ||
        isLoadingProducts ||
        isLoadingMarkets

    const totalOrders = data?.items?.length ?? 0

    return (
        <div>
            {/* Page header */}
            <div className="mb-6">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Orders
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Process and manage orders in your
                    market.
                </p>
            </div>

            {/* Filters bar */}
            <div className="surface-card p-5 mb-6">
                <div className="flex flex-wrap gap-4 items-center justify-between">
                    <div>
                        <div className="text-xs text-[#6b7280] mb-1.5">
                            Status
                        </div>

                        <Select
                            allowClear
                            placeholder="All statuses"
                            style={{ width: 220 }}
                            value={statusFilter}
                            options={statusFilterOptions}
                            onChange={(value) =>
                                setStatusFilter(value)
                            }
                        />
                    </div>

                    <div className="flex items-center gap-3">
                        <span className="text-xs text-[#9ca3af]">
                            {totalOrders}{' '}
                            {totalOrders === 1
                                ? 'order'
                                : 'orders'}
                        </span>

                        <Button
                            icon={<ReloadOutlined />}
                            onClick={() => refetch()}
                            loading={isFetching}
                        >
                            Refresh
                        </Button>
                    </div>
                </div>
            </div>

            {/* Table card */}
            <div className="surface-card overflow-hidden">
                <Table
                    rowKey="id"
                    columns={orderColumns}
                    dataSource={data?.items ?? []}
                    loading={
                        isLoading || isLoadingNames
                    }
                    scroll={{ x: 1000 }}
                    pagination={{
                        pageSize: 10,
                        hideOnSinglePage: true,
                    }}
                    locale={{
                        emptyText: (
                            <div className="py-12">
                                <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-3">
                                    <ShoppingCartOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    {statusFilter !==
                                    undefined
                                        ? 'No orders match this filter'
                                        : 'No orders yet'}
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    {statusFilter !==
                                    undefined
                                        ? 'Try a different status.'
                                        : 'New orders will appear here.'}
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>

            {/* View Order Modal */}
            <Modal
                title={
                    <span className="text-lg font-semibold">
                        Order #{selectedOrderId}
                    </span>
                }
                open={selectedOrderId !== null}
                onCancel={() =>
                    setSelectedOrderId(null)
                }
                footer={null}
                width={860}
            >
                {isLoadingOrder ? (
                    <div className="py-12 text-center text-[#6b7280]">
                        Loading order...
                    </div>
                ) : selectedOrder ? (
                    <>
                        <Descriptions
                            bordered
                            column={2}
                            className="mb-6"
                        >
                            <Descriptions.Item label="Order ID">
                                #
                                {
                                    selectedOrder.id
                                }
                            </Descriptions.Item>

                            <Descriptions.Item label="Customer">
                                {getCustomerName(
                                    selectedOrder.customerId
                                )}
                            </Descriptions.Item>

                            <Descriptions.Item label="Status">
                                <span
                                    className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium"
                                    style={{
                                        background:
                                        statusBadge[
                                            selectedOrder
                                                .status
                                            ].bg,
                                        color: statusBadge[
                                            selectedOrder
                                                .status
                                            ].text,
                                    }}
                                >
                                    <span
                                        className="w-1.5 h-1.5 rounded-full"
                                        style={{
                                            background:
                                            statusBadge[
                                                selectedOrder
                                                    .status
                                                ].dot,
                                        }}
                                    />
                                    {
                                        statusLabels[
                                            selectedOrder
                                                .status
                                            ]
                                    }
                                </span>
                            </Descriptions.Item>

                            <Descriptions.Item label="Total Price">
                                $
                                {selectedOrder.totalPrice.toFixed(
                                    2
                                )}
                            </Descriptions.Item>

                            <Descriptions.Item label="Pickup Deadline">
                                {formatDate(
                                    selectedOrder.pickUpDeadline
                                )}
                            </Descriptions.Item>

                            <Descriptions.Item label="Created">
                                {formatDate(
                                    selectedOrder.createdAt
                                )}
                            </Descriptions.Item>

                            <Descriptions.Item label="Updated">
                                {formatDate(
                                    selectedOrder.updatedAt
                                )}
                            </Descriptions.Item>
                        </Descriptions>

                        <h3 className="text-base font-semibold text-[#0a0a0a] mb-3">
                            Items
                        </h3>

                        <Table
                            rowKey={(item) =>
                                `${item.productId}-${item.marketId}`
                            }
                            columns={itemColumns}
                            dataSource={selectedOrder.items}
                            pagination={false}
                            size="small"
                        />
                    </>
                ) : null}
            </Modal>

            {/* Assign Modal */}
            <Modal
                title="Assign Order"
                open={assignOrderId !== null}
                onCancel={() =>
                    setAssignOrderId(null)
                }
                onOk={() => {
                    if (assignOrderId !== null) {
                        assignMutation.mutate(
                            assignOrderId
                        )
                    }
                }}
                okText="Assign to me"
                confirmLoading={
                    assignMutation.isPending
                }
            >
                <p className="text-[#6b7280]">
                    Assign order #{assignOrderId} to
                    yourself? You'll be responsible
                    for collecting and processing
                    it.
                </p>
            </Modal>

            {/* Change Status Modal */}
            <Modal
                title="Change Order Status"
                open={statusOrderId !== null}
                onCancel={() =>
                    setStatusOrderId(null)
                }
                footer={null}
            >
                {nextStatus !== null ? (
                    <div>
                        <p className="mb-6 text-[#6b7280]">
                            Move order #{statusOrderId}{' '}
                            to{' '}
                            <strong className="text-[#0a0a0a]">
                                {
                                    statusLabels[
                                        nextStatus
                                        ]
                                }
                            </strong>
                            ?
                        </p>

                        <Button
                            type="primary"
                            block
                            size="large"
                            onClick={() => {
                                if (
                                    statusOrderId !==
                                    null &&
                                    nextStatus !== null
                                ) {
                                    statusMutation.mutate({
                                        orderId:
                                        statusOrderId,
                                        status: nextStatus,
                                    })
                                }
                            }}
                            loading={
                                statusMutation.isPending
                            }
                        >
                            Confirm
                        </Button>
                    </div>
                ) : (
                    <p className="text-[#6b7280]">
                        This order cannot change
                        status.
                    </p>
                )}
            </Modal>

            {/* Pay Modal */}
            <Modal
                title="Pay Order"
                open={paymentOrderId !== null}
                onCancel={() => {
                    paymentForm.resetFields()
                    setPaymentOrderId(null)
                }}
                footer={null}
            >
                <Form
                    form={paymentForm}
                    layout="vertical"
                    onFinish={handlePayment}
                    requiredMark={false}
                >
                    <Form.Item
                        label="Amount Paid"
                        name="amountPaid"
                        rules={[
                            {
                                required: true,
                                message:
                                    'Please enter the amount paid.',
                            },
                            {
                                type: 'number',
                                min: 0,
                                message:
                                    'Amount must be greater than or equal to 0.',
                            },
                        ]}
                    >
                        <InputNumber
                            className="w-full"
                            size="large"
                            min={0}
                            precision={2}
                            placeholder="0.00"
                            prefix="$"
                        />
                    </Form.Item>

                    <div className="flex justify-end gap-2 mt-6 pt-4 border-t border-[#eaeaea]">
                        <Button
                            onClick={() => {
                                paymentForm.resetFields()
                                setPaymentOrderId(
                                    null
                                )
                            }}
                            disabled={
                                payMutation.isPending
                            }
                        >
                            Cancel
                        </Button>

                        <Button
                            type="primary"
                            htmlType="submit"
                            loading={
                                payMutation.isPending
                            }
                        >
                            Complete Payment
                        </Button>
                    </div>
                </Form>
            </Modal>

            {/* Payment Success Modal */}
            <Modal
                title="Payment Successful"
                open={paymentResult !== null}
                onCancel={() =>
                    setPaymentResult(null)
                }
                footer={
                    <Button
                        type="primary"
                        size="large"
                        onClick={() =>
                            setPaymentResult(null)
                        }
                    >
                        Done
                    </Button>
                }
            >
                {paymentResult && (
                    <div className="space-y-4">
                        <div className="flex items-center gap-3 mb-6">
                            <div className="flex items-center justify-center w-12 h-12 rounded-2xl bg-[#ecfdf5] text-[#059669]">
                                <CheckCircleOutlined
                                    style={{
                                        fontSize: 22,
                                    }}
                                />
                            </div>

                            <div>
                                <div className="font-semibold text-[#0a0a0a]">
                                    Payment received
                                </div>

                                <div className="text-xs text-[#6b7280]">
                                    Order completed
                                    successfully
                                </div>
                            </div>
                        </div>

                        <div className="space-y-3 pb-5 border-b border-[#eaeaea]">
                            <div className="flex justify-between text-sm">
                                <span className="text-[#6b7280]">
                                    Order Total
                                </span>

                                <span className="tabular-nums text-[#0a0a0a]">
                                    $
                                    {paymentResult.totalPrice.toFixed(
                                        2
                                    )}
                                </span>
                            </div>

                            <div className="flex justify-between text-sm">
                                <span className="text-[#6b7280]">
                                    Amount Paid
                                </span>

                                <span className="tabular-nums text-[#0a0a0a]">
                                    $
                                    {paymentResult.amountPaid.toFixed(
                                        2
                                    )}
                                </span>
                            </div>
                        </div>

                        <div className="flex justify-between items-baseline pt-2">
                            <span className="text-sm font-medium text-[#6b7280]">
                                Change
                            </span>

                            <span className="text-2xl font-semibold text-[#0a0a0a] tabular-nums tracking-tight">
                                $
                                {paymentResult.change.toFixed(
                                    2
                                )}
                            </span>
                        </div>
                    </div>
                )}
            </Modal>
        </div>
    )
}

export default Orders