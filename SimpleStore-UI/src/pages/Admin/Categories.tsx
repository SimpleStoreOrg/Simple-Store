import {
    Button,
    Form,
    Input,
    Modal,
    Popconfirm,
    Table,
    TreeSelect,
    message,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
    PlusOutlined,
    SearchOutlined,
    ReloadOutlined,
    EditOutlined,
    DeleteOutlined,
    AppstoreOutlined,
} from '@ant-design/icons'
import {
    useMutation,
    useQuery,
    useQueryClient,
} from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
    createCategory,
    deleteCategory,
    getAllCategories,
    updateCategory,
} from '../../services/categories/categoryService'

import type { Category } from '../../types/category'

interface CategoryFormValues {
    name: string
    parentCategoryId?: number
}

interface TreeOption {
    value: number
    title: string
    children?: TreeOption[]
}

interface DisplayRow extends Category {
    depth: number
}

function Categories() {
    const queryClient = useQueryClient()
    const [form] = Form.useForm<CategoryFormValues>()

    const [isModalOpen, setIsModalOpen] =
        useState(false)

    const [editingCategory, setEditingCategory] =
        useState<Category | null>(null)

    const [search, setSearch] = useState('')

    const {
        data,
        isLoading,
        isError,
        error,
        isFetching,
        refetch,
    } = useQuery({
        queryKey: ['categories'],
        queryFn: getAllCategories,
    })

    const categories: Category[] =
        data?.items ?? []

    const categoryTreeOptions = useMemo(() => {
        const build = (
            parentId: number | null,
            excludeId?: number
        ): TreeOption[] => {
            return categories
                .filter(
                    (c) =>
                        c.parentCategoryId ===
                        parentId &&
                        c.id !== excludeId
                )
                .sort((a, b) =>
                    a.name.localeCompare(b.name)
                )
                .map((c) => {
                    const children = build(
                        c.id,
                        excludeId
                    )

                    return {
                        value: c.id,
                        title: c.name,
                        children:
                            children.length > 0
                                ? children
                                : undefined,
                    }
                })
        }

        return build(
            null,
            editingCategory?.id
        )
    }, [categories, editingCategory])

    const categoryMap = useMemo(() => {
        const map = new Map<number, string>()

        categories.forEach((c) => {
            map.set(c.id, c.name)
        })

        return map
    }, [categories])

    const displayRows = useMemo(() => {
        const rows: DisplayRow[] = []

        const walk = (
            parentId: number | null,
            depth: number
        ) => {
            categories
                .filter(
                    (c) =>
                        c.parentCategoryId ===
                        parentId
                )
                .sort((a, b) =>
                    a.name.localeCompare(b.name)
                )
                .forEach((c) => {
                    rows.push({
                        ...c,
                        depth,
                    })

                    walk(c.id, depth + 1)
                })
        }

        walk(null, 0)

        return rows
    }, [categories])

    const filteredRows = useMemo(() => {
        if (!search.trim()) {
            return displayRows
        }

        const q = search.trim().toLowerCase()

        return displayRows.filter((row) =>
            row.name.toLowerCase().includes(q)
        )
    }, [displayRows, search])

    const createMutation = useMutation({
        mutationFn: createCategory,

        onSuccess: () => {
            message.success(
                'Category created successfully.'
            )

            setIsModalOpen(false)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['categories'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to create category.'
            )
        },
    })

    const updateMutation = useMutation({
        mutationFn: ({
                         id,
                         data,
                     }: {
            id: number
            data: {
                name: string
                parentCategoryId: number | null
            }
        }) => updateCategory(id, data),

        onSuccess: () => {
            message.success(
                'Category updated successfully.'
            )

            setIsModalOpen(false)
            setEditingCategory(null)
            form.resetFields()

            queryClient.invalidateQueries({
                queryKey: ['categories'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to update category.'
            )
        },
    })

    const deleteMutation = useMutation({
        mutationFn: deleteCategory,

        onSuccess: () => {
            message.success(
                'Category deleted successfully.'
            )

            queryClient.invalidateQueries({
                queryKey: ['categories'],
            })
        },

        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to delete category.'
            )
        },
    })

    const handleAdd = () => {
        setEditingCategory(null)
        form.resetFields()
        setIsModalOpen(true)
    }

    const handleEdit = (category: Category) => {
        setEditingCategory(category)

        form.setFieldsValue({
            name: category.name,
            parentCategoryId:
                category.parentCategoryId ?? undefined,
        })

        setIsModalOpen(true)
    }

    const handleCancel = () => {
        setIsModalOpen(false)
        setEditingCategory(null)
        form.resetFields()
    }

    const handleSubmit = (
        values: CategoryFormValues
    ) => {
        const parentCategoryId =
            values.parentCategoryId ?? null

        if (editingCategory) {
            updateMutation.mutate({
                id: editingCategory.id,
                data: {
                    name: values.name,
                    parentCategoryId,
                },
            })

            return
        }

        createMutation.mutate({
            name: values.name,
            parentCategoryId,
        })
    }

    if (isError) {
        return (
            <div>
                <h1 className="text-3xl font-semibold tracking-tight mb-4">
                    Categories
                </h1>

                <div className="surface-card p-6">
                    <p className="text-[#6b7280]">
                        Error loading categories:{' '}
                        {error instanceof Error
                            ? error.message
                            : 'Unknown error'}
                    </p>
                </div>
            </div>
        )
    }

    const columns: ColumnsType<DisplayRow> = [
        {
            title: 'Name',
            dataIndex: 'name',
            key: 'name',
            render: (name: string, row) => (
                <div
                    style={{
                        paddingLeft:
                            row.depth * 24,
                    }}
                    className="flex items-center gap-2"
                >
                    {row.depth > 0 && (
                        <span className="text-[#d4d4d8] select-none">
                            └
                        </span>
                    )}

                    <span
                        className={
                            row.depth === 0
                                ? 'font-medium text-[#0a0a0a]'
                                : 'text-[#0a0a0a]'
                        }
                    >
                        {name}
                    </span>
                </div>
            ),
        },
        {
            title: 'Parent Category',
            dataIndex: 'parentCategoryId',
            key: 'parentCategoryId',
            render: (
                parentCategoryId: number | null
            ) =>
                parentCategoryId ? (
                    <span className="text-[#6b7280]">
                        {categoryMap.get(
                                parentCategoryId
                            ) ??
                            `Category #${parentCategoryId}`}
                    </span>
                ) : (
                    <span className="text-[#9ca3af]">
                        —
                    </span>
                ),
        },
        {
            title: '',
            key: 'actions',
            width: 120,
            render: (_, category) => (
                <div className="flex justify-end gap-1">
                    <Button
                        type="text"
                        icon={<EditOutlined />}
                        onClick={() =>
                            handleEdit(category)
                        }
                    />

                    <Popconfirm
                        title="Delete this category?"
                        description="Cannot delete if it has subcategories or products."
                        okText="Delete"
                        cancelText="Cancel"
                        okButtonProps={{
                            danger: true,
                        }}
                        onConfirm={() =>
                            deleteMutation.mutate(
                                category.id
                            )
                        }
                    >
                        <Button
                            type="text"
                            danger
                            icon={<DeleteOutlined />}
                        />
                    </Popconfirm>
                </div>
            ),
        },
    ]

    return (
        <div>
            {/* Page header */}
            <div className="flex items-start justify-between mb-6">
                <div>
                    <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                        Categories
                    </h1>

                    <p className="text-[#6b7280] mt-1">
                        Organize products into a
                        hierarchy.
                    </p>
                </div>

                <Button
                    type="primary"
                    size="large"
                    icon={<PlusOutlined />}
                    onClick={handleAdd}
                >
                    Add Category
                </Button>
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
                            placeholder="Search categories..."
                            value={search}
                            onChange={(e) =>
                                setSearch(e.target.value)
                            }
                        />
                    </div>

                    <div className="flex items-center gap-3">
                        <span className="text-xs text-[#9ca3af]">
                            {filteredRows.length}{' '}
                            {filteredRows.length === 1
                                ? 'category'
                                : 'categories'}
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
                    dataSource={filteredRows}
                    rowKey="id"
                    columns={columns}
                    pagination={false}
                    locale={{
                        emptyText: (
                            <div className="py-12">
                                <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-[#eef2ff] text-[#4f46e5] mb-3">
                                    <AppstoreOutlined
                                        style={{
                                            fontSize: 22,
                                        }}
                                    />
                                </div>

                                <div className="text-sm font-medium text-[#0a0a0a]">
                                    {search
                                        ? 'No categories match your search'
                                        : 'No categories yet'}
                                </div>

                                <div className="text-xs text-[#6b7280] mt-1">
                                    {search
                                        ? 'Try a different search term.'
                                        : 'Create your first category to get started.'}
                                </div>
                            </div>
                        ),
                    }}
                />
            </div>

            {/* Create / Edit modal */}
            <Modal
                title={
                    editingCategory
                        ? 'Edit Category'
                        : 'Add Category'
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
                    <Form.Item
                        label="Name"
                        name="name"
                        rules={[
                            {
                                required: true,
                                message:
                                    'Please enter category name',
                            },
                        ]}
                    >
                        <Input placeholder="Enter category name" />
                    </Form.Item>

                    <Form.Item
                        label="Parent Category (optional)"
                        name="parentCategoryId"
                    >
                        <TreeSelect
                            allowClear
                            placeholder="No parent (top-level)"
                            treeData={
                                categoryTreeOptions
                            }
                            treeDefaultExpandAll
                        />
                    </Form.Item>

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
                            {editingCategory
                                ? 'Save Changes'
                                : 'Create Category'}
                        </Button>
                    </div>
                </Form>
            </Modal>
        </div>
    )
}

export default Categories