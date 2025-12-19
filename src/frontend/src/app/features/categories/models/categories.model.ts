import { TransactionType } from "../../transactions/models/transaction.model";

interface Category {
    categoryId: string;
    userId: string;
    name: string;
    transactionType: TransactionType
    createdAt: Date | string;
}

interface CategoryFilter {
    isIncome: boolean;
    isExpense: boolean;
    transactions: number;
    category: string;
}

interface NewCategory {
    name: string;
    transactionType: TransactionType;
}

interface CategoryOverview {
    category: string;
    transactionType: TransactionType;
    totalSpending: number;
    averageSpending: number;
    transactionCount: number;
    lastUsed: Date | string;
}

interface CategorySummary {
    totalCategories: number;
    mostUsedCategory: string;
    highestSpendingCategory: string;
}

export type {
    Category,
    CategoryFilter,
    NewCategory,
    CategoryOverview,
    CategorySummary
}