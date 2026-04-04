import { TimePeriod } from "../../core/models/time-period.enum";
import { CategoryFilter } from "../../features/categories/models/categories.model";
import { 
    PAYMENT_TYPE,
    PaymentTypeFilterOption,
    TRANSACTION_TYPE,
    TransactionFilter,
    TransactionTypeFilterOption } 
from "../../features/transactions/models/transaction.model";
import { 
    DropdownLabelOption, 
    FilterKindConfig, 
    FilterTypeOption 
} from "../ui/filters/models/filter.model";

export const transactionTypeOptions
    : FilterTypeOption<TransactionTypeFilterOption>[] = [
    {
      value: 'All Types',
      label: 'All Types'
    }, 
    ...TRANSACTION_TYPE.map(type => ({
      value: type,
      label: type
    }))
];

export const paymentTypeOptions: FilterTypeOption<PaymentTypeFilterOption>[] = [
    {
        value: 'All Payment Method',
        label: 'All Payment Method'
    },
    ...PAYMENT_TYPE.map(type => ({
        value: type,
        label: type.replace(/([A-Z])/g, ' $1').trim() 
    }))
];

export const DATE_FILTER_LABEL_OPTION: DropdownLabelOption = {
    label: 'Date',
    startLabel: 'Start Date',
    endLabel: 'End Date',
    startPlaceholder: 'Select start date',
    endPlaceholder: 'Select end date',
    icon: 'calendar_today'
}

export const TRANSACTION_FILTER_KIND_CONFIG: FilterKindConfig<TransactionFilter>[] = [
    {
        kind: 'searchTerm',
        key: 'searchTerm',
        placeholder: 'Search transactions...'
    },
    {
        kind: 'transactionType',
        key: 'transactionType',
        options: transactionTypeOptions
    },
    {
        kind: 'dateRange',
        key: 'startDate',
    },
    {
        kind: 'categories',
        key: 'category',
    },
    {
        kind: 'paymentType',
        key: 'paymentType',
        options: paymentTypeOptions
    }
]

export const CATEGORY_FILTER_KIND_CONFIG: FilterKindConfig<CategoryFilter>[] = [
    {
        kind: 'searchTerm',
        key: 'searchTerm',
        placeholder: 'Search categories...'
    },
    {
        kind: 'transactionType',
        key: 'transactionType',
        options: transactionTypeOptions
    },
]

export const TIME_PERIOD_OPTIONS = [
  { label: 'All Time',      value: TimePeriod.AllTime },
  { label: 'This Month',    value: TimePeriod.ThisMonth },
  { label: 'Last Month',    value: TimePeriod.LastMonth },
  { label: 'Last 3 Months', value: TimePeriod.Last3Months },
  { label: 'Last 6 Months', value: TimePeriod.Last6Months },
  { label: 'This Year',     value: TimePeriod.ThisYear },
];