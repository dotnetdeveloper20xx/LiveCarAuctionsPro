import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/auctions/auction-list/auction-list.component')
      .then(m => m.AuctionListComponent),
    title: 'Live Auctions - CarAuctions'
  },
  {
    path: 'auctions',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/auctions/auction-list/auction-list.component')
          .then(m => m.AuctionListComponent),
        title: 'Live Auctions - CarAuctions'
      },
      {
        path: ':id',
        loadComponent: () => import('./features/auctions/auction-detail/auction-detail.component')
          .then(m => m.AuctionDetailComponent),
        title: 'Auction Details - CarAuctions'
      }
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component')
      .then(m => m.LoginComponent),
    title: 'Login - CarAuctions'
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component')
      .then(m => m.RegisterComponent),
    title: 'Register - CarAuctions'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.DashboardComponent),
    title: 'Dashboard - CarAuctions'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
