import { 
    DropdownLabelOption,
    FilterTypeOption,
    PAYMENT_TYPE,
    PaymentTypeFilterOption,
    TRANSACTION_TYPE,
    TransactionTypeFilterOption } 
from "../../features/transactions/models/transaction.model";

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

export const AMOUNT_FILTER_LABEL_OPTION: DropdownLabelOption = {
    label: 'Amount',
    startLabel: 'Min Amount',
    endLabel: 'Max Amount',
    startPlaceholder: 'Enter minimum amount',
    endPlaceholder: 'Enter maximum amount',
    icon: 'keyboard_arrow_down'
}

export const DATE_FILTER_LABEL_OPTION: DropdownLabelOption = {
    label: 'Date',
    startLabel: 'Start Date',
    endLabel: 'End Date',
    startPlaceholder: 'Select start date',
    endPlaceholder: 'Select end date',
    icon: 'calendar_today'
}