import { Category } from "../../categories/models/categories.model";

export interface Transaction {
    id: string;
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
