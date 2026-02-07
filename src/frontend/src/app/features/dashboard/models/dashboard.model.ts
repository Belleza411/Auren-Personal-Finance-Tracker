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

export type {
    DashboardSummary,
    ExpenseCategoryChart
}