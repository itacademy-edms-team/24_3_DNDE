import AddTaskIcon from '@mui/icons-material/AddTask';
import GitHubIcon from '@mui/icons-material/GitHub';
import HomeIcon from '@mui/icons-material/Home';
import TerrainIcon from '@mui/icons-material/Terrain';
import CategoryIcon from '@mui/icons-material/Category';

import asyncComponentLoader from '@/utils/loader';

import { Routes } from './types';

const routes: Routes = [
  {
    component: asyncComponentLoader(() => import('@/pages/Welcome')),
    path: '/',
    title: 'Добро пожаловать',
    icon: HomeIcon,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/AccountPage')),
    path: '/account',
    title: 'Аккаунт',
    icon: GitHubIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/WalletsPage')),
    path: '/wallets',
    title: 'Кошельки',
    icon: AddTaskIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/WalletPage')),
    path: '/wallets/:walletId',
    title: '',
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/WalletsArchivePage')),
    path: '/wallets-archive',
    title: 'Кошельки (Архив)',
    icon: AddTaskIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/GeneralAnalyticsPage')),
    path: '/analytics',
    title: 'Аналитика',
    icon: TerrainIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/CategoriesPage')),
    path: '/categories',
    title: 'Категории',
    icon: CategoryIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/NotFound')),
    path: '*',
  },
];

export default routes;
