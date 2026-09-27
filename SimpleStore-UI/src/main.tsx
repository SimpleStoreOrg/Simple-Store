import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import {
    QueryClient,
    QueryClientProvider,
} from '@tanstack/react-query'
import { ConfigProvider } from 'antd'
import './index.css'
import App from './App.tsx'

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            refetchOnWindowFocus: false,
            retry: 1,
        },
    },
})

createRoot(
    document.getElementById('root')!
).render(
    <StrictMode>
        <QueryClientProvider client={queryClient}>
            <ConfigProvider
                theme={{
                    token: {
                        colorPrimary: '#4f46e5',
                        colorSuccess: '#059669',
                        colorWarning: '#d97706',
                        colorError: '#dc2626',

                        colorBgBase: '#ffffff',
                        colorTextBase: '#0a0a0a',

                        colorBorder: '#eaeaea',
                        colorBorderSecondary:
                            '#f0f0f0',

                        borderRadius: 10,
                        borderRadiusLG: 12,
                        borderRadiusSM: 8,

                        fontFamily:
                            "'Inter', ui-sans-serif, system-ui, sans-serif",
                        fontSize: 14,

                        controlHeight: 40,
                        controlHeightLG: 48,

                        boxShadow:
                            '0 1px 3px rgba(0, 0, 0, 0.04), 0 4px 12px rgba(0, 0, 0, 0.04)',

                        lineWidth: 1,
                        wireframe: false,
                    },
                    components: {
                        Button: {
                            primaryShadow: 'none',
                            defaultShadow: 'none',
                            dangerShadow: 'none',
                            fontWeight: 500,
                            paddingInline: 20,
                        },
                        Input: {
                            paddingBlock: 10,
                            paddingInline: 14,
                            activeShadow:
                                '0 0 0 4px rgba(79, 70, 229, 0.12)',
                        },
                        InputNumber: {
                            paddingBlock: 10,
                            paddingInline: 14,
                            activeShadow:
                                '0 0 0 4px rgba(79, 70, 229, 0.12)',
                        },
                        Select: {
                            optionSelectedBg:
                                '#eef2ff',
                            optionSelectedColor:
                                '#4f46e5',
                        },
                        Card: {
                            paddingLG: 24,
                            boxShadowTertiary:
                                '0 1px 3px rgba(0, 0, 0, 0.04), 0 4px 12px rgba(0, 0, 0, 0.04)',
                        },
                        Table: {
                            headerBg: '#fafafa',
                            headerColor: '#6b7280',
                            headerSplitColor:
                                'transparent',
                            rowHoverBg: '#fafafa',
                            borderColor: '#f0f0f0',
                            cellPaddingBlock: 14,
                            cellPaddingInline: 16,
                        },
                        Modal: {
                            borderRadiusLG: 16,
                            paddingContentHorizontalLG: 28,
                        },
                        Tag: {
                            defaultBg: '#f4f4f5',
                            defaultColor: '#52525b',
                        },
                        Menu: {
                            itemBorderRadius: 10,
                            itemSelectedBg:
                                'rgba(79, 70, 229, 0.12)',
                            itemSelectedColor: '#ffffff',
                            itemHoverBg:
                                'rgba(255, 255, 255, 0.06)',
                            itemColor:
                                'rgba(255, 255, 255, 0.7)',
                        },
                        Layout: {
                            siderBg: '#0f0f12',
                            headerBg: '#ffffff',
                            bodyBg: '#fafafa',
                            headerHeight: 64,
                            headerPadding: '0 24px',
                        },
                    },
                }}
            >
                <App />
            </ConfigProvider>
        </QueryClientProvider>
    </StrictMode>
)