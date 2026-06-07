import { Component, Inject, signal, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AiAssistantService, AiMessage } from '../../../core/services/ai-assistant.service';

interface DialogData {
  messages?: AiMessage[];
  sessionId?: string;
  error?: string;
}

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './ai-assistant.component.html',
  styleUrls: ['./ai-assistant.component.scss']
})
export class AiAssistantComponent implements AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  
  messages = signal<AiMessage[]>([]);
  sessionId = signal<string | null>(null);
  isProcessing = signal(false);
  userInput = signal('');
  error = signal<string | null>(null);
  
  constructor(
    private dialogRef: MatDialogRef<AiAssistantComponent>,
    @Inject(MAT_DIALOG_DATA) private data: DialogData,
    private aiService: AiAssistantService
  ) {
    if (data.error) {
      this.error.set(data.error);
    } else {
      this.messages.set(data.messages || this.getDefaultMessages());
      this.sessionId.set(data.sessionId || null);
    }
  }
  
  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }
  
  /**
   * Send user message to AI
   */
  sendMessage(): void {
    const message = this.userInput().trim();
    if (!message || this.isProcessing()) return;
    
    // Add user message to chat
    const userMessage: AiMessage = {
      role: 'user',
      content: message,
      timestamp: new Date()
    };
    this.messages.update(msgs => [...msgs, userMessage]);
    this.userInput.set('');
    this.isProcessing.set(true);
    
    // Send to AI service
    const sessionId = this.sessionId();
    const request = sessionId 
      ? this.aiService.sendMessage(sessionId, message)
      : this.aiService.openAssistant(message);
    
    request.subscribe({
      next: (response) => {
        this.sessionId.set(response.sessionId);
        this.messages.update(msgs => [...msgs, ...response.messages]);
        this.isProcessing.set(false);
        this.error.set(null);
      },
      error: (err) => {
        console.error('AI response error:', err);
        this.messages.update(msgs => [
          ...msgs,
          {
            role: 'assistant',
            content: 'I apologize, but I encountered an error. Please try again later.',
            timestamp: new Date()
          }
        ]);
        this.isProcessing.set(false);
        this.error.set(err.message);
      }
    });
  }
  
  /**
   * Handle Enter key press
   */
  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
  
  /**
   * Close dialog
   */
  close(): void {
    this.dialogRef.close();
  }
  
  /**
   * Get default welcome messages
   */
  private getDefaultMessages(): AiMessage[] {
    return [
      {
        role: 'assistant',
        content: '👋 Hello! I\'m your Conference AI Assistant. I can help you with:\n\n' +
          '• Finding and registering for events\n' +
          '• Managing your conference schedule\n' +
          '• Answering questions about speakers and sessions\n' +
          '• Generating attendance certificates\n' +
          '• Providing insights about past conferences\n\n' +
          'How can I assist you today?',
        timestamp: new Date()
      }
    ];
  }
  
  /**
   * Scroll messages container to bottom
   */
  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const element = this.messagesContainer.nativeElement;
      element.scrollTop = element.scrollHeight;
    }
  }
}
