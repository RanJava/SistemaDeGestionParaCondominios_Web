import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Building {
  id: number
  name: string
  city: string
  totalUnits: number
}

@Component({
  selector: 'app-unit-form',
  templateUrl: './unit-form.html',
  styleUrl: './unit-form.css',
  imports: [CommonModule, ReactiveFormsModule]
})
export class UnitForm implements OnInit {

  @Output() saved  = new EventEmitter<void>()
  @Output() cancel = new EventEmitter<void>()

  buildings: Building[] = []
  saving = false
  error: string | null = null
  form: FormGroup

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.form = this.fb.group({
      unitNumber: ['', Validators.required],
      floor:      [1,  [Validators.required, Validators.min(1)]],
      areaM2:     [0,  [Validators.required, Validators.min(1)]],
      monthlyFee: [0,  [Validators.required, Validators.min(1)]],
      buildingId: ['', Validators.required]
    })
  }

  ngOnInit() {
    this.http.get<Building[]>('http://localhost:5065/api/building').subscribe({
      next:  (data) => this.buildings = data,
      error: ()     => this.error = 'No se pudieron cargar los edificios.'
    })
  }

  get f() { return this.form.controls }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched()
      return
    }

    this.saving = true
    this.error  = null

    const payload = {
      unitNumber: this.form.value.unitNumber,
      floor:      this.form.value.floor,
      areaM2:     this.form.value.areaM2,
      monthlyFee: this.form.value.monthlyFee,
      buildingId: Number(this.form.value.buildingId)
    }

    this.http.post('http://localhost:5065/api/unit', payload).subscribe({
      next: () => {
        this.saving = false
        this.saved.emit()
      },
      error: (err) => {
        this.saving = false
        this.error  = err.error ?? 'Error al crear la unidad.'
        console.error(err)
      }
    })
  }
}