import { HttpParams } from '@angular/common/http';

export function createHttpParams(
  filters: Record<string, any> = {},
  pageNumber?: number,
  pageSize?: number
): HttpParams {

  const cleaned: Record<string, string> = {};

  for (const [key, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null) {
      cleaned[key] =
        value instanceof Date
          ? value.toISOString()
          : String(value);
    }
  }

  if (pageNumber !== undefined) cleaned['pageNumber'] = String(pageNumber);
  if (pageSize !== undefined) cleaned['pageSize'] = String(pageSize);

  return new HttpParams({ fromObject: cleaned });
}
