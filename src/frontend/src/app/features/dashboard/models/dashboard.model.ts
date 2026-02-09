interface DashboardSummary {
    totalBalance: Metric;
    income: Metric;
    expense: Metric;
}

interface Metric {
    amount: number;
    percentageChange: number;
}

interface ExpenseCategoryChart {
    category: string;
    amount: number;
    percentage: number;
}

interface IncomeVsExpenseResponse {
    labels: string[];
    incomes: number[];
    expenses: string[];
}

export type {
    DashboardSummary,
    ExpenseCategoryChart,
    IncomeVsExpenseResponse
}