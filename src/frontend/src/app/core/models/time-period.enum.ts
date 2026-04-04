export enum TimePeriod {
    AllTime = 1,
    ThisMonth = 2,
    LastMonth = 3,
    Last3Months = 4,
    Last6Months = 5,
    ThisYear = 6
}

export const TimePeriodLabel: Record<TimePeriod, string> = {
  [TimePeriod.AllTime]:    "AllTime",
  [TimePeriod.ThisMonth]:  "ThisMonth",
  [TimePeriod.LastMonth]:  "LastMonth",
  [TimePeriod.Last3Months]: "Last3Months",
  [TimePeriod.Last6Months]: "Last6Months",
  [TimePeriod.ThisYear]:   "ThisYear",
};
