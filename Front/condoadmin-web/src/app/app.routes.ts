import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Buildings } from './pages/buildings/buildings';
import { Units } from './pages/units/units';
import { Residents } from './pages/residents/residents';
import { Rentals } from './pages/rentals/rentals';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'buildings', component: Buildings },
  { path: 'units', component: Units },
  { path: 'residents', component: Residents },
  { path: 'rentals', component: Rentals },

  { path: '**', redirectTo: 'dashboard' }
];
