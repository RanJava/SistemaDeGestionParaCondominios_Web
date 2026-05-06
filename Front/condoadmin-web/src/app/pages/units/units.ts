import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Modal } from '../../shared/modal/modal';
import { SaleForm } from './sale-form/sale-form';
import { UnitForm } from './unit-form/unit-form';

interface Unit {
  id: number
  unitNumber: string
  floor: number
  areaM2: number
  monthlyFee: number
  status: string
  buildingId: number
  buildingName: string
}

interface Building {
  id: number
  name: string
}

@Component({
  selector: 'app-units',
  templateUrl: './units.html',
  styleUrl: './units.css',
  imports: [CommonModule, FormsModule, Modal, SaleForm, UnitForm]
})
export class Units implements OnInit {

  units         = signal<Unit[]>([])
  buildings     = signal<Building[]>([])
  loading       = signal(true)
  error         = signal<string | null>(null)
  showSaleModal = signal(false)
  showUnitModal = signal(false)

  selectedBuildingName = signal<string | null>(null)
  selectedStatus     = signal<string | null>(null)

  filteredUnits = computed(() => {
  let result = this.units()

  const buildingName = this.selectedBuildingName()
  if (buildingName !== null) {
    result = result.filter(u => u.buildingName === buildingName)
  }

  const status = this.selectedStatus()
  if (status !== null) {
    result = result.filter(u => u.status === status)
  }

  return result
})

  statusLabels: Record<string, string> = {
    'Available': 'Disponible',
    'Sold':      'Vendida',
    'Rented':    'Alquilada'
  }

  statusColors: Record<string, string> = {
    'Available': 'available',
    'Sold':      'sold',
    'Rented':    'rented'
  }

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadUnits()
    this.loadBuildings()
  }

  loadUnits() {
  this.loading.set(true)
  this.http.get<Unit[]>('http://localhost:5065/api/unit').subscribe({
    next: (data) => {
      console.log('Unidades:', data.map(u => ({ num: u.unitNumber, buildingId: u.buildingId, buildingName: u.buildingName })))
      this.units.set(data)
      this.loading.set(false)
    },
    error: () => {
      this.error.set('No se pudieron cargar las unidades.')
      this.loading.set(false)
    }
  })
}

  loadBuildings() {
    this.http.get<Building[]>('http://localhost:5065/api/building').subscribe({
      next: (data) => this.buildings.set(data),
      error: (err)  => console.error(err)
    })
  }

  filterByBuilding(event: Event) {
  const value = (event.target as HTMLSelectElement).value
  this.selectedBuildingName.set(value === '' ? null : value)
}

  filterByStatus(event: Event) {
    const value = (event.target as HTMLSelectElement).value
    this.selectedStatus.set(value === '' ? null : value)
  }

  clearFilters() {
  this.selectedBuildingName.set(null)
  this.selectedStatus.set(null)
}

  onSaleSaved() {
    this.showSaleModal.set(false)
    this.loadUnits()
  }

  onUnitSaved() {
    this.showUnitModal.set(false)
    this.loadUnits()
  }
}