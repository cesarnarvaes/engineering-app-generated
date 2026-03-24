<template>
  <div id="app" class="h-screen">
    <!-- Loading overlay -->
    <div v-if="isLoading" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <el-loading v-loading="true" element-loading-text="Loading..." />
    </div>

    <!-- Main application -->
    <div v-else class="h-full">
      <!-- Authentication views -->
      <template v-if="!authStore.isAuthenticated">
        <router-view />
      </template>

      <!-- Authenticated layout -->
      <template v-else>
        <el-container class="h-full">
          <!-- Sidebar -->
          <Sidebar v-if="!isMobile" />
          
          <!-- Mobile sidebar -->
          <MobileSidebar v-if="isMobile" v-model:open="mobileSidebarOpen" />

          <!-- Main content -->
          <el-container direction="vertical" class="overflow-hidden">
            <!-- Top navigation -->
            <el-header class="p-0 bg-white border-b border-gray-200">
              <TopNavigation @toggle-mobile-sidebar="mobileSidebarOpen = !mobileSidebarOpen" />
            </el-header>
            
            <!-- Page content -->
            <el-main class="bg-gray-50 overflow-auto">
              <div class="p-6">
                <!-- Breadcrumbs -->
                <Breadcrumbs v-if="$route.meta.breadcrumbs" :items="$route.meta.breadcrumbs" />
                
                <!-- Page title -->
                <div v-if="$route.meta.title" class="mb-6">
                  <h1 class="text-2xl font-semibold text-gray-900">{{ $route.meta.title }}</h1>
                  <p v-if="$route.meta.description" class="mt-1 text-sm text-gray-600">{{ $route.meta.description }}</p>
                </div>
                
                <!-- Router view with transitions -->
                <router-view v-slot="{ Component }">
                  <transition name="el-fade-in" mode="out-in">
                    <component :is="Component" />
                  </transition>
                </router-view>
              </div>
            </el-main>
          </el-container>
        </el-container>
      </template>
    </div>

    <!-- Global notifications -->
    <NotificationCenter />
  </div>
</template>
<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'
import { useSignalRStore } from '@/stores/signalr'

// Components
import Sidebar from '@/components/layout/Sidebar.vue'
import MobileSidebar from '@/components/layout/MobileSidebar.vue'
import TopNavigation from '@/components/layout/TopNavigation.vue'
import Breadcrumbs from '@/components/layout/Breadcrumbs.vue'
import NotificationCenter from '@/components/notifications/NotificationCenter.vue'

// Stores
const authStore = useAuthStore()
const notificationsStore = useNotificationsStore()
const signalRStore = useSignalRStore()

// Router
const router = useRouter()

// Reactive state
const isLoading = ref(true)
const mobileSidebarOpen = ref(false) 
const screenWidth = ref(window.innerWidth)

// Computed
const isMobile = computed(() => screenWidth.value < 768)

// Methods
const handleResize = () => {
  screenWidth.value = window.innerWidth
  if (screenWidth.value >= 768) {
    mobileSidebarOpen.value = false
  }
}

const initializeApp = async () => {
  try {
    // Check if user is already authenticated
    const token = localStorage.getItem('auth-token')
    if (token) {
      await authStore.getCurrentUser()
      
      // Initialize SignalR connection if authenticated
      if (authStore.isAuthenticated) {
        await signalRStore.connect()
      }
    }
    
    // Navigate to appropriate route
    if (authStore.isAuthenticated && router.currentRoute.value.meta.requiresAuth === false) {
      await router.push('/dashboard')
    } else if (!authStore.isAuthenticated && router.currentRoute.value.meta.requiresAuth !== false) {
      await router.push('/login')
    }
  } catch (error) {
    console.error('App initialization error:', error)
    notificationsStore.addNotification({
      type: 'error',
      title: 'Initialization Error',
      message: 'Failed to initialize application'
    })
  } finally {
    isLoading.value = false
  }
}

// Lifecycle
onMounted(() => {
  window.addEventListener('resize', handleResize)
  initializeApp()
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  signalRStore.disconnect()
})
</script>

<style>
/* Element Plus customizations */
.el-container {
  height: 100%;
}

.el-header {
  height: 64px !important;
  line-height: 64px;
}

.el-main {
  padding: 0;
}

/* Custom transitions */
.el-fade-in-enter-active,
.el-fade-in-leave-active {
  transition: opacity 0.3s ease;
}

.el-fade-in-enter-from,
.el-fade-in-leave-to {
  opacity: 0;
}
</style>