import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'transactionAmountSign',
})
export class TransactionAmountSignPipe implements PipeTransform {

  transform(type: "Income" | "Expense"): string {
    return type === "Income" ? "+" : "-";
  }
}
