export type ValidationResult = {
  isValid: boolean;
  errors: string[];
};

export function validateRequired(value: string, fieldName: string): string | null {
  if (!value || value.trim().length === 0) {
    return `${fieldName} is required`;
  }

  return null;
}

export function validatePositiveNumber(value: number, fieldName: string): string | null {
  if (!Number.isFinite(value) || value <= 0) {
    return `${fieldName} must be greater than zero`;
  }

  return null;
}

export function validateDateRange(startTime: string, endTime: string): string | null {
  if (!startTime || !endTime) {
    return 'Start and end time are required';
  }

  const start = new Date(startTime);
  const end = new Date(endTime);

  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
    return 'Start and end time must be valid dates';
  }

  if (start <= new Date()) {
    return 'Start time must be in the future';
  }

  if (end <= start) {
    return 'End time must be after start time';
  }

  const durationHours = (end.getTime() - start.getTime()) / 1000 / 60 / 60;

  if (durationHours > 8) {
    return 'Booking cannot be longer than 8 hours';
  }

  return null;
}

export function validateEmail(value: string): string | null {
  if (!value || value.trim().length === 0) {
    return 'Email is required';
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(value)) {
    return 'Email address is invalid';
  }

  return null;
}
