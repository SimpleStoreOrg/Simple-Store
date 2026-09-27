import type { Customer } from '../types/customer'

export function isProfileComplete(
    customer: Customer | null | undefined
): boolean {
    if (!customer) {
        return false
    }

    const name = customer.name?.trim() ?? ''
    const surname = customer.surname?.trim() ?? ''
    const phoneNumber =
        customer.phoneNumber?.trim() ?? ''

    return (
        name.length > 0 &&
        surname.length > 0 &&
        phoneNumber.length > 0
    )
}

export function getMissingProfileFields(
    customer: Customer | null | undefined
): string[] {
    if (!customer) {
        return ['name', 'surname', 'phoneNumber']
    }

    const missing: string[] = []

    if (!customer.name?.trim()) {
        missing.push('First Name')
    }

    if (!customer.surname?.trim()) {
        missing.push('Last Name')
    }

    if (!customer.phoneNumber?.trim()) {
        missing.push('Phone Number')
    }

    return missing
}