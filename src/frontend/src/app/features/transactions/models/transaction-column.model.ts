export type TransactionColumnKey = 
    'dateCreated' |
    'title' |
    'category' |
    'type' |
    'amount' |
    'transactionDate' |
    'paymentMethod' |
    'actions';

export interface TransactionColumn {
    key: TransactionColumnKey;
    label: string;

    align?: 'left' | 'right' | 'center';
}

export const COMPACT_TRANSACTION_COLUMNS: TransactionColumn[] = [
  {
    key: 'transactionDate',
    label: 'Date',
  },
  {
    key: 'title',
    label: 'Title',
  },
  {
    key: 'category',
    label: 'Category',
  },
  {
    key: 'amount',
    label: 'Amount',
    align: 'right',
  },
];

export const FULL_TRANSACTION_COLUMNS: TransactionColumn[] = [
  {
    key: 'transactionDate',
    label: 'Transaction Date',
  },
  {
    key: 'title',
    label: 'Title',
  },
  {
    key: 'category',
    label: 'Category',
  },
  {
    key: 'type',
    label: 'Type',
  },
  {
    key: 'amount',
    label: 'Amount',
    align: 'right',
  },
  {
    key: 'dateCreated',
    label: 'Date Created',
  },
  {
    key: 'paymentMethod',
    label: 'Payment Method',
  },
  {
    key: 'actions',
    label: '',
  },
];
