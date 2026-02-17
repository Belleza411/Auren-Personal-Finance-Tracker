import { Category } from "../features/categories/models/categories.model";
import { Goal } from "../features/goals/models/goals.model";
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

export const DUMMY_GOALS: Goal[] = [
    {
      goalId: 'goal-001',
      userId: 'user-123',
      name: 'Buy a New Laptop',
      description: 'Save money to buy a high-performance laptop for development and studies.',
      emoji: '💻',
      spent: 450,
      budget: 1500,
      goalStatus: 2,
      completionPercentage: 30,
      timeRemaining: '3 months',
      createdAt: 'January 5, 2025',
      targetDate: 'April 30, 2025',
    },
    {
      goalId: 'goal-002',
      userId: 'user-123',
      name: 'Emergency Savings Fund',
      description: 'Build an emergency fund for unexpected expenses.',
      emoji: '💰',
      spent: 1200,
      budget: 3000,
      goalStatus: 2,
      completionPercentage: 40,
      timeRemaining: '6 months',
      createdAt: 'December 1, 2024',
      targetDate: 'August 1, 2025',
    },
    {
      goalId: 'goal-003',
      userId: 'user-123',
      name: 'Vacation Trip',
      description: 'Save for a short holiday trip with friends.',
      emoji: '🌴',
      spent: null,
      budget: 2000,
      goalStatus: 4,
      completionPercentage: null,
      timeRemaining: '9 months',
      createdAt: 'January 20, 2025',
      targetDate: 'October 15, 2025',
    }
]