import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

// Interface actualizada para coincidir con ListResidentsOutput del backend
interface Resident {
  id: number
  fullName: string
  dni: string
  phone: string
  email: string
  unitNumber: string  // "Sin unidad" si no tiene
}

@Component({
  selector: 'app-residents',
  templateUrl: './residents.html',
  styleUrl: './residents.css',
  imports: [CommonModule, FormsModule]
})
export class Residents implements OnInit {

  residents = signal<Resident[]>([])
  loading   = signal(true)
  error     = signal<string | null>(null)
  search    = signal('')

  filteredResidents = computed(() => {
    const term = this.search().toLowerCase().trim()
    if (!term) return this.residents()

    return this.residents().filter(r =>
      r.fullName.toLowerCase().includes(term) ||
      r.dni.includes(term)
    )
  })

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadResidents()
  }

  loadResidents() {
    this.loading.set(true)
    this.error.set(null)

    this.http.get<Resident[]>('http://localhost:5065/api/resident').subscribe({
      next: (data) => {
        this.residents.set(data)
        this.loading.set(false)
      },
      error: (err) => {
        this.error.set('No se pudieron cargar los residentes.')
        this.loading.set(false)
        console.error(err)
      }
    })
  }

  updateSearch(event: Event) {
    this.search.set((event.target as HTMLInputElement).value)
  }

  // Extrae iniciales del fullName — toma primera letra de cada palabra
  getInitials(fullName: string): string {
    return fullName
      .split(' ')
      .slice(0, 2)
      .map(n => n.charAt(0))
      .join('')
      .toUpperCase()
  }
}