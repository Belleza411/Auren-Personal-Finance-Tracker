interface Transaction {
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

interface CreateTransaction {
    name: string;
    amount: number;
    category: string;
    transactionType: TransactionType;
    paymentType: PaymentType;
}

interface TransactionFilter {
    isIncome: boolean;
    isExpense: boolean;
    minAmount: number;
    maxAmount: number;
    endDate: Date;
    startDate: Date;
    category: string;
    paymentMethod: string;
}

enum TransactionType {
    Income = 1,
    Expense = 2
}

enum PaymentType {
    Cash = 1,
    CreditCard = 2,
    BankTransfer = 3,
    Other = 4
}

export type {
    Transaction,
    CreateTransaction,
    TransactionFilter,
    TransactionType,
    PaymentType
}