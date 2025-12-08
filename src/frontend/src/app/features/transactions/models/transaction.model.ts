interface Transaction {
    transactionId: string;
    userId: string;
    categoryId: string;
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
}

interface TransactionFilter {
    isIncome: boolean;
    isExpense: boolean;
    minAmount: number;
    maxAmount: number;
    endDate: Date | string;
    startDate: Date | string;
    category: string;
    paymentMethod: string;
}

enum TransactionType {
    income = 1,
    expense = 2
}

enum PaymentType {
    cash = 1,
    creditCard = 2,
    bankTransfer = 3,
    other = 4
}

interface AvgDailySpending {
    avgSpending: number;
    percentageChange: number;
}

export type {
    Transaction,
    NewTransaction,
    TransactionFilter,
    TransactionType,
    PaymentType,
    AvgDailySpending
}