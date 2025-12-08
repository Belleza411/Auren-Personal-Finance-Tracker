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
    createdAt: Date;
    targetDate: Date;
}

interface NewGoal {
    name: string;
    description: string;
    spent: number;
    budget: number;
    status: GoalStatus;
    targetDate: Date;
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
    completed = 1,
    onTrack = 2,
    onHold = 3,
    notStarted = 4,
    behindSchedule = 5,
    cancelled = 6
}

export type {
    Goal,
    GoalStatus,
    GoalFilter,
    NewGoal
}