import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-building-form',
  templateUrl: './building-form.html',
  styleUrl: './building-form.css',
  imports: [CommonModule, ReactiveFormsModule]
})
export class BuildingForm {

  @Output() saved  = new EventEmitter<void>()
  @Output() cancel = new EventEmitter<void>()

  saving = false
  error: string | null = null
  form: FormGroup

  constructor(private fb: FormBuilder, private http: HttpClient) {
    // El form se inicializa aquí porque necesita fb ya inyectado
    this.form = this.fb.group({
      name:       ['', [Validators.required, Validators.minLength(3)]],
      address:    ['', [Validators.required]],
      city:       ['La Paz', [Validators.required]],
      totalUnits: [0, [Validators.required, Validators.min(0)]]
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

    this.http.post('http://localhost:5065/api/building', this.form.value)
      .subscribe({
        next: () => {
          this.saving = false
          this.saved.emit()
        },
        error: (err) => {
          this.saving = false
          this.error  = 'Error al guardar el edificio. Intenta de nuevo.'
          console.error(err)
        }
      })
  }
}