import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="register-container">
      <h2>Реєстрація</h2>
      <input [(ngModel)]="email" placeholder="Email">
      <input [(ngModel)]="password" placeholder="Пароль" type="password">
      <input [(ngModel)]="firstName" placeholder="Ім'я">
      <input [(ngModel)]="lastName" placeholder="Прізвище">
      <button (click)="register()">Зареєструватися</button>
    </div>
  `,
  styles: [`
    .register-container { max-width: 400px; margin: 50px auto; padding: 20px; background: rgba(255,255,255,0.1); border-radius: 12px; }
    input { display: block; width: 100%; margin: 10px 0; padding: 10px; }
    button { width: 100%; padding: 10px; background: #2196f3; color: white; border: none; border-radius: 8px; }
  `]
})
export class RegisterComponent {
  email = ''; password = ''; firstName = ''; lastName = '';
  constructor(private auth: AuthService) {}
  register() { this.auth.register({ email: this.email, password: this.password, firstName: this.firstName, lastName: this.lastName }).subscribe(); }
}
