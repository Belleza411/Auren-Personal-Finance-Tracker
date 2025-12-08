import { inject, Injectable } from "@angular/core";
import { apiUrl } from "../../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Category, CategoryFilter, CreateCategory } from "../models/categories.model";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
    private readonly baseUrl = `${apiUrl}/api/categories`;
    private http = inject(HttpClient);

    getAllCategories(
        filters: Partial<CategoryFilter>,
        pageSize: number = 5,
        pageNumber: number = 1
    ): Observable<Category> {
        let params = new HttpParams()
            .set('pageNumber', pageNumber.toString())
            .set('pageSize', pageSize.toString());  
    
        Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                params = params.set(key, value.toString());
            }
        });

        return this.http.get<Category>(this.baseUrl, { params });
    }

    getCategoryById(id: string): Observable<Category> {
        return this.http.get<Category>(`${this.baseUrl}/${id}`);
    }

    createCategory(data: CreateCategory): Observable<Category> {
        return this.http.post<Category>(this.baseUrl, data);
    }

    updateCategory(id: string, data: Partial<CreateCategory>): Observable<Category> {
        return this.http.put<Category>(`${this.baseUrl}/${id}`, data);
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
} 