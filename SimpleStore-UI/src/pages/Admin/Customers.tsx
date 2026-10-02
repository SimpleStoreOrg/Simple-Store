import {
    Button,
    Input,
    Table,
} from 'antd'
import {
    SearchOutlined,
    UserOutlined,
    ReloadOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { getAllCustomers } from '../../services/customers/customerService'

function Customers() {
    const [search, setSearch] = useState('')

    const {
        data,
        isLoading,
        isError,
        error,
        isFetching,
        refetch,
    } = useQuery({
        queryKey: ['customers'],
        queryFn: getAllCustomers,
    })

    const customers = data?.items ?? []

    const filteredCustomers = useMemo(() => {
        if (!search.trim()) {
            return customers
        }

        const q = search.trim().toLowerCase()

        return customers.filter((customer) => {
            const name = (customer.name ?? '').toLowerCase()
            const surname = (customer.surname ?? '').toLowerCase()
            const username = (customer.username ?? '').toLowerCase()
            const email = (customer.email ?? '').toLowerCase()

            return (
                name.includes(q) ||
                surname.includes(q) ||
                username.includes(q) ||
                email.includes(q)
            )
        })
    }, [customers, search])

    if (isError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Customers
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

    const columns = [
        {
            title: 'Name',
            dataIndex: 'name',
            render: (name: string | null) => (
                <span className="font-medium text-[#0a0a0a]">
                    {name ?? '—'}
                </span>
            ),
        },
        {
            title: 'Surname',
            dataIndex: 'surname',
            render: (surname: string | null) => (
                <span className="text-[#0a0a0a]">
                    {surname ?? '—'}
                </span>
            ),
        },
        {
            title: 'Username',
            dataIndex: 'username',
            render: (username: string | null) => (
                <span className="text-[#6b7280]">
                    {username ?? '—'}
                </span>
            ),
        },
        {
            title: 'Email',
            dataIndex: 'email',
            render: (email: string | null) => (
                <span className="text-[#6b7280]">
                    {email ?? '—'}
                </span>
            ),
        },
        {
            title: 'Phone',
            dataIndex: 'phoneNumber',
            render: (phone: string | null) => (
                <span className="text-[#6b7280] tabular-nums">
                    {phone ?? '—'}
                </span>
            ),
        },
    ]

    return (
        <div>
            {/* Page header */}
            <div className="mb-6">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    Customers
                </h1>

                <p className="text-[#6b7280] mt-1">
                    View customers registered on the
                    platform.
                </p>
            </div>

            {/* Filters bar */}
            <div className="surface-card p-5 mb-6">
                <div className="flex flex-wrap gap-4 items-center justify-between">
                    <div
                        style={{ width: 320 }}
                    >
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
                            placeholder="Search by name, username, or email..."
                            value={search}
                            onChange={(e) =>
                                setSearch(e.target.value)
                            }
                        />
                    </div>

                    <div className="flex items-center gap-3">
                        <span className="text-xs text-[#9ca3af]">
                            {filteredCustomers.length}{' '}
                            {filteredCustomers.length === 1
                                ? 'customer'
                                : 'customers'}
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
                    loading={isLoading}
                    dataSource={filteredCustomers}
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
                                    <UserOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    {search
                                        ? 'No customers match your search'
                                        : 'No customers yet'}
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    {search
                                        ? 'Try a different search term.'
                                        : 'Customers will appear here once they register.'}
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>
        </div>
    )
}

export default Customers