import { Service } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TimePeriod } from '../models/time-period.enum';

@Service()
export class TimePeriodService {
  private _selectedTimePeriod = new BehaviorSubject<TimePeriod>(2);
  selectedTimePeriod$ = this._selectedTimePeriod.asObservable();

  setTimePeriod(timePeriod: TimePeriod) {
    this._selectedTimePeriod.next(timePeriod);
  }
}
