import { useAuthStore } from '../stores/authStore'
import {
    Button,
    Form,
    Input,
    Select,
    message,
} from 'antd'
import { useMutation } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { login } from '../services/authService'
import type { LoginRequest } from '../types/auth'

function Login() {
    const setTokens = useAuthStore(
        (state) => state.setTokens
    )

    const navigate = useNavigate()

    const loginMutation = useMutation({
        mutationFn: login,

        onSuccess: (data) => {
            setTokens(
                data.accessToken,
                data.refreshToken
            )

            message.success('Welcome back')

            navigate('/')
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Login failed. Please check your credentials.'
            )
        },
    })

    const handleSubmit = (
        values: LoginRequest
    ) => {
        loginMutation.mutate(values)
    }

    return (
        <div className="min-h-screen bg-gradient-subtle flex items-center justify-center px-4">
            <div className="w-full max-w-md">
                {/* Brand */}
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-[#4f46e5] text-white text-xl font-bold mb-4 shadow-lg shadow-indigo-500/20">
                        S
                    </div>

                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        SimpleStore
                    </h1>

                    <p className="text-[#6b7280] mt-2 text-sm">
                        Welcome back. Sign in to
                        continue.
                    </p>
                </div>

                {/* Card */}
                <div className="surface-float p-8">
                    <Form
                        layout="vertical"
                        onFinish={handleSubmit}
                        requiredMark={false}
                        size="large"
                    >
                        <Form.Item
                            label="Username"
                            name="username"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter your username',
                                },
                            ]}
                        >
                            <Input
                                placeholder="Enter your username"
                                autoComplete="username"
                            />
                        </Form.Item>

                        <Form.Item
                            label="Email"
                            name="email"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter your email',
                                },
                            ]}
                        >
                            <Input
                                placeholder="Enter your email"
                                autoComplete="email"
                            />
                        </Form.Item>

                        <Form.Item
                            label="Password"
                            name="password"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please enter your password',
                                },
                            ]}
                        >
                            <Input.Password
                                placeholder="Enter your password"
                                autoComplete="current-password"
                            />
                        </Form.Item>

                        <Form.Item
                            label="Role"
                            name="role"
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please select your role',
                                },
                            ]}
                        >
                            <Select
                                placeholder="Select your role"
                                options={[
                                    {
                                        label: 'Admin',
                                        value: 0,
                                    },
                                    {
                                        label: 'Shopper Assistant',
                                        value: 1,
                                    },
                                    {
                                        label: 'Customer',
                                        value: 2,
                                    },
                                ]}
                            />
                        </Form.Item>

                        <Button
                            type="primary"
                            htmlType="submit"
                            block
                            size="large"
                            loading={
                                loginMutation.isPending
                            }
                            className="mt-2"
                        >
                            Sign in
                        </Button>
                    </Form>

                    <div className="mt-6 pt-6 border-t border-[#eaeaea] text-center text-sm">
                        <span className="text-[#6b7280]">
                            Don't have an account?{' '}
                        </span>

                        <Link
                            to="/register"
                            className="text-[#4f46e5] font-medium hover:text-[#4338ca] transition-colors"
                        >
                            Register as Customer
                        </Link>
                    </div>
                </div>

                {/* Footer */}
                <p className="text-center text-xs text-[#9ca3af] mt-8">
                    © {new Date().getFullYear()}{' '}
                    SimpleStore
                </p>
            </div>
        </div>
    )
}

export default Login