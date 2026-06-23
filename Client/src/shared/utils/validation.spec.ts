import { describe, expect, it, vi } from 'vitest';

import {
  validateDateRange,
  validateEmail,
  validatePositiveNumber,
  validateRequired
} from './validation';

describe('validation utilities', () => {
  it('validates required text', () => {
    expect(validateRequired('', 'Purpose')).toBe('Purpose is required.');
    expect(validateRequired('   ', 'Purpose')).toBe('Purpose is required.');
    expect(validateRequired('Planning', 'Purpose')).toBeNull();
  });

  it('validates positive numbers', () => {
    expect(validatePositiveNumber(0, 'Capacity')).toBe('Capacity must be greater than zero.');
    expect(validatePositiveNumber(Number.NaN, 'Capacity')).toBe('Capacity must be greater than zero.');
    expect(validatePositiveNumber(4, 'Capacity')).toBeNull();
  });

  it('validates date ranges', () => {
    vi.setSystemTime(new Date('2030-01-01T10:00:00.000Z'));

    const futureStart = new Date('2030-01-01T11:00:00.000Z');
    const futureEnd = new Date('2030-01-01T12:00:00.000Z');

    expect(validateDateRange(null, futureEnd)).toBe('Please select both a start time and an end time.');
    expect(validateDateRange(new Date('invalid'), futureEnd)).toBe('Please select valid start and end times.');
    expect(validateDateRange(new Date('2030-01-01T09:00:00.000Z'), futureEnd)).toBe(
      'Start time must be in the future.'
    );
    expect(validateDateRange(futureEnd, futureStart)).toBe('End time must be after start time.');
    expect(validateDateRange(futureStart, new Date('2030-01-01T20:00:01.000Z'))).toBe(
      'A booking cannot be longer than 8 hours.'
    );
    expect(validateDateRange(futureStart, futureEnd)).toBeNull();

    vi.useRealTimers();
  });

  it('validates email addresses', () => {
    expect(validateEmail('')).toBe('Email is required.');
    expect(validateEmail('not-an-email')).toBe('Email address is invalid.');
    expect(validateEmail('user@example.com')).toBeNull();
  });
});
