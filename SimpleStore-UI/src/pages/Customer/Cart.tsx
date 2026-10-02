import {
    Button,
    InputNumber,
    Popconfirm,
    Spin,
    Tooltip,
    message,
} from 'antd'
import {
    DeleteOutlined,
    MinusOutlined,
    PlusOutlined,
    ShoppingCartOutlined,
    ShopOutlined,
    ShoppingOutlined,
    ArrowRightOutlined,
    WarningOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import {
    getMyCart,
    removeCartItem,
    updateCartItem,
} from '../../services/cart/cartService'

import { getAllProducts } from '../../services/products/productService'
import { getAllMarkets } from '../../services/markets/marketService'
import { createOrder } from '../../services/orderService'
import { getCurrentCustomer } from '../../services/customers/customerService'

import { ApiError } from '../../services/api/apiClient'

import {
    isProfileComplete,
    getMissingProfileFields,
} from '../../utils/profile'

import type { Product } from '../../types/product'
import type { Market } from '../../types/market'

function Cart() {
    const queryClient = useQueryClient()
    const navigate = useNavigate()

    const {
        data: cart,
        isLoading: cartLoading,
        isError: cartError,
        error: cartErrorMessage,
    } = useQuery({
        queryKey: ['my-cart'],
        queryFn: getMyCart,
        retry: false,
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
        queryKey: ['markets'],
        queryFn: getAllMarkets,
    })

    const {
        data: currentCustomer,
        isLoading: customerLoading,
    } = useQuery({
        queryKey: ['current-customer'],
        queryFn: getCurrentCustomer,
    })

    const updateMutation = useMutation({
        mutationFn: ({
                         productId,
                         quantity,
                     }: {
            productId: number
            quantity: number
        }) =>
            updateCartItem(productId, {
                quantity,
            }),

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ['my-cart'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to update cart item.'
            )
        },
    })

    const removeMutation = useMutation({
        mutationFn: (productId: number) =>
            removeCartItem(productId),

        onSuccess: () => {
            message.success(
                'Item removed from cart.'
            )

            queryClient.invalidateQueries({
                queryKey: ['my-cart'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to remove cart item.'
            )
        },
    })

    const createOrderMutation = useMutation({
        mutationFn: createOrder,

        onSuccess: (order) => {
            message.success(
                `Order #${order.id} created successfully.`
            )

            queryClient.removeQueries({
                queryKey: ['my-cart'],
            })

            queryClient.invalidateQueries({
                queryKey: ['customer-order-history'],
            })

            navigate('/customer')
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to create order.'
            )
        },
    })

    if (
        cartLoading ||
        productsLoading ||
        marketsLoading ||
        customerLoading
    ) {
        return (
            <div className="flex justify-center items-center py-20">
                <Spin size="large" />
            </div>
        )
    }

    const cartDoesNotExist =
        cartError &&
        cartErrorMessage instanceof ApiError &&
        cartErrorMessage.status === 404

    if (cartError && !cartDoesNotExist) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Cart
                </h1>

                <p className="text-[#6b7280]">
                    Failed to load cart:{' '}
                    {cartErrorMessage instanceof Error
                        ? cartErrorMessage.message
                        : 'Unknown error'}
                </p>
            </div>
        )
    }

    if (productsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Cart
                </h1>

                <p className="text-[#6b7280]">
                    Failed to load products.
                </p>
            </div>
        )
    }

    if (marketsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Cart
                </h1>

                <p className="text-[#6b7280]">
                    Failed to load markets.
                </p>
            </div>
        )
    }

    if (
        cartDoesNotExist ||
        !cart ||
        cart.items.length === 0
    ) {
        return (
            <div>
                <div className="mb-6">
                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Cart
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        Review your items before
                        checkout.
                    </p>
                </div>

                <div className="surface-card p-12 text-center">
                    <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-4">
                        <ShoppingCartOutlined
                            style={{
                                fontSize: 28,
                            }}
                        />
                    </div>

                    <h2 className="text-lg font-semibold text-[#0a0a0a] mb-2">
                        Your cart is empty
                    </h2>

                    <p className="text-[#6b7280] text-sm mb-6">
                        Browse products and start
                        adding items to your cart.
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
            </div>
        )
    }

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

    const handleQuantityChange = (
        productId: number,
        quantity: number | null
    ) => {
        if (quantity === null || quantity < 1) {
            return
        }

        updateMutation.mutate({
            productId,
            quantity,
        })
    }

    const handleIncrease = (
        productId: number,
        currentQuantity: number
    ) => {
        const product = productMap.get(productId)

        if (!product) {
            message.error(
                'Product information could not be found.'
            )
            return
        }

        if (currentQuantity >= product.stock) {
            message.warning(
                `Only ${product.stock} item(s) are available.`
            )
            return
        }

        updateMutation.mutate({
            productId,
            quantity: currentQuantity + 1,
        })
    }

    const handleDecrease = (
        productId: number,
        currentQuantity: number
    ) => {
        if (currentQuantity <= 1) {
            return
        }

        updateMutation.mutate({
            productId,
            quantity: currentQuantity - 1,
        })
    }

    const handleRemove = (productId: number) => {
        removeMutation.mutate(productId)
    }

    const handleCreateOrder = () => {
        if (!profileComplete) {
            message.warning(
                'Please complete your profile before placing an order.'
            )
            navigate('/customer/profile')
            return
        }

        createOrderMutation.mutate()
    }

    const isBusy =
        updateMutation.isPending ||
        removeMutation.isPending ||
        createOrderMutation.isPending

    const itemCount = cart.items.reduce(
        (sum, item) => sum + item.quantity,
        0
    )

    const profileComplete =
        isProfileComplete(currentCustomer)

    const missingFields =
        getMissingProfileFields(currentCustomer)

    return (
        <div>
            <div className="mb-6">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Cart
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Review your items before
                    checkout.
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
                                Complete your profile to
                                place an order
                            </div>

                            <div className="text-xs text-[#6b7280] mt-1">
                                Missing:{' '}
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

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2">
                    <div className="surface-card p-5 mb-4">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                                <ShoppingOutlined
                                    style={{
                                        color: '#6b7280',
                                    }}
                                />

                                <span className="text-sm font-medium text-[#0a0a0a]">
                                    Your items
                                </span>
                            </div>

                            <span className="text-xs text-[#9ca3af]">
                                {itemCount}{' '}
                                {itemCount === 1
                                    ? 'item'
                                    : 'items'}
                            </span>
                        </div>
                    </div>

                    <div className="space-y-4">
                        {cart.items.map((item) => {
                            const product =
                                productMap.get(
                                    item.productId
                                )

                            const market =
                                marketMap.get(
                                    item.marketId
                                )

                            const maxStock =
                                product?.stock ??
                                item.quantity

                            const lineTotal =
                                item.quantity *
                                item.price

                            return (
                                <div
                                    key={item.productId}
                                    className="surface-card p-4 flex gap-4"
                                >
                                    <div className="flex-shrink-0 w-24 h-24 rounded-xl bg-gradient-to-br from-[#f4f4f5] to-[#eaeaea] flex items-center justify-center">
                                        <ShoppingOutlined
                                            style={{
                                                fontSize: 24,
                                                color: '#d4d4d8',
                                            }}
                                        />
                                    </div>

                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-start justify-between gap-4 mb-1">
                                            <div className="min-w-0">
                                                <h3 className="font-semibold text-[#0a0a0a] text-base leading-snug truncate">
                                                    {product?.name ??
                                                        `Product #${item.productId}`}
                                                </h3>

                                                <div className="flex items-center gap-1.5 text-xs text-[#6b7280] mt-0.5">
                                                    <ShopOutlined
                                                        style={{
                                                            fontSize: 11,
                                                        }}
                                                    />

                                                    <span className="truncate">
                                                        {market?.name ??
                                                            `Market #${item.marketId}`}
                                                    </span>
                                                </div>
                                            </div>

                                            <div className="text-right flex-shrink-0">
                                                <div className="font-semibold text-[#0a0a0a] text-base tabular-nums">
                                                    $
                                                    {lineTotal.toFixed(
                                                        2
                                                    )}
                                                </div>

                                                <div className="text-xs text-[#6b7280] tabular-nums">
                                                    $
                                                    {item.price.toFixed(
                                                        2
                                                    )}{' '}
                                                    ea.
                                                </div>
                                            </div>
                                        </div>

                                        <div className="flex items-center justify-between mt-4">
                                            <div className="flex items-center gap-1">
                                                <Button
                                                    size="small"
                                                    type="text"
                                                    icon={
                                                        <MinusOutlined />
                                                    }
                                                    disabled={
                                                        item.quantity <=
                                                        1 ||
                                                        isBusy
                                                    }
                                                    onClick={() =>
                                                        handleDecrease(
                                                            item.productId,
                                                            item.quantity
                                                        )
                                                    }
                                                />

                                                <InputNumber
                                                    min={1}
                                                    max={
                                                        maxStock
                                                    }
                                                    value={
                                                        item.quantity
                                                    }
                                                    disabled={
                                                        isBusy
                                                    }
                                                    onChange={(
                                                        value
                                                    ) =>
                                                        handleQuantityChange(
                                                            item.productId,
                                                            value
                                                        )
                                                    }
                                                    controls={
                                                        false
                                                    }
                                                    style={{
                                                        width: 56,
                                                        textAlign:
                                                            'center',
                                                    }}
                                                />

                                                <Button
                                                    size="small"
                                                    type="text"
                                                    icon={
                                                        <PlusOutlined />
                                                    }
                                                    disabled={
                                                        item.quantity >=
                                                        maxStock ||
                                                        isBusy
                                                    }
                                                    onClick={() =>
                                                        handleIncrease(
                                                            item.productId,
                                                            item.quantity
                                                        )
                                                    }
                                                />
                                            </div>

                                            <Popconfirm
                                                title="Remove this item?"
                                                description="It will be removed from your cart."
                                                okText="Remove"
                                                cancelText="Cancel"
                                                okButtonProps={{
                                                    danger: true,
                                                }}
                                                onConfirm={() =>
                                                    handleRemove(
                                                        item.productId
                                                    )
                                                }
                                            >
                                                <Button
                                                    type="text"
                                                    danger
                                                    size="small"
                                                    icon={
                                                        <DeleteOutlined />
                                                    }
                                                    disabled={
                                                        isBusy
                                                    }
                                                >
                                                    Remove
                                                </Button>
                                            </Popconfirm>
                                        </div>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </div>

                <div className="lg:col-span-1">
                    <div className="surface-card p-6 lg:sticky lg:top-6">
                        <h2 className="text-lg font-semibold text-[#0a0a0a] mb-5">
                            Order summary
                        </h2>

                        <div className="space-y-3 pb-5 border-b border-[#eaeaea]">
                            <div className="flex justify-between text-sm">
                                <span className="text-[#6b7280]">
                                    Items
                                </span>

                                <span className="text-[#0a0a0a] tabular-nums">
                                    {itemCount}
                                </span>
                            </div>

                            <div className="flex justify-between text-sm">
                                <span className="text-[#6b7280]">
                                    Subtotal
                                </span>

                                <span className="text-[#0a0a0a] tabular-nums">
                                    $
                                    {cart.totalPrice.toFixed(
                                        2
                                    )}
                                </span>
                            </div>
                        </div>

                        <div className="flex justify-between items-baseline pt-5 pb-6">
                            <span className="text-sm text-[#6b7280]">
                                Total
                            </span>

                            <span className="text-2xl font-semibold text-[#0a0a0a] tabular-nums tracking-tight">
                                $
                                {cart.totalPrice.toFixed(
                                    2
                                )}
                            </span>
                        </div>

                        <Tooltip
                            title={
                                profileComplete
                                    ? ''
                                    : 'Complete your profile to place an order'
                            }
                        >
                            <div>
                                <Button
                                    type="primary"
                                    size="large"
                                    block
                                    loading={
                                        createOrderMutation.isPending
                                    }
                                    disabled={
                                        isBusy ||
                                        !profileComplete
                                    }
                                    onClick={
                                        handleCreateOrder
                                    }
                                    icon={
                                        <ArrowRightOutlined />
                                    }
                                    iconPlacement="end"
                                >
                                    Create Order
                                </Button>
                            </div>
                        </Tooltip>

                        <p className="text-xs text-[#9ca3af] text-center mt-4">
                            You'll be able to pick up
                            your order at the market.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default Cart