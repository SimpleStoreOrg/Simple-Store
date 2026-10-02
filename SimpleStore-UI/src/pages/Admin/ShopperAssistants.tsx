import {
    Button,
    Form,
    Input,
    Modal,
    Popconfirm,
    Select,
    Table,
} from 'antd'
import {
    PlusOutlined,
    SearchOutlined,
    ReloadOutlined,
    EditOutlined,
    DeleteOutlined,
    TeamOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { useAuthStore } from '../../stores/authStore'

import {
    createShopperAssistant,
    deleteShopperAssistant,
    getAllShopperAssistants,
    updateShopperAssistant,
} from '../../services/shopperAssistants/shopperAssistantService'

import { getAllMarkets } from '../../services/markets/marketService'

import type {
    CreateShopperAssistantRequest,
    ShopperAssistant,
    ShopperAssistantPosition,
    UpdateShopperAssistantRequest,
} from '../../types/shopperAssistant'

import type { Market } from '../../types/market'

interface ShopperAssistantFormValues {
    name: string
    surname: string
    email: string
    username: string
    password?: string
    position: ShopperAssistantPosition
    phoneNumber: string
}

const positionOptions = [
    { label: 'Cashier', value: 0 },
    { label: 'Collector', value: 1 },
    { label: 'Packer', value: 2 },
]

const positionStyle: Record<
    ShopperAssistantPosition,
    { bg: string; text: string; dot: string }
> = {
    0: {
        bg: '#eef2ff',
        text: '#4f46e5',
        dot: '#4f46e5',
    },
    1: {
        bg: '#fffbeb',
        text: '#d97706',
        dot: '#d97706',
    },
    2: {
        bg: '#ecfdf5',
        text: '#059669',
        dot: '#059669',
    },
}

function ShopperAssistants() {
    const role = useAuthStore((state) => state.role)

    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const isAdmin = role === 'Admin'

    const isMarketAdmin =
        isAdmin &&
        adminPosition === 'MarketAdmin'

    const [isModalOpen, setIsModalOpen] =
        useState(false)

    const [
        editingShopperAssistant,
        setEditingShopperAssistant,
    ] = useState<ShopperAssistant | null>(null)

    const [selectedPosition, setSelectedPosition] =
        useState<
            ShopperAssistantPosition | undefined
        >(undefined)

    const [search, setSearch] = useState('')

    const [form] = Form.useForm()

    const queryClient = useQueryClient()

    const {
        data,
        isLoading,
        isError,
        error,
        isFetching,
        refetch,
    } = useQuery({
        queryKey: [
            'shopperAssistants',
            selectedPosition,
        ],
        queryFn: () =>
            getAllShopperAssistants(
                1,
                100,
                selectedPosition
            ),
        enabled: isAdmin,
    })

    const {
        data: marketsData,
        isLoading: marketsLoading,
    } = useQuery({
        queryKey: ['markets'],
        queryFn: getAllMarkets,
        enabled: isAdmin,
    })

    const marketMap = useMemo(() => {
        const map = new Map<number, string>()

        const markets = marketsData?.items ?? []

        markets.forEach((market: Market) => {
            map.set(market.id, market.name)
        })

        return map
    }, [marketsData])

    const getMarketName = (marketId: number) => {
        return (
            marketMap.get(marketId) ??
            `Market #${marketId}`
        )
    }

    const createMutation = useMutation({
        mutationFn: createShopperAssistant,

        onSuccess: () => {
            setIsModalOpen(false)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['shopperAssistants'],
            })
        },

        onError: (error) => {
            console.error(
                'CREATE SHOPPER ASSISTANT FAILED:',
                error
            )
        },
    })

    const updateMutation = useMutation({
        mutationFn: ({
                         id,
                         data,
                     }: {
            id: number
            data: UpdateShopperAssistantRequest
        }) =>
            updateShopperAssistant(
                id,
                data
            ),

        onSuccess: () => {
            setIsModalOpen(false)
            setEditingShopperAssistant(null)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['shopperAssistants'],
            })
        },

        onError: (error) => {
            console.error(
                'UPDATE SHOPPER ASSISTANT FAILED:',
                error
            )
        },
    })

    const deleteMutation = useMutation({
        mutationFn: deleteShopperAssistant,

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ['shopperAssistants'],
            })
        },

        onError: (error) => {
            console.error(
                'DELETE SHOPPER ASSISTANT FAILED:',
                error
            )
        },
    })

    const handleAdd = () => {
        setEditingShopperAssistant(null)
        form.resetFields()
        setIsModalOpen(true)
    }

    const handleEdit = (
        shopperAssistant: ShopperAssistant
    ) => {
        setEditingShopperAssistant(
            shopperAssistant
        )

        form.setFieldsValue({
            name: shopperAssistant.name,
            surname: shopperAssistant.surname,
            email: shopperAssistant.email,
            username: shopperAssistant.username,
            position: shopperAssistant.position,
            phoneNumber:
            shopperAssistant.phoneNumber,
        })

        setIsModalOpen(true)
    }

    const handleCancel = () => {
        setIsModalOpen(false)
        setEditingShopperAssistant(null)
        form.resetFields()
    }

    const handleSubmit = (
        values: ShopperAssistantFormValues
    ) => {
        if (editingShopperAssistant) {
            const updateData: UpdateShopperAssistantRequest =
                {
                    name: values.name,
                    surname: values.surname,
                    email: values.email,
                    username: values.username,
                    position: values.position,
                    phoneNumber:
                    values.phoneNumber,
                }

            updateMutation.mutate({
                id: editingShopperAssistant.id,
                data: updateData,
            })

            return
        }

        const createData: CreateShopperAssistantRequest =
            {
                name: values.name,
                surname: values.surname,
                email: values.email,
                username: values.username,
                password: values.password!,
                position: values.position,
                phoneNumber: values.phoneNumber,
            }

        createMutation.mutate(createData)
    }

    if (!isAdmin) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Access Denied
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Only admins can access this
                        page.
                    </p>
                </div>
            </div>
        )
    }

    if (isError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Shopper Assistants
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Error:{' '}
                        {error instanceof Error
                            ? error.message
                            : 'Unknown error'}
                    </p>
                </div>
            </div>
        )
    }

    const shopperAssistants: ShopperAssistant[] =
        data?.items ?? []

    const filteredAssistants = useMemo(() => {
        if (!search.trim()) {
            return shopperAssistants
        }

        const q = search.trim().toLowerCase()

        return shopperAssistants.filter((assistant) => {
            const name = (assistant.name ?? '').toLowerCase()
            const surname = (assistant.surname ?? '').toLowerCase()
            const username = (assistant.username ?? '').toLowerCase()
            const email = (assistant.email ?? '').toLowerCase()

            return (
                name.includes(q) ||
                surname.includes(q) ||
                username.includes(q) ||
                email.includes(q)
            )
        })
    }, [shopperAssistants, search])

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
            title: 'Surname',
            dataIndex: 'surname',
            render: (surname: string) => (
                <span className="text-[#0a0a0a]">
                    {surname}
                </span>
            ),
        },
        {
            title: 'Username',
            dataIndex: 'username',
            render: (username: string) => (
                <span className="text-[#6b7280]">
                    {username}
                </span>
            ),
        },
        {
            title: 'Email',
            dataIndex: 'email',
            render: (email: string) => (
                <span className="text-[#6b7280]">
                    {email}
                </span>
            ),
        },
        {
            title: 'Phone',
            dataIndex: 'phoneNumber',
            render: (phone: string) => (
                <span className="text-[#6b7280] tabular-nums">
                    {phone ?? '—'}
                </span>
            ),
        },
        {
            title: 'Position',
            render: (
                _: unknown,
                assistant: ShopperAssistant
            ) => {
                const style =
                    positionStyle[assistant.position]

                const label =
                    assistant.position === 0
                        ? 'Cashier'
                        : assistant.position === 1
                            ? 'Collector'
                            : 'Packer'

                return (
                    <span
                        className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium"
                        style={{
                            background: style.bg,
                            color: style.text,
                        }}
                    >
                        <span
                            className="w-1.5 h-1.5 rounded-full"
                            style={{
                                background: style.dot,
                            }}
                        />
                        {label}
                    </span>
                )
            },
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
                        assistant: ShopperAssistant
                    ) => (
                        <div className="flex justify-end gap-1">
                            <Button
                                type="text"
                                icon={<EditOutlined />}
                                onClick={() =>
                                    handleEdit(
                                        assistant
                                    )
                                }
                            />

                            <Popconfirm
                                title="Delete this Shopper Assistant?"
                                description="This action cannot be undone."
                                okText="Delete"
                                cancelText="Cancel"
                                okButtonProps={{
                                    danger: true,
                                }}
                                onConfirm={() =>
                                    deleteMutation.mutate(
                                        assistant.id
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
                        Shopper Assistants
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        {isMarketAdmin
                            ? 'Manage the assistants in your market.'
                            : 'View assistants across all markets.'}
                    </p>
                </div>

                {isMarketAdmin && (
                    <Button
                        type="primary"
                        size="large"
                        icon={<PlusOutlined />}
                        onClick={handleAdd}
                    >
                        Add Assistant
                    </Button>
                )}
            </div>

            {/* Filters bar */}
            <div className="surface-card p-5 mb-6">
                <div className="flex flex-wrap gap-4 items-end justify-between">
                    <div className="flex flex-wrap gap-4 items-end">
                        <div>
                            <div className="text-xs text-[#6b7280] mb-1.5">
                                Position
                            </div>

                            <Select
                                allowClear
                                placeholder="All positions"
                                value={selectedPosition}
                                onChange={(
                                    value:
                                        | ShopperAssistantPosition
                                        | undefined
                                ) =>
                                    setSelectedPosition(
                                        value
                                    )
                                }
                                options={positionOptions}
                                style={{
                                    width: 180,
                                }}
                            />
                        </div>

                        <div>
                            <div className="text-xs text-[#6b7280] mb-1.5">
                                Search
                            </div>

                            <Input
                                allowClear
                                prefix={
                                    <SearchOutlined
                                        style={{
                                            color: '#9ca3af',
                                        }}
                                    />
                                }
                                placeholder="Name, username, or email..."
                                value={search}
                                onChange={(e) =>
                                    setSearch(
                                        e.target.value
                                    )
                                }
                                style={{ width: 280 }}
                            />
                        </div>
                    </div>

                    <div className="flex items-center gap-3">
                        <span className="text-xs text-[#9ca3af]">
                            {filteredAssistants.length}{' '}
                            {filteredAssistants.length === 1
                                ? 'assistant'
                                : 'assistants'}
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
                    loading={isLoading || marketsLoading}
                    dataSource={filteredAssistants}
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
                                    <TeamOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    {search ||
                                    selectedPosition !==
                                    undefined
                                        ? 'No assistants match your filters'
                                        : 'No shopper assistants yet'}
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    {search ||
                                    selectedPosition !==
                                    undefined
                                        ? 'Try a different filter.'
                                        : isMarketAdmin
                                            ? 'Add your first assistant to get started.'
                                            : 'No assistants are registered yet.'}
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>

            {/* Create / Edit modal */}
            <Modal
                title={
                    editingShopperAssistant
                        ? 'Edit Shopper Assistant'
                        : 'Add Shopper Assistant'
                }
                open={isModalOpen}
                onCancel={handleCancel}
                footer={null}
                width={560}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                    requiredMark={false}
                    className="mt-4"
                >
                    <div className="grid grid-cols-2 gap-4">
                        <Form.Item
                            label="Name"
                            name="name"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter name',
                                },
                            ]}
                        >
                            <Input placeholder="Enter name" />
                        </Form.Item>

                        <Form.Item
                            label="Surname"
                            name="surname"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter surname',
                                },
                            ]}
                        >
                            <Input placeholder="Enter surname" />
                        </Form.Item>
                    </div>

                    <Form.Item
                        label="Email"
                        name="email"
                        rules={[
                            {
                                required: true,
                                type: 'email',
                                message:
                                    'Please enter a valid email',
                            },
                        ]}
                    >
                        <Input placeholder="Enter email" />
                    </Form.Item>

                    <Form.Item
                        label="Username"
                        name="username"
                        rules={[
                            {
                                required: true,
                                message:
                                    'Please enter username',
                            },
                        ]}
                    >
                        <Input placeholder="Enter username" />
                    </Form.Item>

                    {!editingShopperAssistant && (
                        <Form.Item
                            label="Password"
                            name="password"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter password',
                                },
                                {
                                    min: 6,
                                    message:
                                        'Password must be at least 6 characters',
                                },
                            ]}
                        >
                            <Input.Password placeholder="Enter password" />
                        </Form.Item>
                    )}

                    <div className="grid grid-cols-2 gap-4">
                        <Form.Item
                            label="Position"
                            name="position"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please select position',
                                },
                            ]}
                        >
                            <Select
                                placeholder="Select position"
                                options={positionOptions}
                            />
                        </Form.Item>

                        <Form.Item
                            label="Phone Number"
                            name="phoneNumber"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter phone number',
                                },
                            ]}
                        >
                            <Input placeholder="Phone number" />
                        </Form.Item>
                    </div>

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
                            {editingShopperAssistant
                                ? 'Save Changes'
                                : 'Create Assistant'}
                        </Button>
                    </div>
                </Form>
            </Modal>
        </div>
    )
}

export default ShopperAssistants