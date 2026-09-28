import {
    Card,
    Col,
    Empty,
    Rate,
    Row,
    Spin,
    Statistic,
    Table,
    Tabs,
    Tag,
} from 'antd'
import {
    DollarOutlined,
    ShoppingOutlined,
    StarOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { getTotalRevenue, getTopProductsByCategory } from '../../services/reports/reportService'
import { getAllReviews } from '../../services/reviews/reviewService'
import { getAllCategories } from '../../services/categories/categoryService'

import type { Category } from '../../types/category'
import type { TopProduct } from '../../types/report'
import type { TopProductsByCategory } from '../../types/report'

interface ReviewRow {
    id: number
    productId: number
    customerId: number
    rating: number
    message: string | null
    createdAt: string
}

function Reports() {
    const [activeTab, setActiveTab] = useState<string>('revenue')

    const {
        data: revenueData,
        isLoading: revenueLoading,
        isError: revenueError,
    } = useQuery({
        queryKey: ['reports-total-revenue'],
        queryFn: () => getTotalRevenue(),
        enabled: activeTab === 'revenue',
    })

    const {
        data: topProductsData,
        isLoading: topProductsLoading,
        isError: topProductsError,
    } = useQuery({
        queryKey: ['reports-top-products'],
        queryFn: () => getTopProductsByCategory(1, 50),
        enabled: activeTab === 'top-products',
    })

    const {
        data: reviewsData,
        isLoading: reviewsLoading,
        isError: reviewsError,
    } = useQuery({
        queryKey: ['reports-all-reviews'],
        queryFn: () => getAllReviews(1, 100),
        enabled: activeTab === 'reviews',
    })

    const {
        data: categoriesData,
    } = useQuery({
        queryKey: ['categories'],
        queryFn: getAllCategories,
    })

    const categoryMap = useMemo(() => {
        const map = new Map<number, Category>()

        const items = categoriesData?.items ?? []

        items.forEach((category: Category) => {
            map.set(category.id, category)
        })

        return map
    }, [categoriesData])

    const revenueTab = (
        <div>
            {revenueLoading ? (
                <div className="flex justify-center items-center py-20">
                    <Spin size="large" />
                </div>
            ) : revenueError ? (
                <Empty description="Failed to load revenue data." />
            ) : (
                <Row gutter={[16, 16]}>
                    <Col xs={24} md={12}>
                        <Card>
                            <Statistic
                                title="Total Revenue"
                                value={Number(
                                    revenueData?.totalRevenue ?? 0
                                )}
                                precision={2}
                                prefix={<DollarOutlined />}
                            />
                        </Card>
                    </Col>

                    <Col xs={24} md={12}>
                        <Card>
                            <Statistic
                                title="Total Orders"
                                value={revenueData?.totalOrders ?? 0}
                                prefix={<ShoppingOutlined />}
                            />
                        </Card>
                    </Col>
                </Row>
            )}
        </div>
    )

    const topProductsColumns = [
        {
            title: 'Product',
            dataIndex: 'productName',
            key: 'productName',
            render: (name: string | null, row: TopProduct) =>
                name ?? `Product #${row.productId}`,
        },
        {
            title: 'Price',
            dataIndex: 'price',
            key: 'price',
            render: (price: number) => `$${price.toFixed(2)}`,
        },
        {
            title: 'Sold',
            dataIndex: 'soldQuantity',
            key: 'soldQuantity',
            render: (quantity: number) => quantity.toFixed(0),
        },
    ]

    const topProductsTab = (
        <div>
            {topProductsLoading ? (
                <div className="flex justify-center items-center py-20">
                    <Spin size="large" />
                </div>
            ) : topProductsError ? (
                <Empty description="Failed to load top products." />
            ) : (topProductsData?.items ?? []).length === 0 ? (
                <Empty description="No product sales yet." />
            ) : (
                <div className="space-y-6">
                    {(topProductsData?.items ?? []).map(
                        (group: TopProductsByCategory) => {
                            const categoryName =
                                categoryMap.get(group.categoryId)?.name ??
                                `Category #${group.categoryId}`

                            return (
                                <Card
                                    key={group.categoryId}
                                    title={categoryName}
                                >
                                    <Table
                                        dataSource={group.products}
                                        columns={topProductsColumns}
                                        rowKey="productId"
                                        pagination={false}
                                        size="small"
                                    />
                                </Card>
                            )
                        }
                    )}
                </div>
            )}
        </div>
    )

    const reviewColumns = [
        {
            title: 'Review ID',
            dataIndex: 'id',
            key: 'id',
        },
        {
            title: 'Product ID',
            dataIndex: 'productId',
            key: 'productId',
        },
        {
            title: 'Customer ID',
            dataIndex: 'customerId',
            key: 'customerId',
        },
        {
            title: 'Rating',
            dataIndex: 'rating',
            key: 'rating',
            render: (rating: number) => (
                <Rate
                    disabled
                    value={rating}
                    style={{ fontSize: 14 }}
                />
            ),
        },
        {
            title: 'Message',
            dataIndex: 'message',
            key: 'message',
            render: (message: string | null) =>
                message ? (
                    <span>{message}</span>
                ) : (
                    <Tag color="default">No message</Tag>
                ),
        },
        {
            title: 'Created',
            dataIndex: 'createdAt',
            key: 'createdAt',
            render: (value: string) =>
                new Date(value).toLocaleString(),
        },
    ]

    const reviewsTab = (
        <div>
            {reviewsLoading ? (
                <div className="flex justify-center items-center py-20">
                    <Spin size="large" />
                </div>
            ) : reviewsError ? (
                <Empty description="Failed to load reviews." />
            ) : (
                <Table
                    dataSource={(reviewsData?.items ?? []) as ReviewRow[]}
                    columns={reviewColumns}
                    rowKey="id"
                    pagination={{
                        pageSize: 10,
                    }}
                />
            )}
        </div>
    )

    return (
        <div>
            {/* Page header */}
            <div className="mb-8">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Reports
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Revenue, top products, and customer reviews.
                </p>
            </div>

            <div className="surface-card p-6">
                <Tabs
                    activeKey={activeTab}
                    onChange={setActiveTab}
                    items={[
                        {
                            key: 'revenue',
                            label: (
                                <span>
                                    <DollarOutlined /> Revenue
                                </span>
                            ),
                            children: revenueTab,
                        },
                        {
                            key: 'top-products',
                            label: (
                                <span>
                                    <ShoppingOutlined /> Top Products
                                </span>
                            ),
                            children: topProductsTab,
                        },
                        {
                            key: 'reviews',
                            label: (
                                <span>
                                    <StarOutlined /> Reviews
                                </span>
                            ),
                            children: reviewsTab,
                        },
                    ]}
                />
            </div>
        </div>
    )
}

export default Reports
