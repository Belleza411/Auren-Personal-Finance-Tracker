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

export interface TransactionFilter {
    isIncome: boolean | null;
    isExpense: boolean | null;
    minAmount: number | null;
    maxAmount: number | null;
    endDate: Date | null;
    startDate: Date | null;
    category: string | null;
    paymentMethod: string | null;
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

// public bool? IsIncome { get; set; }
// public bool? IsExpense { get; set; }
// public decimal? MinAmount { get; set; }
// public decimal? MaxAmount { get; set; }
// public DateTime? EndDate { get; set; }
// public DateTime? StartDate { get; set; }
// public string? Category { get; set; }
// public string? PaymentMethod { get; set; }