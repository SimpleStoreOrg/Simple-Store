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
    SafetyOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { useAuthStore } from '../../stores/authStore'

import {
    createMarketAdmin,
    createSuperAdmin,
    deleteAdmin,
    getAllAdmins,
    updateAdmin,
} from '../../services/admins/adminService'

import { getAllMarkets } from '../../services/markets/marketService'

import type {
    Admin,
    CreateAdminRequest,
    UpdateAdminRequest,
} from '../../types/admin'

import type { Market } from '../../types/market'

interface AdminFormValues {
    createType?: 'MarketAdmin' | 'SuperAdmin'
    name: string
    surname: string
    email: string
    username: string
    password?: string
    phoneNumber: string
}

const positionFilters = [
    { label: 'Super Admin', value: 0 },
    { label: 'Market Admin', value: 1 },
]

const positionBadge: Record<
    number,
    { bg: string; text: string; dot: string; label: string }
> = {
    0: {
        bg: '#eef2ff',
        text: '#4f46e5',
        dot: '#4f46e5',
        label: 'Super Admin',
    },
    1: {
        bg: '#fffbeb',
        text: '#d97706',
        dot: '#d97706',
        label: 'Market Admin',
    },
}

function Admins() {
    const role = useAuthStore((state) => state.role)

    const adminPosition = useAuthStore(
        (state) => state.adminPosition
    )

    const userId = useAuthStore(
        (state) => state.userId
    )

    const isAdmin = role === 'Admin'

    const isSuperAdmin =
        isAdmin &&
        adminPosition === 'SuperAdmin'

    const [isModalOpen, setIsModalOpen] =
        useState(false)

    const [editingAdmin, setEditingAdmin] =
        useState<Admin | null>(null)

    const [createType, setCreateType] =
        useState<'MarketAdmin' | 'SuperAdmin'>(
            'MarketAdmin'
        )

    const [positionFilter, setPositionFilter] =
        useState<number | undefined>(undefined)

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
        queryKey: ['admins'],
        queryFn: getAllAdmins,
        enabled: isAdmin,
    })

    const {
        data: marketsData,
        isLoading: marketsLoading,
        isError: marketsError,
    } = useQuery({
        queryKey: ['markets'],
        queryFn: getAllMarkets,
        enabled: isSuperAdmin,
    })

    const createMarketAdminMutation =
        useMutation({
            mutationFn: createMarketAdmin,

            onSuccess: () => {
                setIsModalOpen(false)
                form.resetFields()

                queryClient.invalidateQueries({
                    queryKey: ['admins'],
                })
            },
        })

    const createSuperAdminMutation =
        useMutation({
            mutationFn: createSuperAdmin,

            onSuccess: () => {
                setIsModalOpen(false)
                form.resetFields()

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
            data: UpdateAdminRequest
        }) => updateAdmin(id, data),

        onSuccess: () => {
            setIsModalOpen(false)
            setEditingAdmin(null)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['admins'],
            })
        },
    })

    const deleteMutation = useMutation({
        mutationFn: deleteAdmin,

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ['admins'],
            })
        },
    })

    const handleAdd = () => {
        setEditingAdmin(null)
        setCreateType('MarketAdmin')

        form.resetFields()

        form.setFieldValue(
            'createType',
            'MarketAdmin'
        )

        setIsModalOpen(true)
    }

    const handleEdit = (admin: Admin) => {
        setEditingAdmin(admin)

        form.setFieldsValue({
            name: admin.name,
            surname: admin.surname,
            email: admin.email,
            username: admin.username,
            phoneNumber: admin.phoneNumber,
        })

        setIsModalOpen(true)
    }

    const handleCancel = () => {
        setIsModalOpen(false)
        setEditingAdmin(null)
        form.resetFields()
    }

    const handleSubmit = (
        values: AdminFormValues
    ) => {
        if (editingAdmin) {
            const updateData: UpdateAdminRequest = {
                name: values.name,
                surname: values.surname,
                email: values.email,
                username: values.username,
                phoneNumber: values.phoneNumber,
            }

            updateMutation.mutate({
                id: editingAdmin.id,
                data: updateData,
            })

            return
        }

        const createData: CreateAdminRequest = {
            name: values.name,
            surname: values.surname,
            email: values.email,
            username: values.username,
            password: values.password!,
            phoneNumber: values.phoneNumber,
        }

        if (createType === 'MarketAdmin') {
            createMarketAdminMutation.mutate(
                createData
            )
        } else {
            createSuperAdminMutation.mutate(
                createData
            )
        }
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
                    Admins
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Error loading admins:{' '}
                        {error instanceof Error
                            ? error.message
                            : 'Unknown error'}
                    </p>
                </div>
            </div>
        )
    }

    if (isSuperAdmin && marketsError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Admins
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Error loading markets.
                    </p>
                </div>
            </div>
        )
    }

    const admins: Admin[] = data?.items ?? []

    const markets: Market[] = marketsData?.items ?? []

    const getMarketName = (
        marketId: number | null
    ) => {
        if (marketId === null) {
            return '—'
        }

        const market = markets.find(
            (market) => market.id === marketId
        )

        return market?.name ?? 'Unknown market'
    }

    const filteredAdmins = useMemo(() => {
        let result = admins

        if (positionFilter !== undefined) {
            result = result.filter(
                (admin) =>
                    admin.position ===
                    positionFilter
            )
        }

        if (search.trim()) {
            const q = search.trim().toLowerCase()

            result = result.filter((admin) => {
                const name = (
                    admin.name ?? ''
                ).toLowerCase()
                const surname = (
                    admin.surname ?? ''
                ).toLowerCase()
                const username = (
                    admin.username ?? ''
                ).toLowerCase()
                const email = (
                    admin.email ?? ''
                ).toLowerCase()

                return (
                    name.includes(q) ||
                    surname.includes(q) ||
                    username.includes(q) ||
                    email.includes(q)
                )
            })
        }

        return result
    }, [admins, positionFilter, search])

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
            render: (_: unknown, admin: Admin) => {
                const badge =
                    positionBadge[admin.position] ??
                    positionBadge[1]

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
                        {badge.label}
                    </span>
                )
            },
        },
        {
            title: 'Market',
            dataIndex: 'marketId',
            render: (marketId: number | null) => (
                <span className="text-[#6b7280]">
                    {getMarketName(marketId)}
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
                        admin: Admin
                    ) => {
                        const isCurrentAdmin =
                            admin.id === userId

                        return (
                            <div className="flex justify-end gap-1">
                                {isCurrentAdmin && (
                                    <Button
                                        type="text"
                                        icon={
                                            <EditOutlined />
                                        }
                                        onClick={() =>
                                            handleEdit(
                                                admin
                                            )
                                        }
                                    />
                                )}

                                <Popconfirm
                                    title="Delete this admin?"
                                    description="This action cannot be undone."
                                    okText="Delete"
                                    cancelText="Cancel"
                                    okButtonProps={{
                                        danger: true,
                                    }}
                                    onConfirm={() =>
                                        deleteMutation.mutate(
                                            admin.id
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
                        )
                    },
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
                        Admins
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        Manage platform
                        administrators.
                    </p>
                </div>

                {isSuperAdmin && (
                    <Button
                        type="primary"
                        size="large"
                        icon={<PlusOutlined />}
                        onClick={handleAdd}
                    >
                        Add Admin
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
                                value={positionFilter}
                                onChange={setPositionFilter}
                                options={positionFilters}
                                style={{ width: 180 }}
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
                            {filteredAdmins.length}{' '}
                            {filteredAdmins.length === 1
                                ? 'admin'
                                : 'admins'}
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
                    dataSource={filteredAdmins}
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
                                    <SafetyOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    {search ||
                                    positionFilter !==
                                    undefined
                                        ? 'No admins match your filters'
                                        : 'No admins yet'}
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    {search ||
                                    positionFilter !==
                                    undefined
                                        ? 'Try a different filter.'
                                        : 'Add the first admin to get started.'}
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>

            {/* Create / Edit modal */}
            <Modal
                title={
                    editingAdmin
                        ? 'Edit Admin'
                        : 'Add Admin'
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
                    {!editingAdmin && (
                        <Form.Item
                            label="Admin Type"
                            name="createType"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please select admin type',
                                },
                            ]}
                        >
                            <Select
                                size="large"
                                value={createType}
                                onChange={(
                                    value:
                                        | 'MarketAdmin'
                                        | 'SuperAdmin'
                                ) =>
                                    setCreateType(
                                        value
                                    )
                                }
                                options={[
                                    {
                                        label: 'Market Admin',
                                        value: 'MarketAdmin',
                                    },
                                    {
                                        label: 'Super Admin',
                                        value: 'SuperAdmin',
                                    },
                                ]}
                            />
                        </Form.Item>
                    )}

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
                            <Input placeholder="Name" />
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
                            <Input placeholder="Surname" />
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

                    {!editingAdmin && (
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
                        <Input placeholder="Enter phone number" />
                    </Form.Item>

                    <div className="flex justify-end gap-2 mt-6 pt-4 border-t border-[#eaeaea]">
                        <Button
                            onClick={handleCancel}
                            disabled={
                                createMarketAdminMutation.isPending ||
                                createSuperAdminMutation.isPending ||
                                updateMutation.isPending
                            }
                        >
                            Cancel
                        </Button>

                        <Button
                            type="primary"
                            htmlType="submit"
                            loading={
                                createMarketAdminMutation.isPending ||
                                createSuperAdminMutation.isPending ||
                                updateMutation.isPending
                            }
                        >
                            {editingAdmin
                                ? 'Save Changes'
                                : 'Create Admin'}
                        </Button>
                    </div>
                </Form>
            </Modal>
        </div>
    )
}

export default Admins