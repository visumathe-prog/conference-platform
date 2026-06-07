import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Event {
  id: number;
  title: string;
  date: string;
  location: string;
  price: number;
  availableSeats: number;
  image: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard">
      <h1>Актуальні події</h1>
      <div class="events-grid">
        <div class="event-card" *ngFor="let event of events">
          <div class="event-image">{{ event.image }}</div>
          <div class="event-content">
            <h3>{{ event.title }}</h3>
            <div class="event-details">
              <span>📅 {{ event.date }}</span>
              <span>📍 {{ event.location }}</span>
              <span>🎫 {{ event.availableSeats }} місць</span>
            </div>
            <div class="event-footer">
              <span class="price">{{ event.price }} грн</span>
              <button class="register-btn" (click)="register(event.id)">Зареєструватися</button>
            </div>
          </div>
        </div>
      </div>
      <div class="more-events">
        <button class="more-btn" (click)="loadMore()">Завантажити ще</button>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }
    h1 {
      font-size: 28px;
      margin-bottom: 30px;
      color: #2196f3;
    }
    .events-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 24px;
    }
    .event-card {
      background: rgba(19, 47, 76, 0.6);
      backdrop-filter: blur(10px);
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 16px;
      overflow: hidden;
      transition: transform 0.2s;
    }
    .event-card:hover {
      transform: translateY(-4px);
      border-color: rgba(33,150,243,0.3);
    }
    .event-image {
      height: 140px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 48px;
      background: linear-gradient(135deg, #2196f3, #1565c0);
    }
    .event-content {
      padding: 20px;
    }
    .event-content h3 {
      font-size: 18px;
      margin-bottom: 12px;
      color: white;
    }
    .event-details {
      display: flex;
      flex-direction: column;
      gap: 6px;
      margin-bottom: 16px;
      font-size: 13px;
      color: rgba(255,255,255,0.7);
    }
    .event-details span {
      display: flex;
      align-items: center;
      gap: 6px;
    }
    .event-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 12px;
    }
    .price {
      font-size: 20px;
      font-weight: bold;
      color: #2196f3;
    }
    .register-btn {
      background: #2196f3;
      color: white;
      border: none;
      padding: 8px 20px;
      border-radius: 8px;
      cursor: pointer;
      transition: background 0.2s;
    }
    .register-btn:hover {
      background: #1565c0;
    }
    .more-events {
      text-align: center;
      margin-top: 40px;
    }
    .more-btn {
      background: transparent;
      border: 1px solid #2196f3;
      color: #2196f3;
      padding: 10px 30px;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s;
    }
    .more-btn:hover {
      background: rgba(33,150,243,0.1);
    }
  `]
})
export class DashboardComponent {
  events: Event[] = [
    {
      id: 1,
      title: 'Tech Conference 2023',
      date: '15-16 грудня 2023',
      location: 'Київ, ВЦ "Акваріум"',
      price: 1200,
      availableSeats: 234,
      image: '💻'
    },
    {
      id: 2,
      title: 'AI & Machine Learning Summit',
      date: '10-11 січня 2024',
      location: 'Онлайн',
      price: 850,
      availableSeats: 156,
      image: '🤖'
    },
    {
      id: 3,
      title: 'JavaScript Kyiv Meetup',
      date: '25 січня 2024',
      location: 'Київ, Creative State',
      price: 450,
      availableSeats: 45,
      image: '📜'
    },
    {
      id: 4,
      title: 'Cybersecurity Forum',
      date: '5-6 лютого 2024',
      location: 'Львів, Arena Lviv',
      price: 1500,
      availableSeats: 89,
      image: '🔒'
    },
    {
      id: 5,
      title: 'Product Management Workshop',
      date: '18 лютого 2024',
      location: 'Онлайн',
      price: 650,
      availableSeats: 200,
      image: '📊'
    },
    {
      id: 6,
      title: 'Data Science Conference',
      date: '1-2 березня 2024',
      location: 'Одеса, Морвокзал',
      price: 1100,
      availableSeats: 67,
      image: '📈'
    }
  ];

  register(eventId: number) {
    alert(`Ви зареєструвалися на подію #${eventId}. Найближчим часом ми надішлемо деталі на email.`);
  }

  loadMore() {
    alert('Буде завантажено більше подій (інтеграція з API)');
  }
}
