import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-summary-card',
  imports: [],
  templateUrl: './summary-card.html',
  styleUrl: './summary-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'bg-white border border-gray-300 rounded-[14px] flex flex-col px-[1.2rem] py-[1.1rem] shadow-lg'
  }
})
export class SummaryCard {

}
