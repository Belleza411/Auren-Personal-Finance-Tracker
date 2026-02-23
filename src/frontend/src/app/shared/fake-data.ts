import { Category } from "../features/categories/models/categories.model";
import { Goal } from "../features/goals/models/goals.model";
import { Transaction } from "../features/transactions/models/transaction.model";

export const dummyCategories: Category[] = [
  {
    id: '1',
    userId: '1',
    name: 'Salary',
    transactionType: 'Income',
    createdAt: 'June 1, 2025'
  },
  {
    id: '2',
    userId: '1',
    name: 'Freelance',
    transactionType: 'Income',
    createdAt: 'June 3, 2025'
  },
  {
    id: '3',
    userId: '1',
    name: 'Investments',
    transactionType: 'Income',
    createdAt: 'June 5, 2025'
  },
  {
    id: '4',
    userId: '1',
    name: 'Groceries',
    transactionType: 'Expense',
    createdAt: 'June 2, 2025'
  },
  {
    id: '5',
    userId: '1',
    name: 'Rent',
    transactionType: 'Expense',
    createdAt: 'June 1, 2025'
  },
  {
    id: '6',
    userId: '1',
    name: 'Transport',
    transactionType: 'Expense',
    createdAt: 'June 4, 2025'
  },
  {
    id: '7',
    userId: '1',
    name: 'Health',
    transactionType: 'Expense',
    createdAt: 'June 6, 2025'
  },
  {
    id: '8',
    userId: '1',
    name: 'Entertainment',
    transactionType: 'Expense',
    createdAt: 'June 7, 2025'
  },
  {
    id: '9',
    userId: '1',
    name: 'Utilities',
    transactionType: 'Expense',
    createdAt: 'June 8, 2025'
  },
  {
    id: '10',
    userId: '1',
    name: 'Savings',
    transactionType: 'Income',
    createdAt: 'June 9, 2025'
  }
];

export const dummyTransactions: Transaction[] = [
  {
    id: '1',
    userId: '1',
    categoryId: '1',
    category: dummyCategories[0],
    transactionType: 'Income',
    name: 'Monthly Salary',
    amount: 3500,
    paymentType: 'BankTransfer',
    transactionDate: 'June 1, 2025',
    createdAt: 'June 1, 2025'
  },
  {
    id: '2',
    userId: '1',
    categoryId: '2',
    category: dummyCategories[1],
    transactionType: 'Income',
    name: 'Website Project',
    amount: 1200,
    paymentType: 'BankTransfer',
    transactionDate: 'June 3, 2025',
    createdAt: 'June 3, 2025'
  },
  {
    id: '3',
    userId: '1',
    categoryId: '3',
    category: dummyCategories[2],
    transactionType: 'Income',
    name: 'Stock Dividends',
    amount: 300,
    paymentType: 'Other',
    transactionDate: 'June 5, 2025',
    createdAt: 'June 5, 2025'
  },
  {
    id: '4',
    userId: '1',
    categoryId: '4',
    category: dummyCategories[3],
    transactionType: 'Expense',
    name: 'Weekly Groceries',
    amount: 150,
    paymentType: 'CreditCard',
    transactionDate: 'June 2, 2025',
    createdAt: 'June 2, 2025'
  },
  {
    id: '5',
    userId: '1',
    categoryId: '5',
    category: dummyCategories[4],
    transactionType: 'Expense',
    name: 'Monthly Rent',
    amount: 1200,
    paymentType: 'BankTransfer',
    transactionDate: 'June 1, 2025',
    createdAt: 'June 1, 2025'
  },
  {
    id: '6',
    userId: '1',
    categoryId: '6',
    category: dummyCategories[5],
    transactionType: 'Expense',
    name: 'Fuel',
    amount: 80,
    paymentType: 'CreditCard',
    transactionDate: 'June 4, 2025',
    createdAt: 'June 4, 2025'
  },
  {
    id: '7',
    userId: '1',
    categoryId: '7',
    category: dummyCategories[6],
    transactionType: 'Expense',
    name: 'Doctor Visit',
    amount: 60,
    paymentType: 'Other',
    transactionDate: 'June 6, 2025',
    createdAt: 'June 6, 2025'
  },
  {
    id: '8',
    userId: '1',
    categoryId: '8',
    category: dummyCategories[7],
    transactionType: 'Expense',
    name: 'Movie Night',
    amount: 45,
    paymentType: 'CreditCard',
    transactionDate: 'June 7, 2025',
    createdAt: 'June 7, 2025'
  },
  {
    id: '9',
    userId: '1',
    categoryId: '9',
    category: dummyCategories[8],
    transactionType: 'Expense',
    name: 'Electricity Bill',
    amount: 110,
    paymentType: 'BankTransfer',
    transactionDate: 'June 8, 2025',
    createdAt: 'June 8, 2025'
  },
  {
    id: '10',
    userId: '1',
    categoryId: '10',
    category: dummyCategories[9],
    transactionType: 'Income',
    name: 'Emergency Fund',
    amount: 500,
    paymentType: 'BankTransfer',
    transactionDate: 'June 9, 2025',
    createdAt: 'June 9, 2025'
  }
];

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