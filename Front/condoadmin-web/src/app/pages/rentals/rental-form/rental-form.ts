import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Unit {
  id: number
  unitNumber: string
  floor: number
  areaM2: number
  monthlyFee: number
  status: string
  buildingName: string
}

interface RentalResult {
  tenant: string
  dni: string
  createdAt: string
  contracts: {
    contractId: number
    unitNumber: string
    buildingName: string
    startDate: string
    endDate: string
    monthlyRent: number
    depositAmount: number
    totalMonths: number
    totalContractValue: number
  }[]
}

@Component({
  selector: 'app-rental-form',
  templateUrl: './rental-form.html',
  styleUrl: './rental-form.css',
  imports: [CommonModule, ReactiveFormsModule]
})
export class RentalForm implements OnInit {

  @Output() saved  = new EventEmitter<void>()
  @Output() cancel = new EventEmitter<void>()

  availableUnits: Unit[] = []
  loading  = false
  saving   = false
  error: string | null = null
  result: RentalResult | null = null

  form: FormGroup

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.form = this.fb.group({
      firstName: ['', Validators.required],
      lastName:  ['', Validators.required],
      dni:       ['', Validators.required],
      email:     ['', [Validators.required, Validators.email]],
      phone:     ['', Validators.required],
      units:     this.fb.array([this.createUnitRow()])
    })
  }

  ngOnInit() {
    this.loadAvailableUnits()
  }

  loadAvailableUnits() {
    this.loading = true
    this.http.get<Unit[]>('http://localhost:5065/api/unit').subscribe({
      next: (data) => {
        this.availableUnits = data.filter(u => u.status === 'Available')
        this.loading = false
      },
      error: () => {
        this.error = 'No se pudieron cargar las unidades.'
        this.loading = false
      }
    })
  }

  get units(): FormArray {
    return this.form.get('units') as FormArray
  }

  get f() { return this.form.controls }

  createUnitRow(): FormGroup {
    // Fechas por defecto: hoy y en 12 meses
    const today    = new Date()
    const nextYear = new Date(today)
    nextYear.setFullYear(nextYear.getFullYear() + 1)

    return this.fb.group({
      unitNumber:    ['', Validators.required],
      startDate:     [today.toISOString().split('T')[0], Validators.required],
      endDate:       [nextYear.toISOString().split('T')[0], Validators.required],
      monthlyRent:   [0, [Validators.required, Validators.min(1)]],
      depositAmount: [0, [Validators.required, Validators.min(0)]],
      notes:         ['']
    })
  }

  addUnit() {
    this.units.push(this.createUnitRow())
  }

  removeUnit(index: number) {
    if (this.units.length > 1) {
      this.units.removeAt(index)
    }
  }

  // Auto-completa el precio mensual sugerido basado en la unidad seleccionada
  onUnitSelected(index: number, event: Event) {
    const unitNumber = (event.target as HTMLSelectElement).value
    const unit = this.availableUnits.find(u => u.unitNumber === unitNumber)

    if (unit) {
      this.units.at(index).patchValue({
        monthlyRent:   unit.monthlyFee,
        depositAmount: unit.monthlyFee * 2  // 2 meses de depósito por defecto
      })
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched()
      return
    }

    this.saving = true
    this.error  = null

    const payload = {
      firstName: this.form.value.firstName,
      lastName:  this.form.value.lastName,
      dni:       this.form.value.dni,
      email:     this.form.value.email,
      phone:     this.form.value.phone,
      units:     this.form.value.units.map((u: any) => ({
        unitNumber:    u.unitNumber,
        startDate:     u.startDate,
        endDate:       u.endDate,
        monthlyRent:   u.monthlyRent,
        depositAmount: u.depositAmount,
        notes:         u.notes
      }))
    }

    this.http.post<RentalResult>('http://localhost:5065/api/rental/trigger', payload).subscribe({
      next: (result) => {
        this.saving = false
        this.result = result
      },
      error: (err) => {
        this.saving = false
        this.error  = err.error ?? 'Error al registrar el contrato.'
        console.error(err)
      }
    })
  }

  close() {
    this.result = null
    this.saved.emit()
  }
}