import {
    Button,
    Form,
    Input,
    InputNumber,
    Modal,
    Popconfirm,
    Select,
    Switch,
    Table,
    TreeSelect,
} from 'antd'
import {
    PlusOutlined,
    FilterOutlined,
    EditOutlined,
    DeleteOutlined,
    InboxOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { useAuthStore } from '../../stores/authStore'

import {
    createProduct,
    deleteProduct,
    getAllProducts,
    updateProduct,
} from '../../services/products/productService'

import { getAllCategories } from '../../services/categories/categoryService'
import { getAllMarkets } from '../../services/markets/marketService'

import type { Product } from '../../types/product'
import type { Category } from '../../types/category'
import type { Market } from '../../types/market'

interface ProductFormValues {
    name: string
    price: number
    stock: number
    categoryId: number
}

interface ProductFilters {
    categoryId?: number
    marketId?: number
    minPrice?: number
    maxPrice?: number
    onlyAvailable: boolean
}

function Products() {
    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const isMarketAdmin =
        adminPosition === 'MarketAdmin'

    const isSuperAdmin =
        adminPosition === 'SuperAdmin'

    const [isModalOpen, setIsModalOpen] =
        useState(false)

    const [editingProduct, setEditingProduct] =
        useState<Product | null>(null)

    const [filters, setFilters] =
        useState<ProductFilters>({
            onlyAvailable: false,
        })

    const [form] =
        Form.useForm<ProductFormValues>()

    const queryClient = useQueryClient()

    const queryParams = useMemo(() => {
        const params: {
            categoryIds?: number[]
            minPrice?: number
            maxPrice?: number
            isAvailable?: boolean
            marketId?: number
        } = {}

        if (filters.categoryId) {
            params.categoryIds = [
                filters.categoryId,
            ]
        }

        if (filters.minPrice !== undefined) {
            params.minPrice = filters.minPrice
        }

        if (filters.maxPrice !== undefined) {
            params.maxPrice = filters.maxPrice
        }

        if (filters.onlyAvailable) {
            params.isAvailable = true
        }

        if (filters.marketId) {
            params.marketId = filters.marketId
        }

        return params
    }, [filters])

    const {
        data,
        isLoading,
        isError,
        error,
    } = useQuery({
        queryKey: ['products', queryParams],
        queryFn: () =>
            getAllProducts(queryParams),
    })

    const {
        data: categoriesData,
        isLoading: categoriesLoading,
    } = useQuery({
        queryKey: ['categories'],
        queryFn: getAllCategories,
    })

    const {
        data: marketsData,
        isLoading: marketsLoading,
    } = useQuery({
        queryKey: ['markets'],
        queryFn: getAllMarkets,
    })

    const categoryTreeOptions = useMemo(() => {
        const items = categoriesData?.items ?? []

        interface TreeOption {
            value: number
            title: string
            disabled: boolean
            children?: TreeOption[]
        }

        const build = (
            parentId: number | null
        ): TreeOption[] => {
            return items
                .filter(
                    (c: Category) =>
                        c.parentCategoryId === parentId
                )
                .sort(
                    (a: Category, b: Category) =>
                        a.name.localeCompare(b.name)
                )
                .map((c: Category) => {
                    const children = build(c.id)

                    const isParent =
                        children.length > 0

                    return {
                        value: c.id,
                        title: c.name,
                        disabled: isParent,
                        children:
                            children.length > 0
                                ? children
                                : undefined,
                    }
                })
        }

        return build(null)
    }, [categoriesData])

    const categoryMap = useMemo(() => {
        const map = new Map<number, string>()

        const categories =
            categoriesData?.items ?? []

        categories.forEach(
            (category: Category) => {
                map.set(
                    category.id,
                    category.name
                )
            }
        )

        return map
    }, [categoriesData])

    const marketMap = useMemo(() => {
        const map = new Map<number, string>()

        const markets = marketsData?.items ?? []

        markets.forEach((market: Market) => {
            map.set(market.id, market.name)
        })

        return map
    }, [marketsData])

    const getCategoryName = (
        categoryId: number
    ) => {
        return (
            categoryMap.get(categoryId) ??
            `Category #${categoryId}`
        )
    }

    const getMarketName = (marketId: number) => {
        return (
            marketMap.get(marketId) ??
            `Market #${marketId}`
        )
    }

    const createMutation = useMutation({
        mutationFn: createProduct,

        onSuccess: () => {
            setIsModalOpen(false)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['products'],
            })
        },
    })

    const updateMutation = useMutation({
        mutationFn: ({
                         id,
                         data,
                     }: {
            id: number
            data: ProductFormValues
        }) => updateProduct(id, data),

        onSuccess: () => {
            setIsModalOpen(false)
            setEditingProduct(null)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['products'],
            })
        },
    })

    const deleteMutation = useMutation({
        mutationFn: deleteProduct,

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ['products'],
            })
        },
    })

    const handleSubmit = (
        values: ProductFormValues
    ) => {
        if (editingProduct) {
            updateMutation.mutate({
                id: editingProduct.id,
                data: values,
            })
        } else {
            createMutation.mutate(values)
        }
    }

    const handleEdit = (product: Product) => {
        setEditingProduct(product)

        form.setFieldsValue({
            name: product.name,
            price: product.price,
            stock: product.stock,
            categoryId: product.categoryId,
        })

        setIsModalOpen(true)
    }

    const handleAdd = () => {
        setEditingProduct(null)
        form.resetFields()
        setIsModalOpen(true)
    }

    const handleCancel = () => {
        setIsModalOpen(false)
        setEditingProduct(null)
        form.resetFields()
    }

    const clearFilters = () => {
        setFilters({
            onlyAvailable: false,
        })
    }

    if (isError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Products
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Error: {error.message}
                    </p>
                </div>
            </div>
        )
    }

    const products: Product[] = data?.items ?? []

    const columns = [
        {
            title: 'Name',
            dataIndex: 'name',
            render: (name: string) => (
                <span className="font-medium text-[#0a0a0a]">
                    {name}
                </span>
            ),
        },
        {
            title: 'Price',
            dataIndex: 'price',
            render: (price: number) => (
                <span className="tabular-nums">
                    ${price.toFixed(2)}
                </span>
            ),
        },
        {
            title: 'Stock',
            dataIndex: 'stock',
            render: (stock: number) => {
                if (stock === 0) {
                    return (
                        <span className="inline-flex items-center gap-1.5 text-xs font-medium text-[#6b7280] bg-[#f4f4f5] px-2.5 py-1 rounded-full">
                            <span className="w-1.5 h-1.5 rounded-full bg-[#9ca3af]" />
                            Out
                        </span>
                    )
                }

                if (stock <= 5) {
                    return (
                        <span className="inline-flex items-center gap-1.5 text-xs font-medium text-[#d97706] bg-[#fffbeb] px-2.5 py-1 rounded-full">
                            <span className="w-1.5 h-1.5 rounded-full bg-[#d97706]" />
                            {stock} left
                        </span>
                    )
                }

                return (
                    <span className="inline-flex items-center gap-1.5 text-xs font-medium text-[#059669] bg-[#ecfdf5] px-2.5 py-1 rounded-full">
                        <span className="w-1.5 h-1.5 rounded-full bg-[#059669]" />
                        {stock}
                    </span>
                )
            },
        },
        {
            title: 'Category',
            dataIndex: 'categoryId',
            render: (categoryId: number) => (
                <span className="text-[#6b7280]">
                    {getCategoryName(categoryId)}
                </span>
            ),
        },
        {
            title: 'Market',
            dataIndex: 'marketId',
            render: (marketId: number) => (
                <span className="text-[#6b7280]">
                    {getMarketName(marketId)}
                </span>
            ),
        },
        ...(isMarketAdmin
            ? [
                {
                    title: '',
                    key: 'actions',
                    width: 120,
                    render: (
                        _: unknown,
                        product: Product
                    ) => (
                        <div className="flex justify-end gap-1">
                            <Button
                                type="text"
                                icon={<EditOutlined />}
                                onClick={() =>
                                    handleEdit(product)
                                }
                            />

                            <Popconfirm
                                title="Delete this product?"
                                description="This action cannot be undone."
                                okText="Delete"
                                cancelText="Cancel"
                                okButtonProps={{
                                    danger: true,
                                }}
                                onConfirm={() =>
                                    deleteMutation.mutate(
                                        product.id
                                    )
                                }
                            >
                                <Button
                                    type="text"
                                    danger
                                    icon={
                                        <DeleteOutlined />
                                    }
                                />
                            </Popconfirm>
                        </div>
                    ),
                },
            ]
            : []),
    ]

    return (
        <div>
            {/* Page header */}
            <div className="flex items-start justify-between mb-6">
                <div>
                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Products
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        {isMarketAdmin
                            ? 'Manage the products in your market.'
                            : 'Browse products across all markets.'}
                    </p>
                </div>

                {isMarketAdmin && (
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={handleAdd}
                        size="large"
                    >
                        Add Product
                    </Button>
                )}
            </div>

            {/* Filters card */}
            <div className="surface-card p-5 mb-6">
                <div className="flex items-center gap-2 mb-4">
                    <FilterOutlined
                        style={{ color: '#6b7280' }}
                    />

                    <span className="text-sm font-medium text-[#0a0a0a]">
                        Filters
                    </span>
                </div>

                <div className="flex flex-wrap gap-4 items-end">
                    <div>
                        <div className="text-xs text-[#6b7280] mb-1.5">
                            Category
                        </div>

                        <TreeSelect
                            allowClear
                            placeholder="All categories"
                            style={{ width: 220 }}
                            value={filters.categoryId}
                            treeData={
                                categoryTreeOptions
                            }
                            treeDefaultExpandAll
                            onChange={(value) =>
                                setFilters((prev) => ({
                                    ...prev,
                                    categoryId: value,
                                }))
                            }
                        />
                    </div>

                    {isSuperAdmin && (
                        <div>
                            <div className="text-xs text-[#6b7280] mb-1.5">
                                Market
                            </div>

                            <Select
                                allowClear
                                placeholder="All markets"
                                style={{ width: 200 }}
                                value={filters.marketId}
                                options={
                                    marketsData?.items?.map(
                                        (m: Market) => ({
                                            value: m.id,
                                            label: m.name,
                                        })
                                    ) ?? []
                                }
                                onChange={(value) =>
                                    setFilters(
                                        (prev) => ({
                                            ...prev,
                                            marketId: value,
                                        })
                                    )
                                }
                            />
                        </div>
                    )}

                    <div>
                        <div className="text-xs text-[#6b7280] mb-1.5">
                            Min Price
                        </div>

                        <InputNumber
                            min={0}
                            placeholder="0"
                            style={{ width: 120 }}
                            value={filters.minPrice}
                            onChange={(value) =>
                                setFilters((prev) => ({
                                    ...prev,
                                    minPrice:
                                        value ?? undefined,
                                }))
                            }
                        />
                    </div>

                    <div>
                        <div className="text-xs text-[#6b7280] mb-1.5">
                            Max Price
                        </div>

                        <InputNumber
                            min={0}
                            placeholder="Any"
                            style={{ width: 120 }}
                            value={filters.maxPrice}
                            onChange={(value) =>
                                setFilters((prev) => ({
                                    ...prev,
                                    maxPrice:
                                        value ?? undefined,
                                }))
                            }
                        />
                    </div>

                    <div>
                        <div className="text-xs text-[#6b7280] mb-1.5">
                            In Stock Only
                        </div>

                        <Switch
                            checked={
                                filters.onlyAvailable
                            }
                            onChange={(checked) =>
                                setFilters((prev) => ({
                                    ...prev,
                                    onlyAvailable:
                                    checked,
                                }))
                            }
                        />
                    </div>

                    <Button onClick={clearFilters}>
                        Clear Filters
                    </Button>
                </div>
            </div>

            {/* Table card */}
            <div className="surface-card overflow-hidden">
                <Table
                    loading={
                        isLoading ||
                        marketsLoading ||
                        categoriesLoading
                    }
                    dataSource={products}
                    rowKey="id"
                    columns={columns}
                    pagination={{
                        pageSize: 10,
                        hideOnSinglePage: true,
                    }}
                    locale={{
                        emptyText: (
                            <div className="py-12">
                                <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-3">
                                    <InboxOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    No products found
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    Try adjusting your
                                    filters.
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>

            {/* Create / Edit modal */}
            <Modal
                title={
                    editingProduct
                        ? 'Edit Product'
                        : 'Add Product'
                }
                open={isModalOpen}
                onCancel={handleCancel}
                footer={null}
                width={520}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                    requiredMark={false}
                    className="mt-4"
                >
                    <Form.Item
                        label="Name"
                        name="name"
                        rules={[
                            {
                                required: true,
                                message:
                                    'Please enter product name',
                            },
                        ]}
                    >
                        <Input placeholder="Enter product name" />
                    </Form.Item>

                    <div className="grid grid-cols-2 gap-4">
                        <Form.Item
                            label="Price"
                            name="price"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter price',
                                },
                            ]}
                        >
                            <InputNumber
                                min={0}
                                className="w-full"
                                placeholder="0.00"
                                precision={2}
                            />
                        </Form.Item>

                        <Form.Item
                            label="Stock"
                            name="stock"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter stock',
                                },
                            ]}
                        >
                            <InputNumber
                                min={0}
                                className="w-full"
                                placeholder="0"
                            />
                        </Form.Item>
                    </div>

                    <Form.Item
                        label="Category"
                        name="categoryId"
                        rules={[
                            {
                                required: true,
                                message:
                                    'Please select a category',
                            },
                        ]}
                    >
                        <TreeSelect
                            placeholder="Select a category"
                            treeData={categoryTreeOptions}
                            treeDefaultExpandAll
                            style={{ width: '100%' }}
                        />
                    </Form.Item>

                    <div className="flex justify-end gap-2 mt-6 pt-4 border-t border-[#eaeaea]">
                        <Button
                            onClick={handleCancel}
                            disabled={
                                createMutation.isPending ||
                                updateMutation.isPending
                            }
                        >
                            Cancel
                        </Button>

                        <Button
                            type="primary"
                            htmlType="submit"
                            loading={
                                createMutation.isPending ||
                                updateMutation.isPending
                            }
                        >
                            {editingProduct
                                ? 'Save Changes'
                                : 'Create Product'}
                        </Button>
                    </div>
                </Form>
            </Modal>
        </div>
    )
}

export default Products