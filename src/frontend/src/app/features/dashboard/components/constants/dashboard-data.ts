import { DashboardSummary, ExpenseBreakdown, IncomeVsExpenseResponse } from "../../models/dashboard.model";

export const DASHBOARD_SUMMARY_INITIAL_DATA: DashboardSummary = {
  totalBalance: { amount: 0, percentageChange: 0 },
  income: { amount: 0, percentageChange: 0 },
  expense: { amount: 0, percentageChange: 0 }
};

export const INCOME_VS_EXPENSE_INITIAL_DATA: IncomeVsExpenseResponse = {
    labels: [],
    incomes: [],
    expenses: []
}

export const EXPENSE_BREAKDOWN_INITIAL_DATA: ExpenseBreakdown = {
    labels: [],
    data: [],
    percentage: [],
    backgroundColor: [],
    totalSpent: 0
}