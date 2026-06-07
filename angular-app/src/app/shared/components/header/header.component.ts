import { Component, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatBadgeModule } from '@angular/material/badge';

import { AuthService } from '../../../core/services/auth.service';
import { AiAssistantService } from '../../../core/services/ai-assistant.service';
import { AiAssistantComponent } from '../ai-assistant/ai-assistant.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatSidenavModule,
    MatBadgeModule
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  // Reactive state using signals
  isMenuOpen = signal(false);
  isAiLoading = signal(false);
  notificationCount = signal(3);
  
  constructor(
    public authService: AuthService,
    private aiAssistantService: AiAssistantService,
    private dialog: MatDialog
  ) {}
  
  /**
   * Toggle mobile menu
   */
  toggleMenu(): void {
    this.isMenuOpen.update(value => !value);
  }
  
  /**
   * Open AI Assistant modal with REST API call
   */
  openAiAssistant(): void {
    this.isAiLoading.set(true);
    
    // Call external AI Tool REST API
    this.aiAssistantService.openAssistant().subscribe({
      next: (response) => {
        this.isAiLoading.set(false);
        
        // Open dialog with AI response
        this.dialog.open(AiAssistantComponent, {
          width: '600px',
          maxWidth: '90vw',
          maxHeight: '80vh',
          data: {
            messages: response.messages || [],
            sessionId: response.sessionId
          },
          panelClass: 'ai-assistant-dialog'
        });
      },
      error: (error) => {
        this.isAiLoading.set(false);
        console.error('AI Assistant error:', error);
        
        // Fallback dialog with error message
        this.dialog.open(AiAssistantComponent, {
          width: '500px',
          data: {
            error: 'AI service is currently unavailable. Please try again later.',
            messages: []
          }
        });
      }
    });
  }
  
  /**
   * Logout user
   */
  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        // Redirect handled in service
      },
      error: (error) => {
        console.error('Logout error:', error);
      }
    });
  }
  
  /**
   * Handle window resize for responsive behavior
   */
  @HostListener('window:resize', ['$event'])
  onResize(event: Event): void {
    const width = (event.target as Window).innerWidth;
    if (width > 768 && this.isMenuOpen()) {
      this.isMenuOpen.set(false);
    }
  }
}
