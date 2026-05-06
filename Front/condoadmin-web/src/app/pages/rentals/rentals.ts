import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Modal } from '../../shared/modal/modal';
import { RentalForm } from './rental-form/rental-form';
import { PaymentForm } from './payment-form/payment-form';

interface Rental {
  id: number
  tenantName: string
  tenantDNI: string
  unitNumber: string
  buildingName: string
  startDate: string
  endDate: string
  monthlyRent: number
  depositAmount: number
  creditBalance: number
  status: string
  pendingMonths: number
  totalDebt: number
}

@Component({
  selector: 'app-rentals',
  templateUrl: './rentals.html',
  styleUrl: './rentals.css',
  imports: [CommonModule, FormsModule, Modal, RentalForm, PaymentForm]
})
export class Rentals implements OnInit {

  rentals           = signal<Rental[]>([])
  loading           = signal(true)
  error             = signal<string | null>(null)
  filter            = signal<string>('all')
  showModal         = signal(false)
  showPaymentModal  = signal(false)

  // Contrato seleccionado para pagar
  selectedRental: Rental | null = null

  filteredRentals = computed(() => {
  const f = this.filter()
  if (f === 'all') return this.rentals()
  if (f === 'terminated') {
    return this.rentals().filter(r =>
      r.status.toLowerCase() === 'terminated' ||
      r.status.toLowerCase() === 'cancelled' ||
      (r.status === 'Active' && new Date(r.endDate) < new Date())
    )
  }
  if (f === 'active') {
    return this.rentals().filter(r =>
      r.status === 'Active' && new Date(r.endDate) >= new Date()
    )
  }
  return this.rentals()
})

  totalDebt = computed(() =>
    this.rentals()
      .filter(r => r.status === 'Active')
      .reduce((sum, r) => sum + r.totalDebt, 0)
  )

  totalCreditBalance = computed(() =>
    this.rentals()
      .filter(r => r.status === 'Active')
      .reduce((sum, r) => sum + r.creditBalance, 0)
  )

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadRentals()
  }

  loadRentals() {
    this.loading.set(true)
    this.error.set(null)

    this.http.get<Rental[]>('http://localhost:5065/api/rental').subscribe({
      next: (data) => {
        this.rentals.set(data)
        this.loading.set(false)
      },
      error: () => {
        this.error.set('No se pudieron cargar los contratos.')
        this.loading.set(false)
      }
    })
  }

  setFilter(f: string) {
    this.filter.set(f)
  }

  // Abre el modal de pago con el contrato seleccionado
  openPayment(rental: Rental) {
    this.selectedRental = rental
    this.showPaymentModal.set(true)
  }

  onRentalSaved() {
    this.showModal.set(false)
    this.loadRentals()
  }

  onPaymentSaved() {
    this.showPaymentModal.set(false)
    this.selectedRental = null
    this.loadRentals()
  }

  getContractProgress(startDate: string, endDate: string): number {
    const start = new Date(startDate).getTime()
    const end   = new Date(endDate).getTime()
    const now   = Date.now()

    if (now >= end)   return 100
    if (now <= start) return 0

    return Math.round(((now - start) / (end - start)) * 100)
  }

  getStatusLabel(status: string, endDate: string): string {
  if (status === 'Active' && new Date(endDate) < new Date()) {
    return 'Vencido'
  }
  const labels: Record<string, string> = {
    'Active':     'Activo',
    'Terminated': 'Terminado',
    'Cancelled':  'Cancelado'
  }
  return labels[status] ?? status
}
isExpired(status: string, endDate: string): boolean {
  return status === 'Active' && new Date(endDate) < new Date()
}
}