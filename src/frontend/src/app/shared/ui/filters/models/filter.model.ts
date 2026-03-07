export interface FilterTypeOption<T> {
    value: T;
    label: string;
}

export interface DropdownLabelOption {
    label: string;
    startLabel: string;
    endLabel: string;
    startPlaceholder: string;
    endPlaceholder: string;
    icon: string;
}

export type DropdownKind = 'number' | 'date' | 'string';

export type FilterKind =
    | 'searchTerm'
    | 'transactionType'
    | 'categories'
    | 'paymentType'
    | 'status'
    | 'budget'
    | 'dateRange';

export interface FilterKindConfig<T> {
    kind: FilterKind;
    key: keyof T;
    options?: any[];
    placeholder?: string;
}