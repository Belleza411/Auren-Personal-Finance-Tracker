interface Goal {
    goalId: string;
    userId: string;
    name: string;
    description: string;
    spent: number | null;
    budget: number;
    goalStatus: GoalStatus,
    completionPercentage: number | null;
    timeRemaining: string | null;
    createdAt: Date | string;
    targetDate: Date | string;
}

interface NewGoal {
    name: string;
    description: string;
    spent: number;
    budget: number;
    status: GoalStatus;
    targetDate: Date | string;
}

interface GoalFilter {
    isCompleted: boolean;
    isOnTracker: boolean;
    isOnHold: boolean;
    isNotStarted: boolean;
    isBehindSchedule: boolean;
    isCancelled: boolean;
}

enum GoalStatus {
    Completed = 1,
    OnTrack = 2,
    OnHold = 3,
    NotStarted = 4,
    BehindSchedule = 5,
    Cancelled = 6
}

interface GoalsSummary {
    totalGoals: number;
    goalsCompleted: number;
    activeGoals: number;
}

export type {
    Goal,
    GoalStatus,
    GoalFilter,
    NewGoal,
    GoalsSummary
}