import { inject, Injectable } from "@angular/core";
import { apiUrl } from "../../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { Goal, GoalFilter, GoalsSummary, NewGoal } from "../models/goals.model";
import { createHttpParams } from "../../../shared/utils/http-params.util";
import { PagedResult } from "../../transactions/models/transaction.model";

@Injectable({
  providedIn: 'root',
})
export class GoalService {
    private readonly baseUrl = `${apiUrl}/api/goals`;
    private http = inject(HttpClient);

    getAllGoals(
        filters?: Partial<GoalFilter>, 
        pageNumber: number = 1, 
        pageSize: number = 5
    ): Observable<PagedResult<Goal>> {
        const params = createHttpParams(filters, pageSize, pageNumber);

        return this.http.get<PagedResult<Goal>>(this.baseUrl, { params });
    }

    getGoalById(id: string): Observable<Goal> {
        return this.http.get<Goal>(`${this.baseUrl}/${id}`);
    }

    createGoal(data: NewGoal): Observable<Goal> {
        return this.http.post<Goal>(this.baseUrl, data);
    }

    updateGoal(id: string, data: Partial<NewGoal>): Observable<Goal> {
        return this.http.put<Goal>(`${this.baseUrl}/${id}`, data);
    }

    deleteGoal(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    getGoalsSummary(): Observable<GoalsSummary> {
        return this.http.get<GoalsSummary>(`${this.baseUrl}/summary`);
    }

    addMoneyToGoal(amount: number, id: string): Observable<any> {
        return this.http.put<any>(`${this.baseUrl}/${id}/add-money`, amount);
    }
}