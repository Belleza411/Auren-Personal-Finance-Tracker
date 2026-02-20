import { Pipe, PipeTransform } from '@angular/core';
import { PaymentType } from '../models/transaction.model';

@Pipe({
  name: 'addWhitespace',
})
export class AddWhitespacePipe implements PipeTransform {
  transform(value: PaymentType,): unknown {
    return value.replace(/([A-Z])/g, ' $1').trim();
  }
}
