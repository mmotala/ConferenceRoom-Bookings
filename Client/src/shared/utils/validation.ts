export type ValidationResult = {
  isValid: boolean;
  errors: string[];
};

export function validateRequired(value: string, fieldName: string): string | null {
  if (!value || value.trim().length === 0) {
    return `${fieldName} is required.`;
  }

  return null;
}

export function validatePositiveNumber(value: number, fieldName: string): string | null {
  if (!Number.isFinite(value) || value <= 0) {
    return `${fieldName} must be greater than zero.`;
  }

  return null;
}

export function validateDateRange(
  startTime: Date | null,
  endTime: Date | null
): string | null {
  if (!startTime || !endTime) {
    return 'Please select both a start time and an end time.';
  }

  if (Number.isNaN(startTime.getTime()) || Number.isNaN(endTime.getTime())) {
    return 'Please select valid start and end times.';
  }

  const now = new Date();

  if (startTime <= now) {
    return 'Start time must be in the future.';
  }

  if (endTime <= startTime) {
    return 'End time must be after start time.';
  }

  const durationHours =
    (endTime.getTime() - startTime.getTime()) / 1000 / 60 / 60;

  if (durationHours > 8) {
    return 'A booking cannot be longer than 8 hours.';
  }

  return null;
}

export function validateEmail(value: string): string | null {
  if (!value || value.trim().length === 0) {
    return 'Email is required.';
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(value)) {
    return 'Email address is invalid.';
  }

  return null;
}
