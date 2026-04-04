import { TimePeriod } from "../../features/transactions/models/transaction.model";
import { createHttpParams } from "./http-params.util";

export const createTimePeriodParams = (timePeriod?: TimePeriod) => {
  const filters = timePeriod !== undefined ? { timePeriod: timePeriod.toString().replace(/\s+/g, '') } : {};
  return createHttpParams(filters);
};