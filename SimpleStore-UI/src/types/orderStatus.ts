export const OrderStatus = {
    New: 0,
    Accepted: 1,
    Collecting: 2,
    ReadyToGo: 3,
    Completed: 4,
    CancelledByCustomer: 5,
    CancelledByShop: 6,
} as const

export type OrderStatus =
    (typeof OrderStatus)[keyof typeof OrderStatus]