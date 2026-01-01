import { HttpParams } from '@angular/common/http';

export function createHttpParams(
  filters: Record<string, any> = {},
  pageSize?: number,
  pageNumber?: number
): HttpParams {
  const cleaned: Record<string, string | string[]> = {};

  for (const [key, value] of Object.entries(filters)) {
    if (value === undefined || value === null || value === '') {
      continue;
    }
    if (Array.isArray(value)) {
      if (value.length > 0) {
        cleaned[key] = value.map(v => String(v));
      }
    }
    else if (value instanceof Date) {
      cleaned[key] = value.toISOString();
    }
    else {
      cleaned[key] = String(value);
    }
  }

  if (pageNumber !== undefined) cleaned['pageNumber'] = String(pageNumber);
  if (pageSize !== undefined) cleaned['pageSize'] = String(pageSize);

  return new HttpParams({ fromObject: cleaned });
}