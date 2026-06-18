import { Component, inject } from '@angular/core';
import { AlertService } from '../../../core/services/alert.service';
import { AsyncPipe } from '@angular/common';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideCircleCheck, lucideCircleX } from '@ng-icons/lucide';
import { HlmAlert, HlmAlertDescription, HlmAlertTitle } from "../../../libs/ui/alert/src";

@Component({
  selector: 'alert-container',
  imports: [
    HlmAlert,
    HlmAlertDescription,
    HlmAlertTitle,
    AsyncPipe,
    NgIcon
],
  providers: [provideIcons({ lucideCircleCheck, lucideCircleX })],
  templateUrl: './alert-container.html',
  styleUrl: './alert-container.css',
})
export class AlertContainer {
  private readonly alertService = inject(AlertService);
  public alerts$ = this.alertService.alerts$;
}
