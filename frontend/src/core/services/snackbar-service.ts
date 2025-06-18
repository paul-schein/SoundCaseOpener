import {inject, Injectable} from '@angular/core';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Duration} from '@js-joda/core';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  public static readonly DURATION: Duration = Duration.ofSeconds(4);
  private snackbar: MatSnackBar = inject(MatSnackBar);
  public show(message: string, duration: Duration = SnackbarService.DURATION): void {
    this.snackbar.open(message, "Close", {
      duration: duration.toMillis(),
      horizontalPosition: "center",
      verticalPosition: "bottom"
    });
  }
}
