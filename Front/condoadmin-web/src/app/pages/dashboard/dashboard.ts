import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface DashboardData {
  buildings:   { total: number; active: number }
  units:       { total: number; available: number; sold: number; rented: number }
  residents:   { total: number; active: number }
  payments:    { pendingCount: number; totalDebt: number }
  maintenance: { pendingCount: number; resolvedCount: number }
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  imports: [CommonModule, RouterLink]
})
export class Dashboard implements OnInit {

  data    = signal<DashboardData | null>(null)
  loading = signal(true)
  error   = signal<string | null>(null)

  // Ciudad — dato local, no viene de la API
  city = 'La Paz, Bolivia'

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<DashboardData>('http://localhost:5065/api/dashboard').subscribe({
      next:  (d) => { this.data.set(d); this.loading.set(false) },
      error: ()  => { this.error.set('No se pudo cargar el dashboard.'); this.loading.set(false) }
    })
  }
}