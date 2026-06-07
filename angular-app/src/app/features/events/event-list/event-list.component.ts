import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule],
  template: `<h2>Список подій</h2><p>Тут будуть конференції</p>`
})
export class EventListComponent {}
