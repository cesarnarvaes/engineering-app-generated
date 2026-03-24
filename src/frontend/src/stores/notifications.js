import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useToast } from 'vue-toastification'

export const useNotificationStore = defineStore('notifications', () => {
  // State
  const notifications = ref([])
  const unreadCount = ref(0)
  const maxNotifications = ref(100) // Keep last 100 notifications
  
  // Toast instance
  const toast = useToast()
  
  // Computed
  const hasUnread = computed(() => unreadCount.value > 0)
  
  const recentNotifications = computed(() => 
    notifications.value.slice(0, 10)
  )
  
  const unreadNotifications = computed(() => 
    notifications.value.filter(n => !n.read)
  )

  // Actions
  function addNotification(notification) {
    const newNotification = {
      id: Date.now() + Math.random(),
      timestamp: new Date(),
      read: false,
      ...notification
    }
    
    notifications.value.unshift(newNotification)
    unreadCount.value++
    
    // Trim to max notifications
    if (notifications.value.length > maxNotifications.value) {
      notifications.value = notifications.value.slice(0, maxNotifications.value)
    }
    
    // Show toast notification
    showToast(notification)
    
    return newNotification.id
  }
  
  function showToast(notification) {
    const options = {
      timeout: notification.timeout || 5000,
      hideProgressBar: false,
      closeOnClick: true,
      pauseOnHover: true,
      draggable: true
    }
    
    switch (notification.type) {
      case 'success':
        toast.success(notification.message, options)
        break
      case 'error':
        toast.error(notification.message, options)
        break
      case 'warning':
        toast.warning(notification.message, options)
        break
      case 'info':
      default:
        toast.info(notification.message, options)
        break
    }
  }
  
  function markAsRead(id) {
    const notification = notifications.value.find(n => n.id === id)
    if (notification && !notification.read) {
      notification.read = true
      unreadCount.value--
    }
  }
  
  function markAllAsRead() {
    notifications.value.forEach(notification => {
      notification.read = true
    })
    unreadCount.value = 0
  }
  
  function removeNotification(id) {
    const index = notifications.value.findIndex(n => n.id === id)
    if (index !== -1) {
      const notification = notifications.value[index]
      if (!notification.read) {
        unreadCount.value--
      }
      notifications.value.splice(index, 1)
    }
  }
  
  function clearAll() {
    notifications.value = []
    unreadCount.value = 0
  }
  
  function clearOld() {
    const oneDayAgo = new Date(Date.now() - 24 * 60 * 60 * 1000)
    const oldNotifications = notifications.value.filter(n => 
      new Date(n.timestamp) < oneDayAgo
    )
    
    oldNotifications.forEach(n => {
      if (!n.read) {
        unreadCount.value--
      }
    })
    
    notifications.value = notifications.value.filter(n => 
      new Date(n.timestamp) >= oneDayAgo
    )
  }
  
  // Real-time notification handlers
  function handleRealtimeNotification(data) {
    const notification = {
      type: data.type || 'info',
      title: data.title || 'Notification',
      message: data.message,
      source: 'realtime',
      data: data.payload
    }
    
    addNotification(notification)
  }
  
  function handleActivityReminder(data) {
    const notification = {
      type: 'warning',
      title: 'Activity Reminder',
      message: `Upcoming: ${data.subject}`,
      source: 'activity',
      data,
      timeout: 10000 // Show longer for reminders
    }
    
    addNotification(notification)
  }
  
  function handleReportReady(data) {
    const notification = {
      type: 'success',
      title: 'Report Ready',
      message: `Your ${data.reportType} report is ready to download`,
      source: 'report',
      data,
      action: {
        text: 'Download',
        url: data.downloadUrl
      }
    }
    
    addNotification(notification)
  }
  
  function handleDataChange(data) {
    const notification = {
      type: 'info',
      title: 'Data Updated',
      message: `${data.entityType} was ${data.action} by ${data.changedBy}`,
      source: 'data-change',
      data,
      timeout: 3000 // Show briefly for data changes
    }
    
    addNotification(notification)
  }

  return {
    // State
    notifications,
    unreadCount,
    
    // Computed
    hasUnread,
    recentNotifications,
    unreadNotifications,
    
    // Actions
    addNotification,
    markAsRead,
    markAllAsRead,
    removeNotification,
    clearAll,
    clearOld,
    
    // Real-time handlers
    handleRealtimeNotification,
    handleActivityReminder,
    handleReportReady,
    handleDataChange
  }
})