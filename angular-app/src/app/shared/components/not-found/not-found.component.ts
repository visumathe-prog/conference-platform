import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="not-found">
      <div class="code">404</div>
      <h2>Сторінку не знайдено</h2>
      <p>На жаль, такої сторінки не існує.</p>
      <a routerLink="/dashboard" class="home-link">Повернутися на головну</a>
    </div>
  `,
  styles: [`
    .not-found {
      text-align: center;
      padding: 80px 20px;
      max-width: 500px;
      margin: 0 auto;
    }
    .code {
      font-size: 80px;
      font-weight: bold;
      color: #2196f3;
      margin-bottom: 20px;
    }
    h2 {
      font-size: 28px;
      margin-bottom: 16px;
    }
    p {
      color: rgba(255,255,255,0.7);
      margin-bottom: 30px;
    }
    .home-link {
      display: inline-block;
      background: #2196f3;
      color: white;
      padding: 12px 24px;
      border-radius: 8px;
      text-decoration: none;
    }
  `]
})
export class NotFoundComponent {}
