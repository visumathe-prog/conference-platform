import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-create',
  standalone: true,
  imports: [CommonModule],
  template: `<h2>Створити подію</h2><p>Форма створення конференції</p>`
})
export class EventCreateComponent {}
