import {
    Descriptions,
    Spin,
} from 'antd'
import {
    UserOutlined,
    InfoCircleOutlined,
    ShopOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'

import { getCurrentShopperAssistant } from '../../services/shopperAssistants/shopperAssistantService'
import { getAllMarkets } from '../../services/markets/marketService'

import { shopperAssistantPositionLabels } from '../../types/shopperAssistant'

import type { Market } from '../../types/market'

const positionAccent: Record<
    number,
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

function ShopperAssistantProfile() {
    const {
        data: shopperAssistant,
        isLoading,
        isError,
        error,
    } = useQuery({
        queryKey: ['current-shopper-assistant'],
        queryFn: getCurrentShopperAssistant,
    })

    const {
        data: marketsData,
        isLoading: marketsLoading,
    } = useQuery({
        queryKey: ['markets'],
        queryFn: getAllMarkets,
    })

    if (isLoading || marketsLoading) {
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

    const fullName = shopperAssistant
        ? `${shopperAssistant.name ?? ''} ${
            shopperAssistant.surname ?? ''
        }`.trim()
        : ''

    const initials = fullName
        .split(' ')
        .map((part) => part[0])
        .filter(Boolean)
        .slice(0, 2)
        .join('')
        .toUpperCase()

    const positionLabel = shopperAssistant
        ? shopperAssistantPositionLabels[
            shopperAssistant.position
            ]
        : ''

    const accent = shopperAssistant
        ? positionAccent[
        shopperAssistant.position
        ] ?? positionAccent[0]
        : positionAccent[0]

    const markets: Market[] =
        marketsData?.items ?? []

    const marketName = shopperAssistant
        ? markets.find(
            (m) =>
                m.id === shopperAssistant.marketId
        )?.name ??
        `Market #${shopperAssistant.marketId}`
        : '—'

    return (
        <div>
            {/* Page header */}
            <div className="mb-8">
                <h1 className="text-3xl font-semibold tracking-tight text-[#0a0a0a]">
                    My Profile
                </h1>

                <p className="text-[#6b7280] mt-1">
                    Your personal information.
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

                    <div className="min-w-0 flex-1">
                        <div className="text-base font-semibold text-[#0a0a0a] truncate">
                            {fullName || '—'}
                        </div>

                        <div className="flex items-center gap-2 mt-0.5 flex-wrap">
                            <span className="text-xs text-[#6b7280]">
                                Shopper Assistant
                            </span>

                            {positionLabel && (
                                <>
                                    <span className="text-[#d4d4d8]">
                                        ·
                                    </span>

                                    <span
                                        className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium"
                                        style={{
                                            background:
                                            accent.bg,
                                            color: accent.text,
                                        }}
                                    >
                                        <span
                                            className="w-1 h-1 rounded-full"
                                            style={{
                                                background:
                                                accent.dot,
                                            }}
                                        />
                                        {
                                            positionLabel
                                        }
                                    </span>
                                </>
                            )}
                        </div>
                    </div>
                </div>

                {/* Body */}
                <div className="p-6">
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
                            {shopperAssistant?.name ??
                                '—'}
                        </Descriptions.Item>

                        <Descriptions.Item label="Last Name">
                            {shopperAssistant?.surname ??
                                '—'}
                        </Descriptions.Item>

                        <Descriptions.Item label="Username">
                            {shopperAssistant?.username ??
                                '—'}
                        </Descriptions.Item>

                        <Descriptions.Item label="Email">
                            {shopperAssistant?.email ??
                                '—'}
                        </Descriptions.Item>

                        <Descriptions.Item label="Phone">
                            {shopperAssistant?.phoneNumber ??
                                '—'}
                        </Descriptions.Item>

                        <Descriptions.Item label="Position">
                            {positionLabel || '—'}
                        </Descriptions.Item>

                        <Descriptions.Item label="Market">
                            <span className="inline-flex items-center gap-2">
                                <ShopOutlined
                                    style={{
                                        fontSize: 13,
                                        color: '#6b7280',
                                    }}
                                />

                                {marketName}
                            </span>
                        </Descriptions.Item>
                    </Descriptions>

                    <div className="mt-6 pt-6 border-t border-[#eaeaea] flex items-start gap-2 text-xs text-[#6b7280]">
                        <InfoCircleOutlined
                            style={{
                                fontSize: 13,
                                marginTop: 2,
                                flexShrink: 0,
                            }}
                        />

                        <span>
                            Contact your Market
                            Administrator to update
                            your information.
                        </span>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default ShopperAssistantProfile