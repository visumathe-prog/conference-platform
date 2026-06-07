import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry, timeout } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface AiMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
}

export interface AiAssistantResponse {
  sessionId: string;
  messages: AiMessage[];
  suggestions?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AiAssistantService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.aiApiUrl || '/api/ai';
  
  /**
   * Open AI Assistant and initialize conversation
   * Calls external REST API endpoint (Python/OpenAI tool)
   */
  openAssistant(context?: string): Observable<AiAssistantResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'X-Client-Type': 'angular-web'
    });
    
    const payload = {
      action: 'init',
      context: context || 'conference_platform',
      userId: localStorage.getItem('userId'),
      timestamp: new Date().toISOString()
    };
    
    return this.http.post<AiAssistantResponse>(`${this.baseUrl}/assistant/start`, payload, { headers })
      .pipe(
        timeout(30000), // 30 second timeout
        retry(2),
        catchError(this.handleError)
      );
  }
  
  /**
   * Send message to AI assistant
   */
  sendMessage(sessionId: string, message: string): Observable<AiAssistantResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'X-Session-Id': sessionId
    });
    
    const payload = {
      sessionId,
      message,
      timestamp: new Date().toISOString()
    };
    
    return this.http.post<AiAssistantResponse>(`${this.baseUrl}/assistant/message`, payload, { headers })
      .pipe(
        timeout(30000),
        retry(1),
        catchError(this.handleError)
      );
  }
  
  /**
   * Get AI suggestions for current context
   */
  getSuggestions(context: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/assistant/suggestions?context=${context}`)
      .pipe(
        timeout(10000),
        catchError(() => {
          // Fallback suggestions
          return [
            'How to create a new event?',
            'View my upcoming conferences',
            'Generate certificate for attendees',
            'Analyze event engagement metrics'
          ];
        })
      );
  }
  
  /**
   * Error handler for API calls
   */
  private handleError(error: any): Observable<never> {
    let errorMessage = 'An error occurred while communicating with AI service';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Client Error: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 0:
          errorMessage = 'AI service is unavailable. Please check your connection.';
          break;
        case 401:
          errorMessage = 'Authentication required for AI service';
          break;
        case 429:
          errorMessage = 'Too many requests. Please try again later.';
          break;
        case 503:
          errorMessage = 'AI service is currently warming up. Please try again.';
          break;
        default:
          errorMessage = `Server Error: ${error.status} - ${error.message}`;
      }
    }
    
    console.error('AI Assistant Error:', errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }
}
