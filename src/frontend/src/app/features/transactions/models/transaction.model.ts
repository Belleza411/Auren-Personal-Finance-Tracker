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
    Income = 1,
    Expense = 2
}

enum PaymentType {
    Cash = 1,
    CreditCard = 2,
    BankTransfer = 3,
    Other = 4
}

enum TimePeriod {
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

export const TransactionTypeMap: Record<TransactionType, string> = {
  1: 'Income',
  2: 'Expense',
};

export const PaymentTypeMap: Record<PaymentType, string> = {
    1: 'Cash',
    2: 'Credit Card',
    3: 'Bank Transfer',
    4: 'Other'
}

export type {
    Transaction,
    NewTransaction,
    TransactionFilter,
    TransactionType,
    PaymentType,
    TimePeriod,
    BalanceSummary
}