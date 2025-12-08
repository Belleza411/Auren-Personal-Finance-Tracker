import { TransactionType } from "../../transactions/models/transaction.model";

interface Category {
    categoryId: string;
    userId: string;
    name: string;
    transactionType: TransactionType
    createdAt: Date;
}

export type {
    Category
}