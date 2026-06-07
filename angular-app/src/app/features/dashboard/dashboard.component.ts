import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>Ласкаво просимо!</h1>
    <div class="stats">
      <div class="card">Активні події: 5</div>
      <div class="card">Учасників: 1284</div>
    </div>
  `,
  styles: [`
    .stats { display: flex; gap: 20px; margin-top: 20px; }
    .card { background: rgba(255,255,255,0.1); padding: 20px; border-radius: 12px; }
  `]
})
export class DashboardComponent {}
