import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: 'arrowIconByPercentageChange', pure: true, standalone: true })
export class ArrowIconByPercentageChangePipe implements PipeTransform {
  transform(change: number): string {
    return change > 0 ? 'arrow_upward' : change < 0 ? 'arrow_downward' : '';
  }
}