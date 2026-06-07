import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-gdpr',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './gdpr.component.html',
  styleUrls: ['./gdpr.component.scss']
})
export class GdprComponent {
  showPrivacyModal = false;
  showImprintModal = false;
  showCookieSettings = false;

  openPrivacy() {
    this.showPrivacyModal = true;
  }

  openImprint() {
    this.showImprintModal = true;
  }

  openCookieSettings() {
    this.showCookieSettings = true;
  }

  closeModals() {
    this.showPrivacyModal = false;
    this.showImprintModal = false;
    this.showCookieSettings = false;
  }

  acceptAllCookies() {
    localStorage.setItem('cookieConsent', JSON.stringify({
      essential: true,
      functional: true,
      analytics: true,
      marketing: true,
      timestamp: new Date().toISOString()
    }));
    this.closeModals();
  }

  acceptEssentialOnly() {
    localStorage.setItem('cookieConsent', JSON.stringify({
      essential: true,
      functional: false,
      analytics: false,
      marketing: false,
      timestamp: new Date().toISOString()
    }));
    this.closeModals();
  }
}
