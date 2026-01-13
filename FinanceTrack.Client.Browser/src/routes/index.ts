import AddTaskIcon from '@mui/icons-material/AddTask';
import BugReportIcon from '@mui/icons-material/BugReport';
import GitHubIcon from '@mui/icons-material/GitHub';
import HomeIcon from '@mui/icons-material/Home';
import TerrainIcon from '@mui/icons-material/Terrain';

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
    component: asyncComponentLoader(() => import('@/pages/TransactionsPage')),
    path: '/transactions',
    title: 'Транзакции',
    icon: AddTaskIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/AnalyticsPage')),
    path: '/analytics',
    title: 'Аналитика',
    icon: TerrainIcon,
    protected: true,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/Page4')),
    path: '/page-4',
    title: 'Page 4',
    icon: BugReportIcon,
  },
  {
    component: asyncComponentLoader(() => import('@/pages/NotFound')),
    path: '*',
  },
];

export default routes;
