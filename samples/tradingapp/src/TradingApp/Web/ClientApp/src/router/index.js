import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/', name: 'Home', component: () => import('../pages/Home.vue'), meta: { tab: 'home' } },
  { path: '/search', name: 'Search', component: () => import('../pages/Search.vue'), meta: { tab: 'search' } },
  { path: '/notifications', name: 'Notifications', component: () => import('../pages/Notifications.vue'), meta: { tab: 'notifications' } },
  { path: '/monitoring', name: 'Monitoring', component: () => import('../pages/Monitoring.vue'), meta: { tab: 'monitoring' } },
  { path: '/settings', name: 'Settings', component: () => import('../pages/Settings.vue'), meta: { tab: 'settings' } },
  { path: '/stock/:symbol', name: 'StockDetail', component: () => import('../pages/StockDetail.vue'), meta: { tab: null } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
