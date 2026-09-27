import {
    Button,
    Descriptions,
    Form,
    Input,
    Space,
    Spin,
    message,
} from 'antd'
import {
    EditOutlined,
    ShopOutlined,
    EnvironmentOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import { useAuthStore } from '../../stores/authStore'

import {
    getMarketById,
    updateMarket,
} from '../../services/markets/marketService'

import type { UpdateMarketRequest } from '../../types/market'

interface MarketFormValues {
    name: string
    location: string
    email: string
    phoneNumber: string
}

function MyMarket() {
    const queryClient = useQueryClient()
    const [form] = Form.useForm<MarketFormValues>()
    const [isEditing, setIsEditing] = useState(false)

    const marketId = useAuthStore(
        (state) => state.marketId
    )

    const {
        data: market,
        isLoading,
        isError,
        error,
    } = useQuery({
        queryKey: ['my-market', marketId],
        queryFn: () => getMarketById(marketId!),
        enabled: marketId !== null,
    })

    useEffect(() => {
        if (market) {
            form.setFieldsValue({
                name: market.name ?? '',
                location: market.location ?? '',
                email: market.email ?? '',
                phoneNumber:
                    market.phoneNumber ?? '',
            })
        }
    }, [market, form])

    const updateMutation = useMutation({
        mutationFn: (
            values: UpdateMarketRequest
        ) => updateMarket(marketId!, values),

        onSuccess: async () => {
            message.success(
                'Market updated successfully.'
            )

            setIsEditing(false)

            await queryClient.invalidateQueries({
                queryKey: ['my-market', marketId],
            })

            await queryClient.invalidateQueries({
                queryKey: ['markets'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to update market.'
            )
        },
    })

    const handleSubmit = (
        values: MarketFormValues
    ) => {
        updateMutation.mutate({
            name: values.name,
            location: values.location,
            email: values.email,
            phoneNumber: values.phoneNumber,
        })
    }

    const handleCancel = () => {
        if (market) {
            form.setFieldsValue({
                name: market.name ?? '',
                location: market.location ?? '',
                email: market.email ?? '',
                phoneNumber:
                    market.phoneNumber ?? '',
            })
        }

        setIsEditing(false)
    }

    if (marketId === null) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    My Market
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        You are not assigned to any
                        market.
                    </p>
                </div>
            </div>
        )
    }

    if (isLoading) {
        return (
            <div className="flex justify-center items-center py-20">
                <Spin size="large" />
            </div>
        )
    }

    if (isError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    My Market
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        {error instanceof Error
                            ? error.message
                            : 'Failed to load market.'}
                    </p>
                </div>
            </div>
        )
    }

    return (
        <div>
            {/* Page header */}
            <div className="mb-8">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    My Market
                </h1>

                <p className="text-[#6b7280] mt-1">
                    {isEditing
                        ? 'Update your market information.'
                        : 'View and manage your market information.'}
                </p>
            </div>

            <div className="surface-card max-w-3xl overflow-hidden">
                {/* Identity banner */}
                <div className="flex items-center gap-4 px-6 py-5 border-b border-[#eaeaea] bg-[#fafafa]">
                    <div className="flex items-center justify-center w-14 h-14 rounded-2xl bg-[#4f46e5] text-white text-lg font-semibold shadow-lg shadow-indigo-500/20">
                        <ShopOutlined
                            style={{ fontSize: 22 }}
                        />
                    </div>

                    <div className="min-w-0">
                        <div className="text-base font-semibold text-[#0a0a0a] truncate">
                            {market?.name ?? '—'}
                        </div>

                        <div className="flex items-center gap-1.5 text-xs text-[#6b7280] truncate mt-0.5">
                            <EnvironmentOutlined
                                style={{ fontSize: 11 }}
                            />
                            <span className="truncate">
                                {market?.location ?? '—'}
                            </span>
                        </div>
                    </div>
                </div>

                {/* Body */}
                <div className="p-6">
                    {!isEditing ? (
                        <>
                            <Descriptions
                                column={1}
                                colon={false}
                                labelStyle={{
                                    color: '#6b7280',
                                    width: 160,
                                }}
                                contentStyle={{
                                    color: '#0a0a0a',
                                    fontWeight: 500,
                                }}
                            >
                                <Descriptions.Item label="Market Name">
                                    {market?.name ??
                                        '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Location">
                                    {market?.location ??
                                        '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Email">
                                    {market?.email ?? '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Phone">
                                    {market?.phoneNumber ??
                                        '—'}
                                </Descriptions.Item>
                            </Descriptions>

                            <div className="mt-6 pt-6 border-t border-[#eaeaea]">
                                <Button
                                    type="primary"
                                    icon={
                                        <EditOutlined />
                                    }
                                    onClick={() =>
                                        setIsEditing(
                                            true
                                        )
                                    }
                                >
                                    Edit Market
                                </Button>
                            </div>
                        </>
                    ) : (
                        <Form
                            form={form}
                            layout="vertical"
                            onFinish={handleSubmit}
                            requiredMark={false}
                        >
                            <Form.Item
                                label="Market Name"
                                name="name"
                                rules={[
                                    {
                                        required: true,
                                        message:
                                            'Market name is required',
                                    },
                                ]}
                            >
                                <Input placeholder="Enter market name" />
                            </Form.Item>

                            <Form.Item
                                label="Location"
                                name="location"
                                rules={[
                                    {
                                        required: true,
                                        message:
                                            'Location is required',
                                    },
                                ]}
                            >
                                <Input placeholder="Enter location" />
                            </Form.Item>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-x-4">
                                <Form.Item
                                    label="Email"
                                    name="email"
                                    rules={[
                                        {
                                            required:
                                                true,
                                            type: 'email',
                                            message:
                                                'Please enter a valid email',
                                        },
                                    ]}
                                >
                                    <Input placeholder="Enter email" />
                                </Form.Item>

                                <Form.Item
                                    label="Phone Number"
                                    name="phoneNumber"
                                    rules={[
                                        {
                                            required:
                                                true,
                                            message:
                                                'Phone number is required',
                                        },
                                    ]}
                                >
                                    <Input placeholder="Enter phone number" />
                                </Form.Item>
                            </div>

                            <div className="mt-2 pt-6 border-t border-[#eaeaea]">
                                <Space>
                                    <Button
                                        onClick={
                                            handleCancel
                                        }
                                        disabled={
                                            updateMutation.isPending
                                        }
                                    >
                                        Cancel
                                    </Button>

                                    <Button
                                        type="primary"
                                        htmlType="submit"
                                        loading={
                                            updateMutation.isPending
                                        }
                                    >
                                        Save Changes
                                    </Button>
                                </Space>
                            </div>
                        </Form>
                    )}
                </div>
            </div>
        </div>
    )
}

export default MyMarket