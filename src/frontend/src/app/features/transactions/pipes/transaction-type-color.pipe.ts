import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'transactionTypeColor',
})
export class TransactionTypeColorPipe implements PipeTransform {
  transform(type: "Income" | "Expense", hasBgColor: boolean): string {
    if (hasBgColor) {
      return type === "Income" ? "text-green-500 bg-green-100" : "text-red-500 bg-red-100";
    }

    return type === "Income" ? "text-green-500" : "text-red-500";
  }
}
