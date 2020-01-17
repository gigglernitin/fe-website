import { Routes } from '@angular/router';

import { MediaComponent } from './media/media.component';
import { MessagesComponent } from './messages/messages.component';
import { UserListComponent } from './users/user-list/user-list.component';
import { UserDetailComponent } from './users/user-detail/user-detail.component';
import { HomeComponent } from './home/home.component';

import { AuthGuard } from './_guards/auth.guard';

import { UserListResolver } from './_resolvers/user-list.resolver';
import { UserDetailResolver } from './_resolvers/user-detail.resolver';

export const routes: Routes = [
  {
    path: '',
    children: [
      { path: '', component: HomeComponent },
      { path: 'home', component: HomeComponent },
    ]
  },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      { path: 'users', component: UserListComponent, resolve: {users: UserListResolver} },
      { path: 'users/:id', component: UserDetailComponent, resolve: {user: UserDetailResolver} },
      { path: 'messages', component: MessagesComponent },
      { path: 'media', component: MediaComponent },
    ]
  },
  { path: '**', redirectTo: 'home', pathMatch: 'full' },
];