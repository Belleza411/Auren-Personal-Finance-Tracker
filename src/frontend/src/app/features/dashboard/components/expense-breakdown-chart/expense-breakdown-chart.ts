import { Component, computed, input, OnChanges, signal } from '@angular/core';
import { ExpenseBreakdown } from '../../models/dashboard.model';
import { Chart, ChartData, ChartOptions } from 'chart.js';
import { BaseChartDirective } from "ng2-charts";
import { CurrencyPipe } from '@angular/common';
import { centerTextPlugin } from '../constants/center-text-plugin';

Chart.register(centerTextPlugin);

@Component({
  selector: 'app-expense-breakdown-chart',
  imports: [BaseChartDirective, CurrencyPipe],
  templateUrl: './expense-breakdown-chart.html',
  styleUrl: './expense-breakdown-chart.css',
})
export class ExpenseBreakdownChart implements OnChanges {
  data = input.required<ExpenseBreakdown>();

  items = computed(() => {
    const breakdown = this.data();

    return breakdown.labels.map((label, i) => ({
      label,
      amount: breakdown.data[i], 
      percentage: breakdown.percentage[i],
      backgroundColor: breakdown.backgroundColor[i],
    }));
  });

  protected chartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '70%',
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        enabled: false,
      },
    },
    scales: {
      y: {
        display: false,
      },
      x: {
        display: false,
      }
    },
  }

  protected doughnutData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [],
  };

  ngOnChanges(): void {
    if(!this.data()) return;

    this.doughnutData = {
      labels: this.data().labels,
      datasets: [{
        data: this.data().data,
        backgroundColor: this.data().backgroundColor
      }],
      
    }
  }
}
