import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Modal } from '../../shared/modal/modal';
import { BuildingForm } from './building-form/building-form';

interface Building {
  id: number
  name: string
  address: string
  city: string
  totalUnits: number
  isActive: boolean
}

@Component({
  selector: 'app-buildings',
  templateUrl: './buildings.html',
  styleUrl: './buildings.css',
  imports: [CommonModule, Modal, BuildingForm]
})
export class Buildings implements OnInit {

  buildings = signal<Building[]>([])
  loading = signal(true)
  error = signal<string | null>(null)
  showModal = signal(false)

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadBuildings()
  }

  loadBuildings() {
    this.loading.set(true)
    this.error.set(null)

    this.http.get<Building[]>('http://localhost:5065/api/building').subscribe({
      next: (data) => {this.buildings.set(data); this.loading.set(false) },
      error: (err) => {this.error.set('No se pudieron cargar los edificios'); this.loading.set(false) }
    })
  }

  onBuildingSaved() {
    this.showModal.set(false)
    this.loadBuildings()
  }
}