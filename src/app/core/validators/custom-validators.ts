import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  /**
   * Validates that the password meets complexity requirements:
   * At least 8 characters, one uppercase, one lowercase, and one number.
   */
  static passwordPattern(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;

      const hasUpperCase = /[A-Z]/.test(value);
      const hasLowerCase = /[a-z]/.test(value);
      const hasNumber = /[0-9]/.test(value);
      const isLongEnough = value.length >= 8;

      const passwordValid = hasUpperCase && hasLowerCase && hasNumber && isLongEnough;

      return !passwordValid ? { passwordComplexity: true } : null;
    };
  }

  /**
   * Validates that two controls have the same value (e.g., password and confirmPassword).
   */
  static match(controlName: string, matchingControlName: string): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const control = group.get(controlName);
      const matchingControl = group.get(matchingControlName);

      if (!control || !matchingControl) return null;

      if (matchingControl.errors && !matchingControl.errors['mustMatch']) return null;

      if (control.value !== matchingControl.value) {
        matchingControl.setErrors({ mustMatch: true });
        return { mustMatch: true };
      } else {
        matchingControl.setErrors(null);
        return null;
      }
    };
  }
}
