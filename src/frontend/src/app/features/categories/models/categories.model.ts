import { TransactionType } from "../../transactions/models/transaction.model";

export interface Category {
    id: string;
    userId: string;
    name: string;
    transactionType: TransactionType
    createdAt: Date | string;
}

export interface CategoryFilter {
    searchTerm: string;
    transactionType: TransactionType | null;
}

export interface NewCategory {
    name: string;
    transactionType: TransactionType;
}