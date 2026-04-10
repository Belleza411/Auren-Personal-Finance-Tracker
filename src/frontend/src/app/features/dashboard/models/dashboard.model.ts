export interface DashboardSummary {
    totalBalance: Metric;
    income: Metric;
    expense: Metric;
    averageDailySpending: Metric;
}

export interface Metric {
    amount: number;
    percentageChange: number;
}

export interface IncomeVsExpenseResponse {
    labels: string[];
    incomes: number[];
    expenses: number[];
}

export interface ExpenseBreakdown {
    labels: string[];
    data: number[];
    percentage: number[];
    backgroundColor: string[];
    totalSpent: number;
}

export interface DashboardData {
    summary: DashboardSummary;
    incomeVsExpense: IncomeVsExpenseResponse;
    expenseBreakdown: ExpenseBreakdown;
}