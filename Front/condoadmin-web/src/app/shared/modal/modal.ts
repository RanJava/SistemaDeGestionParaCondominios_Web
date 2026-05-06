import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.html',
  styleUrls: ['./modal.css'],
  imports: [CommonModule]
})

export class Modal {
  @Input() title: string = ''
  @Input() isOpen: boolean = false
  @Output() closed = new EventEmitter<void>()

  close() {
    this.closed.emit()
  }

  onBackdropClick(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.close()
    }
  }
}