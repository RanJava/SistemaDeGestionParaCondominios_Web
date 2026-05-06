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
  status: string        // ← ahora es string Maldigo la existencia del que cambio esto: "Available", "Sold", "Rented"
  buildingName: string  
}

interface SaleResult {
  buyer: string
  dni: string
  methodOfPayment: string
  saleDate: string
  total: number
  units: { unitNumber: string; floor: number; salePrice: number; nameBuilding: string }[]
}

@Component({
  selector: 'app-sale-form',
  templateUrl: './sale-form.html',
  styleUrl: './sale-form.css',
  imports: [CommonModule, ReactiveFormsModule]
})
export class SaleForm implements OnInit {

  @Output() saved  = new EventEmitter<void>()
  @Output() cancel = new EventEmitter<void>()

  availableUnits: Unit[] = []
  loading  = false
  saving   = false
  error: string | null = null
  result: SaleResult | null = null  // Resultado exitoso de la venta

  form: FormGroup

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.form = this.fb.group({
      // Datos del comprador
      firstName:       ['', Validators.required],
      lastName:        ['', Validators.required],
      dni:             ['', Validators.required],
      email:           ['', [Validators.required, Validators.email]],
      phone:           ['', Validators.required],
      methodOfPayment: ['Efectivo', Validators.required],

      // Lista de unidades a vender — FormArray permite agregar/quitar filas
      details: this.fb.array([this.createUnitRow()])
    })
  }

  ngOnInit() {
    this.loadAvailableUnits()
  }

  loadAvailableUnits() {
  this.loading = true
  this.http.get<Unit[]>('http://localhost:5065/api/unit').subscribe({
    next: (data) => {
      // Filtramos por "Available" en lugar de status === 0
      this.availableUnits = data.filter(u => u.status === 'Available')
      this.loading = false
    },
    error: () => {
      this.error = 'No se pudieron cargar las unidades.'
      this.loading = false
    }
  })
}

  // Getter para acceder al FormArray fácilmente
  get details(): FormArray {
    return this.form.get('details') as FormArray
  }

  get f() { return this.form.controls }

  // Crea una fila vacía para agregar una unidad a la venta
  createUnitRow(): FormGroup {
    return this.fb.group({
      unitNumber:   ['', Validators.required],
      price:        [0, [Validators.required, Validators.min(1)]],
      nameBuilding: ['', Validators.required],
      notes:        ['']
    })
  }

  addUnit() {
    this.details.push(this.createUnitRow())
  }

  removeUnit(index: number) {
    if (this.details.length > 1) {
      this.details.removeAt(index)
    }
  }

  // Cuando el usuario selecciona una unidad del dropdown,
  // auto-completa el nombre del edificio
  onUnitSelected(index: number, event: Event) {
  const unitNumber = (event.target as HTMLSelectElement).value
  const unit = this.availableUnits.find(u => u.unitNumber === unitNumber)

  if (unit) {
    this.details.at(index).patchValue({
      nameBuilding: unit.buildingName  // ← era unit.building.name
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
      firstName:       this.form.value.firstName,
      lastName:        this.form.value.lastName,
      dni:             this.form.value.dni,
      email:           this.form.value.email,
      phone:           this.form.value.phone,
      methodOfPayment: this.form.value.methodOfPayment,
      details:         this.form.value.details
    }

    this.http.post<SaleResult>('http://localhost:5065/api/sales', payload).subscribe({
      next: (result) => {
        this.saving = false
        this.result = result  // Mostramos el resumen de la venta
      },
      error: (err) => {
        this.saving = false
        this.error  = err.error ?? 'Error al registrar la venta.'
        console.error(err)
      }
    })
  }

  close() {
    this.result = null
    this.saved.emit()
  }
}