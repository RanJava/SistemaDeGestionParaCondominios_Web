import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Building , CreateBuilding } from '../interfaces/building.interface';

@Injectable({
  providedIn: 'root',
})
export class BuildingService {
  private http = inject(HttpClient)
  private apiUrl = 'http://localhost:5065/api/building'

  getAll(): Observable<Building[]> {
    return this.http.get<Building[]>(this.apiUrl);
  }

  getById(id: number): Observable<Building> {
    return this.http.get<Building>(`${this.apiUrl}/${id}`);
  }

  create(building: CreateBuilding): Observable<Building> {
    return this.http.post<Building>(this.apiUrl, building);
  }

  update(id: number, building: Building): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, building);
  }
}
