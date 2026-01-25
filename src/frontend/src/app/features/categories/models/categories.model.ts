import { TransactionType } from "../../transactions/models/transaction.model";

interface Category {
    categoryId: string;
    userId: string;
    name: string;
    transactionType: TransactionType
    createdAt: Date | string;
}

interface CategoryFilter {
    searchTerm: string;
    transactionType: TransactionType | null;
}

interface NewCategory {
    name: string;
    transactionType: TransactionType;
}

export type {
    Category,
    CategoryFilter,
    NewCategory,
}