import { addHours, format, startOfHour } from 'date-fns';

export function toDateTimeLocalValue(date: Date): string {
  return format(date, "yyyy-MM-dd'T'HH:mm");
}

export function getDefaultStartTime(): string {
  const nextHour = addHours(startOfHour(new Date()), 1);
  return toDateTimeLocalValue(nextHour);
}

export function getDefaultEndTime(): string {
  const nextHour = addHours(startOfHour(new Date()), 2);
  return toDateTimeLocalValue(nextHour);
}

export function toUtcIsoString(localDateTimeValue: string): string {
  return new Date(localDateTimeValue).toISOString();
}

export function formatBookingDate(value: string): string {
  return format(new Date(value), 'dd MMM yyyy, HH:mm');
}
