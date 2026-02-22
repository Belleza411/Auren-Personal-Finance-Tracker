import { Category } from "../../categories/models/categories.model";

export interface Transaction {
    transactionId: string;
    userId: string;
    categoryId: string;
    category: Category;
    transactionType: TransactionType;
    name: string;
    amount: number
    paymentType: PaymentType;
    transactionDate: Date | string;
    createdAt: Date | string;
}

export interface NewTransaction {
    name: string;
    amount: number;
    category: string;
    transactionType: TransactionType;
    paymentType: PaymentType;
    transactionDate: Date | string;
}

export interface TransactionFilter {
    searchTerm: string;
    transactionType: TransactionType | null;
    startDate: Date | string | null;
    endDate: Date | string | null;
    category: string[];
    paymentType: PaymentType | null;
}

export const TRANSACTION_TYPE = ['Income', 'Expense'] as const;
export const PAYMENT_TYPE = ['Cash', 'CreditCard', 'BankTransfer', 'Other'] as const;

export type TransactionType = typeof TRANSACTION_TYPE[number];
export type PaymentType = typeof PAYMENT_TYPE[number];

export type TransactionTypeFilterOption = TransactionType | 'All Types';
export type PaymentTypeFilterOption = PaymentType | 'All Payment Method';

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

export type FilterKind = 'number' | 'date' | 'string';

export enum TimePeriod {
    AllTime = 1,
    ThisMonth = 2,
    LastMonth = 3,
    Last3Months = 4,
    Last6Months = 5,
    ThisYear = 6
}

export interface BalanceSummary {
    income: number;
    expense: number;
    balance: number;
}

export interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
}
