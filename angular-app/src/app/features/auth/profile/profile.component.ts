import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  template: `<h2>Мій профіль</h2><p>Інформація про користувача</p>`
})
export class ProfileComponent {}
