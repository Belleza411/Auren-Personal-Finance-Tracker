import { NoopScrollStrategy } from '@angular/cdk/overlay';
import { inject, Injectable, Type } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { SlidePanel, SlidePanelConfig } from '../models/slide-panel';

@Injectable({
  providedIn: 'root',
})
export class SlidePanelService {
  private dialog = inject(MatDialog);

  open<TComponent, TData = unknown, TRresult = unknown>(
    component: Type<TComponent>,
    config?: SlidePanelConfig<TData>
  ): MatDialogRef<TComponent, TRresult> {
    const dialogRef = this.dialog.open(component, {
      scrollStrategy: new NoopScrollStrategy(),
      width: config?.width ?? '30rem',
      height: '100%',
      data: config?.data,
      panelClass: 'dialog',
      position: { 
        right: '0',
        bottom: '0',
        top: '0',
      },
      disableClose: true,
    });

    dialogRef.backdropClick().subscribe(() => {
      this.triggerClose(dialogRef);
    });

    return dialogRef;
  }

  private triggerClose<T>(dialogRef: MatDialogRef<T>) {
    const instance = dialogRef.componentInstance as SlidePanel;
    instance.startClose();
  }
}
