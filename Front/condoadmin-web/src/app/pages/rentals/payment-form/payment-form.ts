import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface PaymentResult {
  contractId: number
  tenantName: string
  unitNumber: string
  amountReceived: number
  amountApplied: number
  newCreditBalance: number
  periodsPaid: string[]
  message: string
}

@Component({
  selector: 'app-payment-form',
  templateUrl: './payment-form.html',
  styleUrl: './payment-form.css',
  imports: [CommonModule, ReactiveFormsModule]
})
export class PaymentForm {

  // El contrato al que se le aplicará el pago
  @Input() contractId!: number
  @Input() tenantName!: string
  @Input() unitNumber!: string
  @Input() monthlyRent!: number

  @Output() saved  = new EventEmitter<void>()
  @Output() cancel = new EventEmitter<void>()

  saving = false
  error: string | null = null
  result: PaymentResult | null = null

  form: FormGroup

  paymentMethods = ['Efectivo', 'Transferencia', 'Cheque', 'QR']

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.form = this.fb.group({
      amount:        [0, [Validators.required, Validators.min(1)]],
      paymentMethod: ['Efectivo', Validators.required],
      notes:         ['']
    })
  }

  get f() { return this.form.controls }

  // Botones de acceso rápido para montos comunes
  setAmount(months: number) {
    this.form.patchValue({ amount: this.monthlyRent * months })
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched()
      return
    }

    this.saving = true
    this.error  = null

    const payload = {
      amount:        this.form.value.amount,
      paymentMethod: this.form.value.paymentMethod,
      notes:         this.form.value.notes
    }

    this.http.post<PaymentResult>(
      `http://localhost:5065/api/rental/${this.contractId}/pay`,
      payload
    ).subscribe({
      next: (result) => {
        this.saving = false
        this.result = result
      },
      error: (err) => {
        this.saving = false
        this.error  = err.error ?? 'Error al registrar el pago.'
        console.error(err)
      }
    })
  }

  close() {
    this.result = null
    this.saved.emit()
  }
}