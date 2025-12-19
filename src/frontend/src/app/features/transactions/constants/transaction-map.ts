import { PaymentType, TransactionType } from "../models/transaction.model";

export const TransactionTypeMap: Record<TransactionType, string> = {
  1: 'Income',
  2: 'Expense',
} as const;

export const PaymentTypeMap: Record<PaymentType, string> = {
    1: 'Cash',
    2: 'Credit Card',
    3: 'Bank Transfer',
    4: 'Other'
} as const;
