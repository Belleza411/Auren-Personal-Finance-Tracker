import { inject, Injectable } from "@angular/core";
import { apiUrl } from "../../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Category, CategoryFilter, CategoryOverview, CategorySummary, NewCategory } from "../models/categories.model";
import { Observable } from "rxjs";
import { createHttpParams } from "../../../shared/utils/http-params.util";

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
    private readonly baseUrl = `${apiUrl}/api/categories`;
    private http = inject(HttpClient);

    getAllCategories(
        filters?: Partial<CategoryFilter>,
        pageSize: number = 5,
        pageNumber: number = 1
    ): Observable<Category[]> {
        const params = createHttpParams(filters, pageSize, pageNumber);

        return this.http.get<Category[]>(this.baseUrl, { params });
    }

    getCategoryById(id: string): Observable<Category> {
        return this.http.get<Category>(`${this.baseUrl}/${id}`);
    }

    createCategory(data: NewCategory): Observable<Category> {
        return this.http.post<Category>(this.baseUrl, data);
    }

    updateCategory(id: string, data: Partial<NewCategory>): Observable<Category> {
        return this.http.put<Category>(`${this.baseUrl}/${id}`, data);
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    getCategoriesOverview(filters?: Partial<CategoryFilter>,
        pageSize: number = 5,
        pageNumber: number = 1
    ): Observable<CategoryOverview[]> {
        const params = createHttpParams(filters, pageSize, pageNumber);

        return this.http.get<CategoryOverview[]>(`${this.baseUrl}/overview`, { params });
    }

    getCategorySummary(): Observable<CategorySummary> {
        return this.http.get<CategorySummary>(`${this.baseUrl}/summary`);
    }
} 