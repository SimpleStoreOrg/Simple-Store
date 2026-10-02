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
    UserOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
    getCurrentCustomer,
    updateCurrentCustomer,
} from '../../services/customers/customerService'

import type { UpdateCustomerRequest } from '../../services/customers/customerService'

interface ProfileFormValues {
    name: string
    surname: string
    username: string
    email: string
    phoneNumber: string
}

function CustomerProfile() {
    const queryClient = useQueryClient()
    const [form] = Form.useForm<ProfileFormValues>()
    const [isEditing, setIsEditing] = useState(false)

    const {
        data: customer,
        isLoading,
        isError,
        error,
    } = useQuery({
        queryKey: ['current-customer'],
        queryFn: getCurrentCustomer,
    })

    useEffect(() => {
        if (customer) {
            form.setFieldsValue({
                name: customer.name ?? '',
                surname: customer.surname ?? '',
                username: customer.username ?? '',
                email: customer.email ?? '',
                phoneNumber:
                    customer.phoneNumber ?? '',
            })
        }
    }, [customer, form])

    const updateMutation = useMutation({
        mutationFn: (
            values: UpdateCustomerRequest
        ) => updateCurrentCustomer(values),

        onSuccess: async () => {
            message.success(
                'Profile updated successfully.'
            )

            setIsEditing(false)

            await queryClient.invalidateQueries({
                queryKey: ['current-customer'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to update profile.'
            )
        },
    })

    const handleSubmit = (
        values: ProfileFormValues
    ) => {
        updateMutation.mutate({
            name: values.name,
            surname: values.surname,
            username: values.username,
            email: values.email,
            phoneNumber: values.phoneNumber,
        })
    }

    const handleCancel = () => {
        if (customer) {
            form.setFieldsValue({
                name: customer.name ?? '',
                surname: customer.surname ?? '',
                username: customer.username ?? '',
                email: customer.email ?? '',
                phoneNumber:
                    customer.phoneNumber ?? '',
            })
        }

        setIsEditing(false)
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
                    My Profile
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        {error instanceof Error
                            ? error.message
                            : 'Failed to load profile.'}
                    </p>
                </div>
            </div>
        )
    }

    const fullName = customer
        ? `${customer.name ?? ''} ${
            customer.surname ?? ''
        }`.trim()
        : ''

    const initials = fullName
        .split(' ')
        .map((part) => part[0])
        .filter(Boolean)
        .slice(0, 2)
        .join('')
        .toUpperCase()

    return (
        <div>
            {/* Page header */}
            <div className="mb-8">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    My Profile
                </h1>

                <p className="text-[#6b7280] mt-1">
                    {isEditing
                        ? 'Update your personal information.'
                        : 'Your personal information.'}
                </p>
            </div>

            <div className="surface-card max-w-3xl overflow-hidden">
                {/* Identity banner */}
                <div className="flex items-center gap-4 px-6 py-5 border-b border-[#eaeaea] bg-[#fafafa]">
                    <div className="flex items-center justify-center w-14 h-14 rounded-2xl bg-[#4f46e5] text-white text-lg font-semibold shadow-lg shadow-indigo-500/20">
                        {initials || (
                            <UserOutlined />
                        )}
                    </div>

                    <div className="min-w-0">
                        <div className="text-base font-semibold text-[#0a0a0a] truncate">
                            {fullName || '—'}
                        </div>

                        <div className="text-xs text-[#6b7280]">
                            Customer
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
                                <Descriptions.Item label="First Name">
                                    {customer?.name ??
                                        '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Last Name">
                                    {customer?.surname ??
                                        '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Username">
                                    {customer?.username ??
                                        '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Email">
                                    {customer?.email ??
                                        '—'}
                                </Descriptions.Item>

                                <Descriptions.Item label="Phone">
                                    {customer?.phoneNumber ??
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
                                    Edit Profile
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
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-x-4">
                                <Form.Item
                                    label="First Name"
                                    name="name"
                                    rules={[
                                        {
                                            required:
                                                true,
                                            message:
                                                'First name is required',
                                        },
                                        {
                                            max: 20,
                                            message:
                                                'Max 20 characters',
                                        },
                                    ]}
                                >
                                    <Input placeholder="Enter your first name" />
                                </Form.Item>

                                <Form.Item
                                    label="Last Name"
                                    name="surname"
                                    rules={[
                                        {
                                            required:
                                                true,
                                            message:
                                                'Last name is required',
                                        },
                                        {
                                            max: 20,
                                            message:
                                                'Max 20 characters',
                                        },
                                    ]}
                                >
                                    <Input placeholder="Enter your last name" />
                                </Form.Item>
                            </div>

                            <Form.Item
                                label="Username"
                                name="username"
                                rules={[
                                    {
                                        required: true,
                                        message:
                                            'Username is required',
                                    },
                                    {
                                        max: 20,
                                        message:
                                            'Max 20 characters',
                                    },
                                    {
                                        pattern:
                                            /[A-Z]/,
                                        message:
                                            'Must contain at least 1 uppercase letter',
                                    },
                                    {
                                        pattern:
                                            /[a-z]/,
                                        message:
                                            'Must contain at least 1 lowercase letter',
                                    },
                                ]}
                            >
                                <Input placeholder="Enter your username" />
                            </Form.Item>

                            <Form.Item
                                label="Email"
                                name="email"
                                rules={[
                                    {
                                        required: true,
                                        message:
                                            'Email is required',
                                    },
                                    {
                                        type: 'email',
                                        message:
                                            'Please enter a valid email',
                                    },
                                ]}
                            >
                                <Input placeholder="Enter your email" />
                            </Form.Item>

                            <Form.Item
                                label="Phone Number"
                                name="phoneNumber"
                                rules={[
                                    {
                                        required: true,
                                        message:
                                            'Phone number is required',
                                    },
                                ]}
                            >
                                <Input placeholder="Enter your phone number" />
                            </Form.Item>

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

export default CustomerProfile