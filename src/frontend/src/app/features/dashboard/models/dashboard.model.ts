interface DashboardSummary {
    totalBalance: Metric;
    income: Metric;
    expense: Metric;
}

interface Metric {
    amount: number;
    percentageChange: number;
}

interface IncomeVsExpenseResponse {
    labels: string[];
    incomes: number[];
    expenses: number[];
}

interface ExpenseBreakdown {
    labels: string[];
    data: number[];
    percentage: number[];
    totalSpent: number;
}
export type {
    DashboardSummary,
    IncomeVsExpenseResponse,
    ExpenseBreakdown 
}