import { Category } from "../../categories/models/categories.model";

interface Transaction {
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

interface NewTransaction {
    name: string;
    amount: number;
    category: string;
    transactionType: TransactionType;
    paymentType: PaymentType;
    transactionDate: Date | string;
}

interface TransactionFilter {
    searchTerm: string;
    transactionType: TransactionType | null;
    minAmount: number | null;
    maxAmount: number | null;
    startDate: Date | string | null;
    endDate: Date | string | null;
    category: string[];
    paymentType: PaymentType | null;
}

export enum TransactionType {
    Income = 1,
    Expense = 2
}

export enum PaymentType {
    Cash = 1,
    CreditCard = 2,
    BankTransfer = 3,
    Other = 4
}

export enum TimePeriod {
    AllTime = 1,
    ThisMonth = 2,
    LastMonth = 3,
    Last3Months = 4,
    Last6Months = 5,
    ThisYear = 6
}

interface BalanceSummary {
    income: number;
    expense: number;
    balance: number;
}

interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
}

export type {
    Transaction,
    NewTransaction,
    TransactionFilter,
    BalanceSummary,
    PagedResult
}