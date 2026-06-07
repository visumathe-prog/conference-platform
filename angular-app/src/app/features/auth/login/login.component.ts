import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-container">
      <h2>Вхід</h2>
      <input [(ngModel)]="email" placeholder="Email" type="email">
      <input [(ngModel)]="password" placeholder="Пароль" type="password">
      <button (click)="login()">Увійти</button>
    </div>
  `,
  styles: [`
    .login-container { max-width: 400px; margin: 50px auto; padding: 20px; background: rgba(255,255,255,0.1); border-radius: 12px; }
    input { display: block; width: 100%; margin: 10px 0; padding: 10px; }
    button { width: 100%; padding: 10px; background: #2196f3; color: white; border: none; border-radius: 8px; }
  `]
})
export class LoginComponent {
  email = '';
  password = '';
  constructor(private auth: AuthService) {}
  login() { this.auth.login(this.email, this.password).subscribe(); }
}
