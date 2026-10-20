import { Routes } from '@angular/router';
import { NotFoundComponent } from './core/layout/not-found.component';
import { ShellComponent } from './core/layout/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'shifts' },
      { path: '**', component: NotFoundComponent },
    ],
  },
];
