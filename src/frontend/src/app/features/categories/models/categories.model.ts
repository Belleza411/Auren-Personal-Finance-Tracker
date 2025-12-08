import { TransactionType } from "../../transactions/models/transaction.model";

interface Category {
    categoryId: string;
    userId: string;
    name: string;
    transactionType: TransactionType
    createdAt: Date;
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

export type {
    Category,
    CategoryFilter,
    NewCategory
}