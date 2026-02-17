import { Category } from "../features/categories/models/categories.model";
import { Transaction } from "../features/transactions/models/transaction.model";

export const dummyCategories: Category[] = [
    {
        categoryId: '1',
        userId: '1',
        name: 'Salary',
        transactionType: 1,
        createdAt: "June 1, 2025"
    },
    {
        categoryId: '2',
        userId: '1',
        name: 'Shopping',
        transactionType: 2,
        createdAt: "June 2, 2025"
    },
    {
        categoryId: '3',
        userId: '1',
        name: 'Health',
        transactionType: 2,
        createdAt: "June 10, 2025"
    }
]

export const  dummyTransactions: Transaction[] = [
    {
        transactionId: '1',
        userId: '1',
        categoryId: '1',
        category: dummyCategories[0],
        transactionType: 1,
        name: 'Freelance Payment',
        amount: 2000.0,
        paymentType: 3,
        transactionDate: 'June 1, 2025',
        createdAt: 'June 1, 2025'
    },
    {
        transactionId: '2',
        userId: '1',
        categoryId: '2',
        category: dummyCategories[1],
        transactionType: 2,
        name: 'Groceries',
        amount: 100.0,
        paymentType: 3,
        transactionDate: 'June 2, 2025',
        createdAt: 'June 2, 2025'
    },
    {
        transactionId: '3',
        userId: '1',
        categoryId: '3',
        category: dummyCategories[2],
        transactionType: 2,
        name: 'Health Insurance',
        amount: 55.0,
        paymentType: 3,
        transactionDate: 'June 10, 2025',
        createdAt: 'June 10, 2025'
    },
    {
        transactionId: '1',
        userId: '1',
        categoryId: '1',
        category: dummyCategories[0],
        transactionType: 1,
        name: 'Freelance Payment',
        amount: 2000.0,
        paymentType: 3,
        transactionDate: 'June 1, 2025',
        createdAt: 'June 1, 2025'
    },
    {
        transactionId: '2',
        userId: '1',
        categoryId: '2',
        category: dummyCategories[1],
        transactionType: 2,
        name: 'Groceries',
        amount: 100.0,
        paymentType: 3,
        transactionDate: 'June 2, 2025',
        createdAt: 'June 2, 2025'
    },
    {
        transactionId: '3',
        userId: '1',
        categoryId: '3',
        category: dummyCategories[2],
        transactionType: 2,
        name: 'Health Insurance',
        amount: 55.0,
        paymentType: 3,
        transactionDate: 'June 10, 2025',
        createdAt: 'June 10, 2025'
    }
]