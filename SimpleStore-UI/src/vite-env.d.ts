/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_USER_SERVICE_URL?: string
    readonly VITE_ORDER_SERVICE_URL?: string
    readonly VITE_PRODUCT_SERVICE_URL?: string
    readonly VITE_MARKET_SERVICE_URL?: string
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
