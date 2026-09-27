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
    ShopOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { useAuthStore } from '../../stores/authStore'

import {
    createMarket,
    deleteMarket,
    getAllMarkets,
    updateMarket,
} from '../../services/markets/marketService'

import { getAllAdmins } from '../../services/admins/adminService'

import type {
    Market,
    CreateMarketRequest,
    UpdateMarketRequest,
} from '../../types/market'

import type { Admin } from '../../types/admin'

interface MarketFormValues {
    marketAdminId?: number
    name: string
    location: string
    email: string
    phoneNumber: string
}

function Markets() {
    const role = useAuthStore((state) => state.role)

    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const [isModalOpen, setIsModalOpen] =
        useState(false)

    const [editingMarket, setEditingMarket] =
        useState<Market | null>(null)

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
        queryKey: ['markets'],
        queryFn: getAllMarkets,
    })

    const {
        data: adminsData,
        isLoading: adminsLoading,
        isError: adminsError,
    } = useQuery({
        queryKey: ['admins'],
        queryFn: getAllAdmins,
        enabled:
            role === 'Admin' &&
            adminPosition === 'SuperAdmin',
    })

    const admins: Admin[] = adminsData?.items ?? []

    const marketAdmins: Admin[] = admins.filter(
        (admin) =>
            admin.position === 1 &&
            admin.marketId === null
    )

    const getMarketAdminName = (
        marketAdminId: number
    ) => {
        const admin = admins.find(
            (admin) => admin.id === marketAdminId
        )

        if (!admin) {
            return `Unknown admin (ID: ${marketAdminId})`
        }

        return `${admin.name} ${admin.surname}`
    }

    const createMutation = useMutation({
        mutationFn: createMarket,

        onSuccess: () => {
            setIsModalOpen(false)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['markets'],
            })

            queryClient.invalidateQueries({
                queryKey: ['admins'],
            })
        },
    })

    const updateMutation = useMutation({
        mutationFn: ({
                         id,
                         data,
                     }: {
            id: number
            data: UpdateMarketRequest
        }) => updateMarket(id, data),

        onSuccess: () => {
            setIsModalOpen(false)
            setEditingMarket(null)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['markets'],
            })
        },
    })

    const deleteMutation = useMutation({
        mutationFn: deleteMarket,

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ['markets'],
            })

            queryClient.invalidateQueries({
                queryKey: ['admins'],
            })
        },
    })

    const handleAdd = () => {
        setEditingMarket(null)
        form.resetFields()
        setIsModalOpen(true)
    }

    const handleEdit = (market: Market) => {
        setEditingMarket(market)

        form.setFieldsValue({
            name: market.name,
            location: market.location,
            email: market.email,
            phoneNumber: market.phoneNumber,
        })

        setIsModalOpen(true)
    }

    const handleCancel = () => {
        setIsModalOpen(false)
        setEditingMarket(null)
        form.resetFields()
    }

    const handleSubmit = (
        values: MarketFormValues
    ) => {
        if (editingMarket) {
            const updateData: UpdateMarketRequest = {
                name: values.name,
                location: values.location,
                email: values.email,
                phoneNumber: values.phoneNumber,
            }

            updateMutation.mutate({
                id: editingMarket.id,
                data: updateData,
            })

            return
        }

        const createData: CreateMarketRequest = {
            marketAdminId: values.marketAdminId!,
            name: values.name,
            location: values.location,
            email: values.email,
            phoneNumber: values.phoneNumber,
        }

        createMutation.mutate(createData)
    }

    if (isError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Markets
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

    if (adminsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Markets
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Error loading admins.
                    </p>
                </div>
            </div>
        )
    }

    const markets: Market[] = data?.items ?? []

    const filteredMarkets = useMemo(() => {
        if (!search.trim()) {
            return markets
        }

        const q = search.trim().toLowerCase()

        return markets.filter((market) => {
            const name = (market.name ?? '').toLowerCase()
            const location = (market.location ?? '').toLowerCase()
            const email = (market.email ?? '').toLowerCase()

            return (
                name.includes(q) ||
                location.includes(q) ||
                email.includes(q)
            )
        })
    }, [markets, search])

    const isSuperAdmin =
        role === 'Admin' &&
        adminPosition === 'SuperAdmin'

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
            title: 'Location',
            dataIndex: 'location',
            render: (location: string) => (
                <span className="text-[#6b7280]">
                    {location ?? '—'}
                </span>
            ),
        },
        {
            title: 'Email',
            dataIndex: 'email',
            render: (email: string) => (
                <span className="text-[#6b7280]">
                    {email ?? '—'}
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
            title: 'Market Admin',
            dataIndex: 'marketAdminId',
            render: (marketAdminId: number) => (
                <span className="text-[#6b7280]">
                    {getMarketAdminName(
                        marketAdminId
                    )}
                </span>
            ),
        },
        ...(isSuperAdmin
            ? [
                {
                    title: '',
                    key: 'actions',
                    width: 120,
                    render: (
                        _: unknown,
                        market: Market
                    ) => (
                        <div className="flex justify-end gap-1">
                            <Button
                                type="text"
                                icon={<EditOutlined />}
                                onClick={() =>
                                    handleEdit(market)
                                }
                            />

                            <Popconfirm
                                title="Delete this market?"
                                description="This action cannot be undone."
                                okText="Delete"
                                cancelText="Cancel"
                                okButtonProps={{
                                    danger: true,
                                }}
                                onConfirm={() =>
                                    deleteMutation.mutate(
                                        market.id
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
                        Markets
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        Manage all markets on the
                        platform.
                    </p>
                </div>

                {isSuperAdmin && (
                    <Button
                        type="primary"
                        size="large"
                        icon={<PlusOutlined />}
                        onClick={handleAdd}
                    >
                        Add Market
                    </Button>
                )}
            </div>

            {/* Filters bar */}
            <div className="surface-card p-5 mb-6">
                <div className="flex flex-wrap gap-4 items-center justify-between">
                    <div style={{ width: 320 }}>
                        <Input
                            allowClear
                            size="large"
                            prefix={
                                <SearchOutlined
                                    style={{
                                        color: '#9ca3af',
                                    }}
                                />
                            }
                            placeholder="Search by name, location, or email..."
                            value={search}
                            onChange={(e) =>
                                setSearch(e.target.value)
                            }
                        />
                    </div>

                    <div className="flex items-center gap-3">
                        <span className="text-xs text-[#9ca3af]">
                            {filteredMarkets.length}{' '}
                            {filteredMarkets.length === 1
                                ? 'market'
                                : 'markets'}
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
                    loading={isLoading || adminsLoading}
                    dataSource={filteredMarkets}
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
                                    <ShopOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    {search
                                        ? 'No markets match your search'
                                        : 'No markets yet'}
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    {search
                                        ? 'Try a different search term.'
                                        : 'Create your first market to get started.'}
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>

            {/* Create / Edit modal */}
            <Modal
                title={
                    editingMarket
                        ? 'Edit Market'
                        : 'Add Market'
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
                    {!editingMarket && (
                        <Form.Item
                            label="Market Admin"
                            name="marketAdminId"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please select a market admin',
                                },
                            ]}
                        >
                            <Select
                                size="large"
                                placeholder="Select market admin"
                                loading={adminsLoading}
                                options={marketAdmins.map(
                                    (admin) => ({
                                        value: admin.id,
                                        label: `${admin.name} ${admin.surname} (${admin.username})`,
                                    })
                                )}
                            />
                        </Form.Item>
                    )}

                    <Form.Item
                        label="Market Name"
                        name="name"
                        rules={[
                            {
                                required: true,
                                message:
                                    'Please enter market name',
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
                                    'Please enter location',
                            },
                        ]}
                    >
                        <Input placeholder="Enter location" />
                    </Form.Item>

                    <div className="grid grid-cols-2 gap-4">
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
                            <Input placeholder="Email" />
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
                            <Input placeholder="Phone" />
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
                            {editingMarket
                                ? 'Save Changes'
                                : 'Create Market'}
                        </Button>
                    </div>
                </Form>
            </Modal>
        </div>
    )
}

export default Markets