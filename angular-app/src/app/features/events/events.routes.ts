import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

export const EVENT_ROUTES: Routes = [
    { path: '', loadComponent: () => import('./event-list/event-list.component').then(m => m.EventListComponent) },
    { path: 'id', loadComponent: () => import('./event-detail/event-detail.component').then(m => m.EventDetailComponent) }
];
