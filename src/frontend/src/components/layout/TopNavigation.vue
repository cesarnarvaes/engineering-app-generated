<template>
  <header class="bg-white shadow-sm border-b border-gray-200">
    <div class="flex items-center justify-between h-16 px-6">
      <!-- Left side - Mobile menu button -->
      <div class="flex items-center">
        <el-button 
          text 
          size="large" 
          class="md:hidden mr-3"
          @click="$emit('toggle-mobile-sidebar')"
        >
          <el-icon><Menu /></el-icon>
        </el-button>
        
        <!-- Page title -->
        <div v-if="pageTitle">
          <h1 class="text-lg font-semibold text-gray-900">{{ pageTitle }}</h1>
        </div>
      </div>

      <!-- Right side - Search, Notifications, User menu -->
      <div class="flex items-center space-x-4">
        <!-- Global Search -->
        <div class="hidden md:block">
          <el-input
            v-model="searchQuery"
            placeholder="Search contacts, companies..."
            class="w-64"
            clearable
            @keyup.enter="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>

        <!-- Quick Actions -->
        <el-dropdown trigger="click">
          <el-button type="primary" size="default">
            <el-icon class="mr-1"><Plus /></el-icon>
            Create
            <el-icon class="ml-1"><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="handleQuickAction('contact')">
                <el-icon><User /></el-icon>
                New Contact
              </el-dropdown-item>
              <el-dropdown-item @click="handleQuickAction('company')">
                <el-icon><OfficeBuilding /></el-icon>
                New Company
              </el-dropdown-item>
              <el-dropdown-item @click="handleQuickAction('opportunity')">
                <el-icon><TrendCharts /></el-icon>
                New Opportunity
              </el-dropdown-item>
              <el-dropdown-item @click="handleQuickAction('activity')">
                <el-icon><Calendar /></el-icon>
                New Activity
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <!-- Notifications -->
        <el-badge :value="notificationCount" :hidden="notificationCount === 0" class="notification-badge">
          <el-button text size="large" @click="toggleNotifications">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-badge>

        <!-- User Menu -->
        <el-dropdown trigger="click">
          <div class="flex items-center cursor-pointer hover:bg-gray-50 rounded-md px-2 py-1">
            <el-avatar :size="32" :src="user.avatar" icon="UserFilled" class="mr-2" />
            <div class="hidden lg:block">
              <div class="text-sm font-medium text-gray-900">{{ user.name }}</div>
              <div class="text-xs text-gray-500">{{ user.email }}</div>
            </div>
            <el-icon class="ml-1 text-gray-400">
              <ArrowDown />
            </el-icon>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="handleUserAction('profile')">
                <el-icon><User /></el-icon>
                Profile
              </el-dropdown-item>
              <el-dropdown-item @click="handleUserAction('settings')">
                <el-icon><Setting /></el-icon>
                Settings
              </el-dropdown-item>
              <el-dropdown-item divided @click="handleLogout">
                <el-icon><SwitchButton /></el-icon>
                Logout
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- Notifications Panel -->
    <el-drawer
      v-model="showNotifications"
      title="Notifications"
      direction="rtl"
      size="400px"
    >
      <div class="p-4">
        <div v-if="notifications.length === 0" class="text-center text-gray-500">
          <el-icon class="text-4xl mb-2"><Bell /></el-icon>
          <p>No notifications</p>
        </div>
        
        <div v-else class="space-y-3">
          <div 
            v-for="notification in notifications" 
            :key="notification.id"
            class="flex items-start p-3 border border-gray-100 rounded-lg hover:bg-gray-50"
          >
            <el-avatar :size="32" icon="User" class="mr-3" />
            <div class="flex-1">
              <p class="text-sm font-medium text-gray-900">{{ notification.title }}</p>
              <p class="text-xs text-gray-600">{{ notification.message }}</p>
              <p class="text-xs text-gray-400 mt-1">{{ formatTime(notification.createdAt) }}</p>
            </div>
            <el-button text size="small" @click="markAsRead(notification.id)">
              <el-icon><Check /></el-icon>
            </el-button>
          </div>
        </div>
      </div>
    </el-drawer>
  </header>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'
import { format } from 'date-fns'
import {
  Menu,
  Search,
  Plus,
  ArrowDown,
  User,
  OfficeBuilding,
  TrendCharts,
  Calendar,
  Bell,
  UserFilled,
  Setting,
  SwitchButton,
  Check
} from '@element-plus/icons-vue'

// Emits
defineEmits(['toggle-mobile-sidebar'])

// Stores
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const notificationsStore = useNotificationsStore()

// Reactive data
const searchQuery = ref('')
const showNotifications = ref(false)

// Computed properties
const user = computed(() => authStore.user)
const pageTitle = computed(() => route.meta.title)
const notifications = computed(() => notificationsStore.unreadNotifications)
const notificationCount = computed(() => notifications.value.length)

// Methods
const handleSearch = () => {
  if (searchQuery.value.trim()) {
    router.push({
      path: '/search',
      query: { q: searchQuery.value }
    })
  }
}

const handleQuickAction = (type) => {
  const routes = {
    contact: '/contacts/new',
    company: '/companies/new',
    opportunity: '/opportunities/new',
    activity: '/activities/new'
  }
  router.push(routes[type])
}

const handleUserAction = (action) => {
  const routes = {
    profile: '/profile',
    settings: '/settings'
  }
  router.push(routes[action])
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

const toggleNotifications = () => {
  showNotifications.value = !showNotifications.value
}

const markAsRead = (notificationId) => {
  notificationsStore.markAsRead(notificationId)
}

const formatTime = (date) => {
  return format(new Date(date), 'MMM dd, HH:mm')
}
</script>

<style scoped>
.notification-badge :deep(.el-badge__content) {
  background-color: #ef4444;
  border-color: #ef4444;
}
</style>