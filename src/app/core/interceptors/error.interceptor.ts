import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.status === 400 && error.error?.errors) {
        // Handle FluentValidation / ProblemDetails errors
        const validationErrors = error.error.errors;
        const firstErrorKey = Object.keys(validationErrors)[0];
        errorMessage = `${firstErrorKey}: ${validationErrors[firstErrorKey][0]}`;
      } else {
        errorMessage = error.error?.message || error.statusText || errorMessage;
      }

      if (error.status === 0) {
        errorMessage = 'Cannot connect to server. Check Docker status.';
      }

      snackBar.open(errorMessage, 'Close', {
        duration: 7000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['error-snackbar']
      });

      return throwError(() => error);
    })
  );
};
