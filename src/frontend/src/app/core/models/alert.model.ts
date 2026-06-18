export type AlertType =
  | 'success'
  | 'error';

export type CrudAction =
  | 'Added'
  | 'Updated'
  | 'Deleted';

export interface Alert {
  id: string;
  type: AlertType;
  title?: string;
  message: string;
  leaving?: boolean;
}