import {
    Button,
    Form,
    Input,
    message,
} from 'antd'
import { useMutation } from '@tanstack/react-query'
import {
    Link,
    useNavigate,
} from 'react-router-dom'

import { registerCustomer } from '../services/authService'

interface RegisterFormValues {
    username: string
    email: string
    password: string
    confirmPassword: string
}

function Register() {
    const navigate = useNavigate()

    const registerMutation = useMutation({
        mutationFn: registerCustomer,

        onSuccess: () => {
            message.success(
                'Account created. Please sign in.'
            )

            navigate('/login')
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to create account.'
            )
        },
    })

    const handleSubmit = (
        values: RegisterFormValues
    ) => {
        registerMutation.mutate({
            username: values.username,
            email: values.email,
            password: values.password,
        })
    }

    return (
        <div className="min-h-screen bg-gradient-subtle flex items-center justify-center px-4 py-10">
            <div className="w-full max-w-md">
                {/* Brand */}
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-[#4f46e5] text-white text-xl font-bold mb-4 shadow-lg shadow-indigo-500/20">
                        S
                    </div>

                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Create your account
                    </h1>

                    <p className="text-[#6b7280] mt-2 text-sm">
                        Join SimpleStore in a few
                        seconds.
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
                                        'Please enter a username',
                                },
                                {
                                    min: 3,
                                    message:
                                        'Username must be at least 3 characters',
                                },
                            ]}
                        >
                            <Input
                                placeholder="Choose a username"
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
                                {
                                    type: 'email',
                                    message:
                                        'Please enter a valid email',
                                },
                            ]}
                        >
                            <Input
                                placeholder="you@example.com"
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
                                        'Please enter a password',
                                },
                                {
                                    min: 6,
                                    message:
                                        'Password must be at least 6 characters',
                                },
                            ]}
                        >
                            <Input.Password
                                placeholder="Create a password"
                                autoComplete="new-password"
                            />
                        </Form.Item>

                        <Form.Item
                            label="Confirm Password"
                            name="confirmPassword"
                            dependencies={['password']}
                            rules={[
                                {
                                    required: true,
                                    message:
                                        'Please confirm your password',
                                },
                                ({
                                     getFieldValue,
                                 }) => ({
                                    validator(_, value) {
                                        if (
                                            !value ||
                                            getFieldValue(
                                                'password'
                                            ) === value
                                        ) {
                                            return Promise.resolve()
                                        }

                                        return Promise.reject(
                                            new Error(
                                                'Passwords do not match'
                                            )
                                        )
                                    },
                                }),
                            ]}
                        >
                            <Input.Password
                                placeholder="Confirm your password"
                                autoComplete="new-password"
                            />
                        </Form.Item>

                        <Button
                            type="primary"
                            htmlType="submit"
                            block
                            size="large"
                            loading={
                                registerMutation.isPending
                            }
                            className="mt-2"
                        >
                            Create account
                        </Button>
                    </Form>

                    <div className="mt-6 pt-6 border-t border-[#eaeaea] text-center text-sm">
                        <span className="text-[#6b7280]">
                            Already have an account?{' '}
                        </span>

                        <Link
                            to="/login"
                            className="text-[#4f46e5] font-medium hover:text-[#4338ca] transition-colors"
                        >
                            Sign in
                        </Link>
                    </div>
                </div>

                <p className="text-center text-xs text-[#9ca3af] mt-8">
                    © {new Date().getFullYear()}{' '}
                    SimpleStore
                </p>
            </div>
        </div>
    )
}

export default Register