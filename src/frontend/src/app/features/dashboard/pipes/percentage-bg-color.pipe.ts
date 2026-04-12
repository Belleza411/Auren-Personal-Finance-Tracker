import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'percentageBgColor',
})
export class PercentageBgColorPipe implements PipeTransform {
  transform(value: number): string {
    if(value > 0) return 'bg-[#34c7597a] text-green-800'
    if(value < 0) return 'bg-[#ff3b307a] text-red-800'
    return 'bg-gray-300 text-black'
  }
}
