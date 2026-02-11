import { AfterViewInit, Component, input, OnChanges, viewChild } from '@angular/core';
import { IncomeVsExpenseResponse } from '../../models/dashboard.model';
import { ChartData, ChartOptions } from 'chart.js'
import { BaseChartDirective } from 'ng2-charts'

@Component({
  selector: 'app-income-vs-expense-graph',
  imports: [BaseChartDirective],
  templateUrl: './income-vs-expense-graph.html',
  styleUrl: './income-vs-expense-graph.css',
})
export class IncomeVsExpenseGraph implements OnChanges, AfterViewInit {
  readonly chartData = input.required<IncomeVsExpenseResponse>();
  chart = viewChild<BaseChartDirective>('baseChart');

  protected chartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: { 
        mode: 'index', 
        intersect: false,
        borderColor: '#c5c5c5',
        backgroundColor: '#fff',
        displayColors: false,
        padding: 10,
        cornerRadius: 8,
        bodyColor: '#000',
        callbacks: {
          label: (ctx) => {
            const value = ctx.parsed.y;
            return `$${value} ${ctx.dataset.label}`;
          }
        }
      },
    },
    scales: {
      y: {
        grid: { display: false },
        ticks: { display: false },
        border: { display: false }
      },
      x: {
        grid: { display: false },
        border: { display: false }
      }
    }
  } 

  protected chartJsData: ChartData<'line'> = {
    labels: [],
    datasets: []
  };

  ngOnChanges(): void {
    if(!this.chartData()) return;

    const totalIncomes = this.chartData().incomes.reduce((a, b) => a + b, 0);
    const totalExpenses = this.chartData().expenses.reduce((a, b) => a + b, 0);

    const incomeOrder = totalIncomes >= totalExpenses ? 2 : 1;
    const expenseOrder = totalIncomes >= totalExpenses ? 1 : 2;

    this.chartJsData = {
      labels: this.chartData().labels,
      datasets: [
        {
          label: "Incomes",
          data: this.chartData().incomes,
          borderColor: '#0d7818',
          backgroundColor: (ctx) => {
            const chart = ctx.chart;
            const { ctx: c, chartArea } = chart;
            if (!chartArea) return;

            const gradient = c.createLinearGradient(0, 0, 0, chartArea.bottom);
            gradient.addColorStop(0, 'rgba(191, 230, 201, 0.35)');
            gradient.addColorStop(1, 'rgba(57, 179, 90, 0.6)');
            return gradient;
          },
          fill: true,
          tension: 0.3,
          pointRadius: 3,
          borderWidth: 1.5,
          order: incomeOrder
        },
        {
          label: "Expenses",
          data: this.chartData().expenses,
          borderColor: '#ba1616',
           backgroundColor: (ctx) => {
            const chart = ctx.chart;
            const { ctx: c, chartArea } = chart;
            if (!chartArea) return;

            const gradient = c.createLinearGradient(0, 0, 0, chartArea.bottom);
            gradient.addColorStop(0, 'rgba(240, 240, 240, 0.3)');
            gradient.addColorStop(1, 'rgba(255, 0, 0, 0.3)');
            return gradient;
          },
          fill: true,
          tension: 0.3,
          pointRadius: 3,
          borderWidth: 1.5,
          order: expenseOrder
        }
      ]
    };

    setTimeout(() => this.chart()?.update(), 0);
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.chart()?.update(), 0);
  }
}
