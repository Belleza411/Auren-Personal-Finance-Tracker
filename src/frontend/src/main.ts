import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { CategoryScale, Chart, Filler, Legend, LinearScale, LineController, LineElement, PointElement, Tooltip } from 'chart.js';

Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Tooltip,
  Legend,
  Filler
);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
