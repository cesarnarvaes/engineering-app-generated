import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// Lazy load components
const Login = () => import('@/views/auth/Login.vue')
const Dashboard = () => import('@/views/Dashboard.vue')
const Contacts = () => import('@/views/contacts/ContactList.vue')
const ContactDetail = () => import('@/views/contacts/ContactDetail.vue')
const Companies = () => import('@/views/companies/CompanyList.vue')
const CompanyDetail = () => import('@/views/companies/CompanyDetail.vue')
const Opportunities = () => import('@/views/opportunities/OpportunityList.vue')
const OpportunityDetail = () => import('@/views/opportunities/OpportunityDetail.vue')
const Activities = () => import('@/views/activities/ActivityList.vue')
const ActivityDetail = () => import('@/views/activities/ActivityDetail.vue')
const Users = () => import('@/views/admin/UserList.vue')
const UserDetail = () => import('@/views/admin/UserDetail.vue')
const Reports = () => import('@/views/reports/ReportList.vue')
const Profile = () => import('@/views/profile/Profile.vue')
const Settings = () => import('@/views/settings/Settings.vue')
const NotFound = () => import('@/views/NotFound.vue')

const routes = [
  // Authentication routes
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: {
      requiresAuth: false,
      title: 'Login',
      description: 'Sign in to your account'
    }
  },

  // Main application routes
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Dashboard',
      description: 'Overview of your CRM activities',
      breadcrumbs: [{ text: 'Dashboard', to: '/dashboard' }]
    }
  },

  // Contacts
  {
    path: '/contacts',
    name: 'Contacts',
    component: Contacts,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Contacts',
      description: 'Manage your customer contacts',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Contacts', to: '/contacts' }
      ]
    }
  },
  {
    path: '/contacts/:id',
    name: 'ContactDetail',
    component: ContactDetail,
    props: true,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Contact Details',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Contacts', to: '/contacts' },
        { text: 'Contact Details' }
      ]
    }
  },

  // Companies
  {
    path: '/companies',
    name: 'Companies',
    component: Companies,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Companies',
      description: 'Manage customer companies',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Companies', to: '/companies' }
      ]
    }
  },
  {
    path: '/companies/:id',
    name: 'CompanyDetail',
    component: CompanyDetail,
    props: true,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Company Details',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Companies', to: '/companies' },
        { text: 'Company Details' }
      ]
    }
  },

  // Opportunities
  {
    path: '/opportunities',
    name: 'Opportunities',
    component: Opportunities,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Opportunities',
      description: 'Track sales opportunities',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Opportunities', to: '/opportunities' }
      ]
    }
  },
  {
    path: '/opportunities/:id',
    name: 'OpportunityDetail',
    component: OpportunityDetail,
    props: true,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Opportunity Details',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Opportunities', to: '/opportunities' },
        { text: 'Opportunity Details' }
      ]
    }
  },

  // Activities
  {
    path: '/activities',
    name: 'Activities',
    component: Activities,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Activities',
      description: 'Manage your tasks and activities',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Activities', to: '/activities' }
      ]
    }
  },
  {
    path: '/activities/:id',
    name: 'ActivityDetail',
    component: ActivityDetail,
    props: true,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Activity Details',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Activities', to: '/activities' },
        { text: 'Activity Details' }
      ]
    }
  },

  // Reports
  {
    path: '/reports',
    name: 'Reports',
    component: Reports,
    meta: {
      requiresAuth: true,
      roles: ['userstaff', 'manager', 'systemadmin'],
      title: 'Reports',
      description: 'Generate and view reports',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Reports', to: '/reports' }
      ]
    }
  },

  // User Management (Admin only)
  {
    path: '/users',
    name: 'Users',
    component: Users,
    meta: {
      requiresAuth: true,
      roles: ['manager', 'systemadmin'],
      title: 'User Management',
      description: 'Manage system users',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Administration', to: '/users' },
        { text: 'Users', to: '/users' }
      ]
    }
  },
  {
    path: '/users/:id',
    name: 'UserDetail',
    component: UserDetail,
    props: true,
    meta: {
      requiresAuth: true,
      roles: ['manager', 'systemadmin'],
      title: 'User Details',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Administration', to: '/users' },
        { text: 'Users', to: '/users' },
        { text: 'User Details' }
      ]
    }
  },

  // Profile and Settings
  {
    path: '/profile',
    name: 'Profile',
    component: Profile,
    meta: {
      requiresAuth: true,
      roles: ['businessuser', 'userstaff', 'manager', 'systemadmin'],
      title: 'Profile',
      description: 'Manage your profile settings',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Profile', to: '/profile' }
      ]
    }
  },
  {
    path: '/settings',
    name: 'Settings',
    component: Settings,
    meta: {
      requiresAuth: true,
      roles: ['systemadmin'],
      title: 'System Settings',
      description: 'Configure system settings',
      breadcrumbs: [
        { text: 'Dashboard', to: '/dashboard' },
        { text: 'Settings', to: '/settings' }
      ]
    }
  },

  // 404 Not Found
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: NotFound,
    meta: {
      requiresAuth: false,
      title: 'Page Not Found'
    }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  }
})

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Check if route requires authentication
  if (to.meta.requiresAuth !== false) {
    if (!authStore.isAuthenticated) {
      next({
        name: 'Login',
        query: { redirect: to.fullPath }
      })
      return
    }
    
    // Check role-based permissions
    if (to.meta.roles && !authStore.hasAnyRole(to.meta.roles)) {
      // User doesn't have required role, redirect to dashboard
      next({ name: 'Dashboard' })
      return
    }
  }
  
  // If trying to access login while authenticated, redirect to dashboard
  if (to.name === 'Login' && authStore.isAuthenticated) {
    next({ name: 'Dashboard' })
    return
  }
  
  next()
})

export default router