<template>
  <aside class="w-64 bg-white shadow-lg border-r border-gray-200">
    <div class="flex flex-col h-full">
      <!-- Logo -->
      <div class="flex items-center justify-center h-16 px-6 border-b border-gray-200">
        <div class="flex items-center">
          <el-icon class="text-2xl text-blue-600 mr-3">
            <OfficeBuilding />
          </el-icon>
          <h1 class="text-xl font-bold text-gray-900">BizCRM</h1>
        </div>
      </div>

      <!-- Navigation Menu -->
      <nav class="flex-1 overflow-y-auto py-4">
        <el-menu
          :default-active="activeMenuItem"
          class="el-menu-vertical-demo border-none"
          @select="handleMenuSelect"
        >
          <el-menu-item index="dashboard" class="menu-item">
            <el-icon><House /></el-icon>
            <span>Dashboard</span>
          </el-menu-item>

          <el-sub-menu index="contacts" class="menu-item">
            <template #title>
              <el-icon><User /></el-icon>
              <span>Contacts</span>
            </template>
            <el-menu-item index="contacts/list">All Contacts</el-menu-item>
            <el-menu-item index="contacts/new">New Contact</el-menu-item>
          </el-sub-menu>

          <el-sub-menu index="companies" class="menu-item">
            <template #title>
              <el-icon><OfficeBuilding /></el-icon>
              <span>Companies</span>
            </template>
            <el-menu-item index="companies/list">All Companies</el-menu-item>
            <el-menu-item index="companies/new">New Company</el-menu-item>
          </el-sub-menu>

          <el-sub-menu index="opportunities" class="menu-item">
            <template #title>
              <el-icon><TrendCharts /></el-icon>
              <span>Opportunities</span>
            </template>
            <el-menu-item index="opportunities/list">All Opportunities</el-menu-item>
            <el-menu-item index="opportunities/new">New Opportunity</el-menu-item>
          </el-sub-menu>

          <el-sub-menu index="activities" class="menu-item">
            <template #title>
              <el-icon><Calendar /></el-icon>
              <span>Activities</span>
            </template>
            <el-menu-item index="activities/list">All Activities</el-menu-item>
            <el-menu-item index="activities/new">New Activity</el-menu-item>
          </el-sub-menu>

          <el-menu-item index="reports" class="menu-item">
            <el-icon><DataAnalysis /></el-icon>
            <span>Reports</span>
          </el-menu-item>

          <el-divider class="my-2" />

          <el-sub-menu index="admin" class="menu-item" v-if="canAccessAdmin">
            <template #title>
              <el-icon><Setting /></el-icon>
              <span>Administration</span>
            </template>
            <el-menu-item index="admin/users">Users</el-menu-item>
            <el-menu-item index="admin/settings">Settings</el-menu-item>
          </el-sub-menu>

          <el-menu-item index="profile" class="menu-item">
            <el-icon><UserFilled /></el-icon>
            <span>Profile</span>
          </el-menu-item>
        </el-menu>
      </nav>

      <!-- Footer -->
      <div class="border-t border-gray-200 p-4">
        <div class="flex items-center">
          <el-avatar :size="32" icon="UserFilled" class="mr-3" />
          <div class="flex-1">
            <div class="text-sm font-medium text-gray-900">{{ user.name }}</div>
            <div class="text-xs text-gray-500">{{ user.email }}</div>
          </div>
          <el-dropdown trigger="click">
            <el-icon class="text-gray-400 hover:text-gray-600 cursor-pointer">
              <MoreFilled />
            </el-icon>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="handleLogout">
                  <el-icon><SwitchButton /></el-icon>
                  Logout
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { 
  House, 
  User, 
  OfficeBuilding, 
  TrendCharts, 
  Calendar, 
  DataAnalysis, 
  Setting, 
  UserFilled, 
  MoreFilled, 
  SwitchButton 
} from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const user = computed(() => authStore.user)
const canAccessAdmin = computed(() => authStore.hasRole(['systemadmin', 'manager']))

const activeMenuItem = computed(() => {
  const path = route.path
  if (path.startsWith('/contacts')) return 'contacts/list'
  if (path.startsWith('/companies')) return 'companies/list'
  if (path.startsWith('/opportunities')) return 'opportunities/list'
  if (path.startsWith('/activities')) return 'activities/list'
  if (path.startsWith('/reports')) return 'reports'
  if (path.startsWith('/admin')) return path === '/admin/users' ? 'admin/users' : 'admin/settings'
  if (path.startsWith('/profile')) return 'profile'
  return 'dashboard'
})

const handleMenuSelect = (index) => {
  switch (index) {
    case 'dashboard':
      router.push('/dashboard')
      break
    case 'contacts/list':
      router.push('/contacts')
      break
    case 'contacts/new':
      router.push('/contacts/new')
      break
    case 'companies/list':
      router.push('/companies')
      break
    case 'companies/new':
      router.push('/companies/new')
      break
    case 'opportunities/list':
      router.push('/opportunities')
      break
    case 'opportunities/new':
      router.push('/opportunities/new')
      break
    case 'activities/list':
      router.push('/activities')
      break
    case 'activities/new':
      router.push('/activities/new')
      break
    case 'reports':
      router.push('/reports')
      break
    case 'admin/users':
      router.push('/admin/users')
      break
    case 'admin/settings':
      router.push('/admin/settings')
      break
    case 'profile':
      router.push('/profile')
      break
  }
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.el-menu-vertical-demo {
  border-right: none !important;
}

.menu-item {
  margin-bottom: 2px;
}

.el-menu-item:hover,
.el-sub-menu__title:hover {
  background-color: #f3f4f6 !important;
}

.el-menu-item.is-active {
  background-color: #dbeafe !important;
  color: #1d4ed8 !important;
}
</style>