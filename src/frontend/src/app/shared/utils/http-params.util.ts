import { HttpParams } from '@angular/common/http';
import { TimePeriod } from '../../features/transactions/models/transaction.model';

export function createHttpParams<T extends Record<string, any>>(
  filters: T = {} as T,
  pageSize?: number,
  pageNumber?: number,
  enumMap?: Partial<Record<keyof T, any>>
): HttpParams {
  const cleaned: Record<string, string | string[]> = {};

  for (const [key, value] of Object.entries(filters) as [keyof T, any][]) {
    if (value === null || value === '') {
      continue;
    }

    if (Array.isArray(value)) {
      if (value.length > 0) {
        cleaned[key as string] = value.map(v => String(v));
      }
    }
    else if (value instanceof Date) {
      cleaned[key as string] = value.toISOString();
    }
    else if (enumMap && key in enumMap && isEnumValue(enumMap[key]!, value)) {
      cleaned[key as string] = value;
    }
    else {
      cleaned[key as string] = String(value);
    }
  }

  if (pageNumber !== undefined) cleaned['pageNumber'] = String(pageNumber);
  if (pageSize !== undefined) cleaned['pageSize'] = String(pageSize);

  return new HttpParams({ fromObject: cleaned });
}

function isEnumValue<T>(enumObj: T, value: unknown): value is T[keyof T] {
  return Object.values(enumObj as Record<string, unknown>).includes(value as any);
}

