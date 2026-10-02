import {
    Button,
    Col,
    Empty,
    InputNumber,
    Rate,
    Row,
    Spin,
    Switch,
    TreeSelect,
    message,
} from 'antd'
import {
    ShopOutlined,
    ShoppingCartOutlined,
    AppstoreOutlined,
    FilterOutlined,
} from '@ant-design/icons'
import {
    useQuery,
    useQueries,
    useMutation,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { getAllProducts } from '../../services/products/productService'
import { getAllCategories } from '../../services/categories/categoryService'
import { getReviewsByProduct } from '../../services/reviews/reviewService'

import {
    addItemsToCart,
    createCart,
    getMyCart,
} from '../../services/cart/cartService'

import { getAllMarkets } from '../../services/markets/marketService'

import { ApiError } from '../../services/api/apiClient'

import type { Product } from '../../types/product'
import type { Market } from '../../types/market'
import type { Category } from '../../types/category'
import type { ReviewsByProductSummary } from '../../types/review'
import type { Cart } from '../../services/cart/cartService'

interface ProductFilters {
    categoryId?: number
    minPrice?: number
    maxPrice?: number
    onlyAvailable: boolean
}

function Products() {
    const [quantities, setQuantities] = useState<
        Record<number, number>
    >({})

    const [filters, setFilters] = useState<ProductFilters>(
        {
            onlyAvailable: false,
        }
    )

    const queryClient = useQueryClient()

    const queryParams = useMemo(() => {
        const params: {
            categoryIds?: number[]
            minPrice?: number
            maxPrice?: number
            isAvailable?: boolean
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

        return params
    }, [filters])

    const {
        data,
        isLoading,
        isError,
        error,
    } = useQuery({
        queryKey: [
            'customer-products',
            queryParams,
        ],
        queryFn: () => getAllProducts(queryParams),
    })

    const {
        data: marketsData,
        isLoading: marketsLoading,
        isError: marketsError,
    } = useQuery({
        queryKey: ['customer-markets'],
        queryFn: getAllMarkets,
    })

    const {
        data: categoriesData,
        isLoading: categoriesLoading,
    } = useQuery({
        queryKey: ['categories'],
        queryFn: getAllCategories,
    })

    const products: Product[] = data?.items ?? []

    // Batch-fetch review summaries for all visible products.
    // Each entry has its own cache key, so Products re-renders cheaply.
    const reviewQueries = useQueries({
        queries: products.map((product) => ({
            queryKey: [
                'reviews-by-product',
                product.id,
            ],
            queryFn: () =>
                getReviewsByProduct(product.id),
            staleTime: 60_000,
        })),
    })

    const reviewSummaryByProductId = useMemo(() => {
        const map = new Map<
            number,
            ReviewsByProductSummary
        >()

        products.forEach((product, index) => {
            const query = reviewQueries[index]

            if (query?.data) {
                map.set(product.id, query.data)
            }
        })

        return map
    }, [products, reviewQueries])

    const addToCartMutation = useMutation({
        mutationFn: async ({
                               productId,
                               quantity,
                           }: {
            productId: number
            quantity: number
        }) => {
            let cart: Cart | null = null

            try {
                cart = await getMyCart()
            } catch (error) {
                if (
                    error instanceof ApiError &&
                    error.status === 404
                ) {
                    cart = await createCart()
                } else {
                    throw error
                }
            }

            const cartQuantity =
                cart.items?.find(
                    (item) =>
                        item.productId === productId
                )?.quantity ?? 0

            const product = products.find(
                (item) => item.id === productId
            )

            if (!product) {
                throw new Error(
                    'Product not found.'
                )
            }

            const remainingStock =
                product.stock - cartQuantity

            if (remainingStock <= 0) {
                throw new Error(
                    'There are no more available items to add.'
                )
            }

            if (quantity > remainingStock) {
                throw new Error(
                    `Only ${remainingStock} more item(s) are available.`
                )
            }

            return addItemsToCart({
                cartItems: [
                    {
                        productId,
                        quantity,
                    },
                ],
            })
        },

        onSuccess: () => {
            message.success(
                'Added to cart'
            )

            queryClient.invalidateQueries({
                queryKey: ['my-cart'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to add product to cart.'
            )
        },
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
        const map = new Map<number, Category>()

        const categories =
            categoriesData?.items ?? []

        categories.forEach((category: Category) => {
            map.set(category.id, category)
        })

        return map
    }, [categoriesData])

    if (
        isLoading ||
        marketsLoading ||
        categoriesLoading
    ) {
        return (
            <div className="flex justify-center items-center py-20">
                <Spin size="large" />
            </div>
        )
    }

    if (isError) {
        return (
            <div>
                <h1 className="text-2xl font-bold mb-4">
                    Products
                </h1>

                <p>
                    Failed to load products:{' '}
                    {error instanceof Error
                        ? error.message
                        : 'Unknown error'}
                </p>
            </div>
        )
    }

    if (marketsError) {
        return (
            <div>
                <h1 className="text-2xl font-bold mb-4">
                    Products
                </h1>

                <p>Failed to load markets.</p>
            </div>
        )
    }

    const markets: Market[] =
        marketsData?.items ?? []

    const marketMap = new Map(
        markets.map((market) => [
            market.id,
            market,
        ])
    )

    const groupedProducts = new Map<
        string,
        Product[]
    >()

    products.forEach((product) => {
        const category = categoryMap.get(
            product.categoryId
        )

        const groupName =
            category?.name ?? 'Other'

        if (!groupedProducts.has(groupName)) {
            groupedProducts.set(groupName, [])
        }

        groupedProducts.get(groupName)!.push(product)
    })

    const sortedGroupNames = Array.from(
        groupedProducts.keys()
    ).sort((a, b) => a.localeCompare(b))

    const handleQuantityChange = (
        productId: number,
        quantity: number | null
    ) => {
        setQuantities((previous) => ({
            ...previous,
            [productId]: quantity ?? 1,
        }))
    }

    const handleAddToCart = (
        product: Product
    ) => {
        const selectedQuantity =
            quantities[product.id] ?? 1

        if (selectedQuantity <= 0) {
            message.warning(
                'Quantity must be greater than 0.'
            )
            return
        }

        addToCartMutation.mutate({
            productId: product.id,
            quantity: selectedQuantity,
        })
    }

    const clearFilters = () => {
        setFilters({
            onlyAvailable: false,
        })
    }

    return (
        <div>
            {/* Page header */}
            <div className="mb-6">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Products
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Browse products from all markets.
                </p>
            </div>

            {/* Filters card */}
            <div className="surface-card p-5 mb-8">
                <div className="flex items-center gap-2 mb-4">
                    <FilterOutlined
                        style={{
                            color: '#6b7280',
                        }}
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

            {/* Product grid */}
            {products.length === 0 ? (
                <div className="surface-card p-12 text-center">
                    <Empty
                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                        description={
                            <span className="text-[#6b7280]">
                                No products match your
                                filters.
                            </span>
                        }
                    >
                        <Button onClick={clearFilters}>
                            Clear Filters
                        </Button>
                    </Empty>
                </div>
            ) : (
                <div className="space-y-10">
                    {sortedGroupNames.map(
                        (groupName) => {
                            const groupProducts =
                                groupedProducts.get(
                                    groupName
                                ) ?? []

                            return (
                                <div
                                    key={groupName}
                                >
                                    {/* Group header */}
                                    <div className="flex items-center gap-3 mb-4">
                                        <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-[#eef2ff] text-[#4f46e5]">
                                            <AppstoreOutlined
                                                style={{
                                                    fontSize: 14,
                                                }}
                                            />
                                        </div>

                                        <h2 className="text-lg font-semibold text-[#0a0a0a]">
                                            {groupName}
                                        </h2>

                                        <span className="text-xs text-[#9ca3af]">
                                            {
                                                groupProducts.length
                                            }{' '}
                                            {groupProducts.length ===
                                            1
                                                ? 'item'
                                                : 'items'}
                                        </span>
                                    </div>

                                    {/* Cards */}
                                    <Row gutter={[16, 16]}>
                                        {groupProducts.map(
                                            (product) => {
                                                const selectedQuantity =
                                                    quantities[
                                                        product
                                                            .id
                                                        ] ?? 1

                                                const market =
                                                    marketMap.get(
                                                        product.marketId
                                                    )

                                                const isOutOfStock =
                                                    product.stock ===
                                                    0

                                                const reviewSummary =
                                                    reviewSummaryByProductId.get(
                                                        product.id
                                                    )

                                                const reviewCount =
                                                    reviewSummary
                                                        ?.reviewCount ??
                                                    0

                                                const averageRating =
                                                    reviewSummary
                                                        ?.averageRating ??
                                                    0

                                                return (
                                                    <Col
                                                        key={
                                                            product.id
                                                        }
                                                        xs={24}
                                                        sm={12}
                                                        lg={8}
                                                        xl={6}
                                                    >
                                                        <div
                                                            className={`surface-card h-full flex flex-col overflow-hidden transition-all hover:shadow-md ${
                                                                isOutOfStock
                                                                    ? 'opacity-70'
                                                                    : ''
                                                            }`}
                                                        >
                                                            {/* Image placeholder */}
                                                            <div className="aspect-[4/3] bg-gradient-to-br from-[#f4f4f5] to-[#eaeaea] flex items-center justify-center">
                                                                <ShoppingCartOutlined
                                                                    style={{
                                                                        fontSize: 32,
                                                                        color: '#d4d4d8',
                                                                    }}
                                                                />
                                                            </div>

                                                            {/* Body */}
                                                            <div className="flex-1 p-5 flex flex-col">
                                                                <div className="flex-1">
                                                                    <h3 className="font-semibold text-[#0a0a0a] text-base leading-snug mb-1">
                                                                        {
                                                                            product.name
                                                                        }
                                                                    </h3>

                                                                    <div className="flex items-center gap-1.5 text-xs text-[#6b7280] mb-2">
                                                                        <ShopOutlined
                                                                            style={{
                                                                                fontSize: 11,
                                                                            }}
                                                                        />

                                                                        <span className="truncate">
                                                                            {market?.name ??
                                                                                `Market #${product.marketId}`}
                                                                        </span>
                                                                    </div>

                                                                    {/* Rating row */}
                                                                    <div className="flex items-center gap-2 mb-3 min-h-[20px]">
                                                                        {reviewCount >
                                                                        0 ? (
                                                                            <>
                                                                                <Rate
                                                                                    disabled
                                                                                    allowHalf
                                                                                    value={
                                                                                        averageRating
                                                                                    }
                                                                                    style={{
                                                                                        fontSize: 14,
                                                                                    }}
                                                                                />

                                                                                <span className="text-xs text-[#6b7280]">
                                                                                    {averageRating.toFixed(
                                                                                        1
                                                                                    )}{' '}
                                                                                    (
                                                                                    {
                                                                                        reviewCount
                                                                                    }
                                                                                    )
                                                                                </span>
                                                                            </>
                                                                        ) : (
                                                                            <span className="text-xs text-[#9ca3af]">
                                                                                No
                                                                                reviews
                                                                                yet
                                                                            </span>
                                                                        )}
                                                                    </div>

                                                                    <div className="flex items-baseline justify-between mb-2">
                                                                        <span className="text-2xl font-semibold text-[#0a0a0a] tracking-tight">
                                                                            $
                                                                            {product.price.toFixed(
                                                                                2
                                                                            )}
                                                                        </span>
                                                                    </div>

                                                                    {/* Stock pill */}
                                                                    <div className="mb-4">
                                                                        {isOutOfStock ? (
                                                                            <span className="inline-flex items-center gap-1.5 text-xs font-medium text-[#6b7280] bg-[#f4f4f5] px-2.5 py-1 rounded-full">
                                                                                <span className="w-1.5 h-1.5 rounded-full bg-[#9ca3af]" />
                                                                                Out
                                                                                of
                                                                                stock
                                                                            </span>
                                                                        ) : product.stock <=
                                                                        5 ? (
                                                                            <span className="inline-flex items-center gap-1.5 text-xs font-medium text-[#d97706] bg-[#fffbeb] px-2.5 py-1 rounded-full">
                                                                                <span className="w-1.5 h-1.5 rounded-full bg-[#d97706]" />
                                                                                Only{' '}
                                                                                {
                                                                                    product.stock
                                                                                }{' '}
                                                                                left
                                                                            </span>
                                                                        ) : (
                                                                            <span className="inline-flex items-center gap-1.5 text-xs font-medium text-[#059669] bg-[#ecfdf5] px-2.5 py-1 rounded-full">
                                                                                <span className="w-1.5 h-1.5 rounded-full bg-[#059669]" />
                                                                                In
                                                                                stock:{' '}
                                                                                {
                                                                                    product.stock
                                                                                }
                                                                            </span>
                                                                        )}
                                                                    </div>
                                                                </div>

                                                                {/* Actions */}
                                                                <div className="flex gap-2 mt-auto pt-4 border-t border-[#f0f0f0]">
                                                                    <InputNumber
                                                                        min={1}
                                                                        max={
                                                                            product.stock
                                                                        }
                                                                        value={
                                                                            selectedQuantity >
                                                                            product.stock
                                                                                ? product.stock
                                                                                : selectedQuantity
                                                                        }
                                                                        onChange={(
                                                                            value
                                                                        ) =>
                                                                            handleQuantityChange(
                                                                                product.id,
                                                                                value
                                                                            )
                                                                        }
                                                                        disabled={
                                                                            isOutOfStock ||
                                                                            addToCartMutation.isPending
                                                                        }
                                                                        style={{
                                                                            width: 70,
                                                                        }}
                                                                    />

                                                                    <Button
                                                                        type="primary"
                                                                        icon={
                                                                            <ShoppingCartOutlined />
                                                                        }
                                                                        disabled={
                                                                            isOutOfStock
                                                                        }
                                                                        loading={
                                                                            addToCartMutation.isPending
                                                                        }
                                                                        onClick={() =>
                                                                            handleAddToCart(
                                                                                product
                                                                            )
                                                                        }
                                                                        className="flex-1"
                                                                    >
                                                                        {isOutOfStock
                                                                            ? 'Out of stock'
                                                                            : 'Add'}
                                                                    </Button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </Col>
                                                )
                                            }
                                        )}
                                    </Row>
                                </div>
                            )
                        }
                    )}
                </div>
            )}
        </div>
    )
}

export default Products
