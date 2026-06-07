import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule],
  template: `<h2>Деталі події</h2><p>Інформація про конференцію</p>`
})
export class EventDetailComponent {}
