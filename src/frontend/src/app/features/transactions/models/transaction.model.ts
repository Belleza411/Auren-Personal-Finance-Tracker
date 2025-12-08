export interface Transaction {
    transactionId: string;
    userId: string;
    categoryId: string;
    transactionType: TransactionType;
    name: string;
    amount: number
    paymentType: PaymentType;
    transactionDate: Date;
    createAt: Date;
}

export interface CreateTransaction {
    name: string;
    amount: number;
    category: string;
    transactionType: TransactionType;
    paymentType: PaymentType;
}

export interface TransactionFilter {
    isIncome: boolean;
    isExpense: boolean;
    minAmount: number;
    maxAmount: number;
    endDate: Date;
    startDate: Date;
    category: string;
    paymentMethod: string;
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