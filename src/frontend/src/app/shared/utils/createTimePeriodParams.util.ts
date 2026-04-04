import { TimePeriod, TimePeriodLabel } from "../../core/models/time-period.enum";
import { createHttpParams } from "./http-params.util";

export const createTimePeriodParams = (timePeriod?: TimePeriod) => {
  const filters = timePeriod !== undefined ? { timePeriod: TimePeriodLabel[timePeriod] } : {};
  return createHttpParams(filters);
};