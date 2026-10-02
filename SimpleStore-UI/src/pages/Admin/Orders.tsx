import {
    Button,
    Descriptions,
    Modal,
    Select,
    Space,
    Table,
    Tag,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
    getAllOrders,
    getOrderById,
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

interface Product {
    id: number
    name: string
}

interface Market {
    id: number
    name: string
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

const statusColors: Record<OrderStatus, string> = {
    [OrderStatus.New]: 'gold',
    [OrderStatus.Accepted]: 'blue',
    [OrderStatus.Collecting]: 'processing',
    [OrderStatus.ReadyToGo]: 'green',
    [OrderStatus.Completed]: 'success',
    [OrderStatus.CancelledByCustomer]: 'red',
    [OrderStatus.CancelledByShop]: 'red',
}

const statusFilterOptions = [
    { value: OrderStatus.New, label: 'New' },
    { value: OrderStatus.Accepted, label: 'Accepted' },
    {
        value: OrderStatus.Collecting,
        label: 'Collecting',
    },
    {
        value: OrderStatus.ReadyToGo,
        label: 'Ready To Go',
    },
    {
        value: OrderStatus.Completed,
        label: 'Completed',
    },
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
        return '-'
    }

    return new Date(date).toLocaleString()
}

function Orders() {
    const [statusFilter, setStatusFilter] =
        useState<number | undefined>(undefined)

    const [selectedOrderId, setSelectedOrderId] =
        useState<number | null>(null)

    const {
        data,
        isLoading,
        isFetching,
        refetch,
    } = useQuery({
        queryKey: ['admin-orders', statusFilter],
        queryFn: () =>
            getAllOrders(1, 100, statusFilter),
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

        const markets =
            marketsData?.items ?? []

        markets.forEach((market: Market) => {
            map.set(market.id, market.name)
        })

        return map
    }, [marketsData])

    const getCustomerName = (
        customerId: number
    ) => {
        return (
            customerMap.get(customerId) ??
            `Customer #${customerId}`
        )
    }

    const getProductName = (
        productId: number
    ) => {
        return (
            productMap.get(productId) ??
            `Product #${productId}`
        )
    }

    const getMarketName = (
        marketId: number
    ) => {
        return (
            marketMap.get(marketId) ??
            `Market #${marketId}`
        )
    }

    const orderColumns: ColumnsType<Order> = [
        {
            title: 'ID',
            dataIndex: 'id',
            key: 'id',
        },
        {
            title: 'Customer',
            dataIndex: 'customerId',
            key: 'customerId',
            render: (customerId: number) =>
                getCustomerName(customerId),
        },
        {
            title: 'Market',
            key: 'market',
            render: (_: unknown, order: Order) => {
                const marketId =
                    order.items?.[0]?.marketId

                if (!marketId) {
                    return '-'
                }

                return getMarketName(marketId)
            },
        },
        {
            title: 'Status',
            dataIndex: 'status',
            key: 'status',
            render: (status: OrderStatus) => (
                <Tag color={statusColors[status]}>
                    {statusLabels[status]}
                </Tag>
            ),
        },
        {
            title: 'Total',
            dataIndex: 'totalPrice',
            key: 'totalPrice',
            render: (totalPrice: number) =>
                `$${totalPrice.toFixed(2)}`,
        },
        {
            title: 'Created',
            dataIndex: 'createdAt',
            key: 'createdAt',
            render: (
                createdAt: string | null
            ) => formatDate(createdAt),
        },
        {
            title: 'Actions',
            key: 'actions',
            render: (_: unknown, order: Order) => (
                <Button
                    onClick={() =>
                        setSelectedOrderId(order.id)
                    }
                >
                    View
                </Button>
            ),
        },
    ]

    const itemColumns: ColumnsType<OrderItem> = [
        {
            title: 'Market',
            dataIndex: 'marketId',
            key: 'marketId',
            render: (marketId: number) =>
                getMarketName(marketId),
        },
        {
            title: 'Product',
            dataIndex: 'productId',
            key: 'productId',
            render: (productId: number) =>
                getProductName(productId),
        },
        {
            title: 'Quantity',
            dataIndex: 'quantity',
            key: 'quantity',
        },
        {
            title: 'Price',
            dataIndex: 'price',
            key: 'price',
            render: (price: number) =>
                price.toFixed(2),
        },
        {
            title: 'Total',
            dataIndex: 'totalItemPrice',
            key: 'totalItemPrice',
            render: (
                totalItemPrice: number
            ) => totalItemPrice.toFixed(2),
        },
    ]

    const isLoadingNames =
        isLoadingCustomers ||
        isLoadingProducts ||
        isLoadingMarkets

    return (
        <div>
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Orders
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        Monitor orders across your
                        markets.
                    </p>
                </div>

                <Space>
                    <Select
                        allowClear
                        placeholder="Filter by status"
                        style={{ width: 220 }}
                        value={statusFilter}
                        options={statusFilterOptions}
                        onChange={(value) =>
                            setStatusFilter(value)
                        }
                    />

                    <Button
                        onClick={() => refetch()}
                        loading={isFetching}
                    >
                        Refresh
                    </Button>
                </Space>
            </div>

            <div className="surface-card overflow-hidden">
                <Table
                    rowKey="id"
                    columns={orderColumns}
                    dataSource={data?.items ?? []}
                    loading={
                        isLoading || isLoadingNames
                    }
                    scroll={{ x: 1200 }}
                    pagination={{ pageSize: 10 }}
                />
            </div>

            <Modal
                title={`Order #${selectedOrderId}`}
                open={selectedOrderId !== null}
                onCancel={() =>
                    setSelectedOrderId(null)
                }
                footer={null}
                width={900}
            >
                {isLoadingOrder ? (
                    <div>Loading order...</div>
                ) : selectedOrder ? (
                    <>
                        <Descriptions
                            bordered
                            column={2}
                            className="mb-6"
                        >
                            <Descriptions.Item label="Order ID">
                                {selectedOrder.id}
                            </Descriptions.Item>

                            <Descriptions.Item label="Customer">
                                {getCustomerName(
                                    selectedOrder.customerId
                                )}
                            </Descriptions.Item>

                            <Descriptions.Item label="Status">
                                <Tag
                                    color={
                                        statusColors[
                                            selectedOrder
                                                .status
                                            ]
                                    }
                                >
                                    {
                                        statusLabels[
                                            selectedOrder
                                                .status
                                            ]
                                    }
                                </Tag>
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

                        <h3 className="text-lg font-semibold mb-3">
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
        </div>
    )
}

export default Orders