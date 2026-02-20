import { 
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
]