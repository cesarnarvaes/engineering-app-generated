<template>
  <div class="notification-center">
    <!-- Toast notifications container -->
    <transition-group name="notification" tag="div" class="notification-container">
      <div
        v-for="notification in visibleNotifications"
        :key="notification.id"
        :class="[
          'notification-item',
          `notification-${notification.type}`
        ]"
      >
        <el-alert
          :type="notification.type"
          :title="notification.title"
          :description="notification.message"
          :closable="true"
          :show-icon="true"
          :effect="isDark(notification.type) ? 'dark' : 'light'"
          @close="removeNotification(notification.id)"
        >
          <template #default>
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <h4 class="text-sm font-medium mb-1">{{ notification.title }}</h4>
                <p class="text-sm text-gray-600">{{ notification.message }}</p>
                <p v-if="notification.timestamp" class="text-xs text-gray-400 mt-2">
                  {{ formatTimestamp(notification.timestamp) }}
                </p>
              </div>
              
              <!-- Action buttons -->
              <div v-if="notification.actions" class="ml-4 flex-shrink-0">
                <div class="space-x-2">
                  <el-button
                    v-for="action in notification.actions"
                    :key="action.id"
                    :type="action.type || 'primary'"
                    size="small"
                    @click="handleAction(notification.id, action)"
                  >
                    {{ action.text }}
                  </el-button>
                </div>
              </div>
            </div>
          </template>
        </el-alert>
      </div>
    </transition-group>

    <!-- Desktop notifications permission -->
    <div v-if="showPermissionRequest" class="fixed top-4 right-4 z-50">
      <el-alert
        title="Enable Desktop Notifications"
        description="Allow desktop notifications to stay informed about important updates."
        type="info"
        :closable="true"
        show-icon
        @close="hidePermissionRequest"
      >
        <template #default>
          <div class="mt-3 space-x-2">
            <el-button size="small" type="primary" @click="requestNotificationPermission">
              Enable
            </el-button>
            <el-button size="small" @click="hidePermissionRequest">
              Maybe Later
            </el-button>
          </div>
        </template>
      </el-alert>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { useNotificationsStore } from '@/stores/notifications'
import { format } from 'date-fns'

// Stores
const notificationsStore = useNotificationsStore()

// Reactive state
const showPermissionRequest = ref(false)

// Computed
const visibleNotifications = computed(() => 
  notificationsStore.notifications.filter(n => n.visible !== false).slice(0, 5)
)

// Methods
const removeNotification = (id) => {
  notificationsStore.removeNotification(id)
}

const handleAction = (notificationId, action) => {
  if (action.handler) {
    action.handler()
  }
  
  // Remove notification after action unless specified otherwise
  if (action.removeAfterAction !== false) {
    removeNotification(notificationId)
  }
}

const formatTimestamp = (timestamp) => {
  return format(new Date(timestamp), 'MMM dd, HH:mm')
}

const isDark = (type) => {
  return ['error', 'warning'].includes(type)
}

const requestNotificationPermission = async () => {
  try {
    const permission = await Notification.requestPermission()
    if (permission === 'granted') {
      notificationsStore.setDesktopNotificationsEnabled(true)
      showPermissionRequest.value = false
      
      // Show success notification
      notificationsStore.addNotification({
        type: 'success',
        title: 'Notifications Enabled',
        message: 'Desktop notifications are now enabled.'
      })
    }
  } catch (error) {
    console.error('Failed to request notification permission:', error)
  }
}

const hidePermissionRequest = () => {
  showPermissionRequest.value = false
  localStorage.setItem('notification-permission-dismissed', 'true')
}

const checkNotificationPermission = () => {
  const dismissed = localStorage.getItem('notification-permission-dismissed')
  
  if (!dismissed && 
      'Notification' in window && 
      Notification.permission === 'default' &&
      !notificationsStore.desktopNotificationsEnabled
  ) {
    setTimeout(() => {
      showPermissionRequest.value = true
    }, 3000) // Show after 3 seconds
  }
}

// Auto-remove notifications after timeout
const autoRemoveNotifications = () => {
  visibleNotifications.value.forEach(notification => {
    if (notification.timeout && notification.timestamp) {
      const elapsed = Date.now() - new Date(notification.timestamp).getTime()
      if (elapsed > notification.timeout) {
        removeNotification(notification.id)
      }
    }
  })
}

let autoRemoveInterval

// Lifecycle
onMounted(() => {
  checkNotificationPermission()
  
  // Set up auto-remove interval
  autoRemoveInterval = setInterval(autoRemoveNotifications, 1000)
})

onUnmounted(() => {
  if (autoRemoveInterval) {
    clearInterval(autoRemoveInterval)
  }
})
</script>

<style scoped>
.notification-container {
  position: fixed;
  top: 80px;
  right: 20px;
  z-index: 2000;
  max-width: 400px;
  width: 100%;
}

.notification-item {
  margin-bottom: 12px;
}

/* Notification animations */
.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.notification-move {
  transition: transform 0.3s ease;
}

/* Custom alert styles */
:deep(.el-alert) {
  margin-bottom: 0;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border: 1px solid rgba(0, 0, 0, 0.1);
}

:deep(.el-alert__content) {
  padding-right: 0;
}

:deep(.el-alert__title) {
  margin-bottom: 4px;
}

:deep(.el-alert__description) {
  margin: 0;
  line-height: 1.4;
}

/* Type-specific styles */
.notification-success :deep(.el-alert) {
  background-color: #f0f9ff;
  border-color: #10b981;
}

.notification-error :deep(.el-alert) {
  background-color: #fef2f2;
  border-color: #ef4444;
}

.notification-warning :deep(.el-alert) {
  background-color: #fffbeb;
  border-color: #f59e0b;
}

.notification-info :deep(.el-alert) {
  background-color: #f0f9ff;
  border-color: #3b82f6;
}

/* Mobile responsiveness */
@media (max-width: 640px) {
  .notification-container {
    left: 20px;
    right: 20px;
    max-width: none;
  }
}
</style>