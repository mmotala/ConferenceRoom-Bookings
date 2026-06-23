import { describe, expect, it, vi } from 'vitest';

import {
  formatBookingDate,
  getDefaultEndTime,
  getDefaultStartTime,
  toDateTimeLocalValue,
  toUtcIsoString
} from './dateUtils';

describe('date utilities', () => {
  it('formats and converts dates', () => {
    const date = new Date('2030-01-02T03:04:00.000Z');

    expect(toDateTimeLocalValue(date)).toMatch(/^2030-01-02T/);
    expect(toUtcIsoString('2030-01-02T03:04')).toBe(new Date('2030-01-02T03:04').toISOString());
    expect(formatBookingDate('2030-01-02T03:04:00.000Z')).toMatch(/02 Jan 2030/);
  });

  it('creates default start and end values one hour apart', () => {
    vi.setSystemTime(new Date(2030, 0, 1, 10, 23));

    expect(getDefaultStartTime()).toMatch(/^2030-01-01T11:00$/);
    expect(getDefaultEndTime()).toMatch(/^2030-01-01T12:00$/);

    vi.useRealTimers();
  });
});
