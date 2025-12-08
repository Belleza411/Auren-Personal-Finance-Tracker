import { inject, Injectable } from "@angular/core";
import { apiUrl } from "../../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { Goal, GoalFilter, NewGoal } from "../models/goals.model";

@Injectable({
  providedIn: 'root',
})
export class GoalService {
    private readonly baseUrl = `${apiUrl}/api/goals`;
    private http = inject(HttpClient);

    getAllGoals(
        filters: GoalFilter, 
        pageNumber: number = 1, 
        pageSize: number = 5
    ): Observable<Goal> {
        let params = new HttpParams()
            .set('pageNumber', pageNumber.toString())
            .set('pageSize', pageSize.toString());  
    
        Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                params = params.set(key, value.toString());
            }
        });

        return this.http.get<Goal>(apiUrl, { params });
    }

    getGoalById(id: string): Observable<Goal> {
        return this.http.get<Goal>(`${this.baseUrl}/${id}`);
    }

    createGoal(data: NewGoal): Observable<Goal> {
        return this.http.post<Goal>(apiUrl, data);
    }

    updateGoal(id: string, data: Partial<NewGoal>): Observable<Goal> {
        return this.http.put<Goal>(`${apiUrl}/${id}`, data);
    }

    deleteGoal(id: string): Observable<void> {
        return this.http.delete<void>(`${apiUrl}/${id}`);
    }
}