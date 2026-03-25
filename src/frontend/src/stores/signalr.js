import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr'
import { useNotificationsStore } from './notifications'
import { useAuthStore } from './auth'

export const useSignalRStore = defineStore('signalr', () => {
  // State
  const connection = ref(null)
  const connectionState = ref('Disconnected')
  const onlineUsers = ref([])
  const connectionAttempts = ref(0)
  const maxReconnectAttempts = ref(5)
  
  // Computed
  const isConnected = computed(() => connectionState.value === 'Connected')
  const isConnecting = computed(() => connectionState.value === 'Connecting')
  const isDisconnected = computed(() => connectionState.value === 'Disconnected')
  
  // Actions
  async function connect() {
    const authStore = useAuthStore()
    const notificationStore = useNotificationsStore()
    
    if (!authStore.token || connection.value) {
      return
    }
    
    try {
      // Build connection
      connection.value = new HubConnectionBuilder()
        .withUrl('/hubs/notifications', {
          accessTokenFactory: () => authStore.token
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: retryContext => {
            if (retryContext.previousRetryCount < 3) {
              return Math.random() * 1000 + 1000 // 1-2 seconds
            } else if (retryContext.previousRetryCount < 6) {
              return Math.random() * 5000 + 5000 // 5-10 seconds
            } else {
              return null // Stop reconnecting after 6 attempts
            }
          }
        })
        .configureLogging(LogLevel.Information)
        .build()
      
      // Set up event handlers
      setupEventHandlers(notificationStore)
      
      // Connection state change handlers
      connection.value.onclose(error => {
        connectionState.value = 'Disconnected'
        if (error) {
          console.error('SignalR connection closed with error:', error)
        } else {
          console.log('SignalR connection closed')
        }
      })
      
      connection.value.onreconnecting(error => {
        connectionState.value = 'Reconnecting'
        console.log('SignalR reconnecting...', error)
      })
      
      connection.value.onreconnected(connectionId => {
        connectionState.value = 'Connected'
        connectionAttempts.value = 0
        console.log('SignalR reconnected:', connectionId)
        
        notificationStore.addNotification({
          type: 'success',
          title: 'Connected',
          message: 'Real-time connection restored',
          timeout: 3000
        })
      })
      
      // Start connection
      connectionState.value = 'Connecting'
      await connection.value.start()
      connectionState.value = 'Connected'
      connectionAttempts.value = 0
      
      console.log('SignalR connected successfully')
      
      notificationStore.addNotification({
        type: 'success',
        title: 'Connected',
        message: 'Real-time features enabled',
        timeout: 3000
      })
      
    } catch (error) {
      connectionState.value = 'Disconnected'
      connectionAttempts.value++
      console.error('SignalR connection failed:', error)
      
      if (connectionAttempts.value < maxReconnectAttempts.value) {
        // Retry connection after delay
        setTimeout(() => connect(), 5000 * connectionAttempts.value)
      } else {
        notificationStore.addNotification({
          type: 'error',
          title: 'Connection Failed',
          message: 'Unable to establish real-time connection. Some features may be limited.',
          timeout: 8000
        })
      }
    }
  }
  
  function setupEventHandlers(notificationStore) {
    if (!connection.value) return
    
    // User presence events
    connection.value.on('UserOnline', (data) => {
      const user = onlineUsers.value.find(u => u.userId === data.userId)
      if (!user) {
        onlineUsers.value.push({
          userId: data.userId,
          fullName: data.fullName,
          status: 'online',
          lastSeen: new Date()
        })
      }
    })
    
    connection.value.on('UserOffline', (data) => {
      const userIndex = onlineUsers.value.findIndex(u => u.userId === data.userId)
      if (userIndex > -1) {
        onlineUsers.value.splice(userIndex, 1)
      }
    })
    
    connection.value.on('UserStatusChanged', (data) => {
      const user = onlineUsers.value.find(u => u.userId === data.userId)
      if (user) {
        user.status = data.status
        user.context = data.context
        user.lastSeen = new Date(data.timestamp)
      }
    })
    
    // Direct messages
    connection.value.on('DirectMessage', (data) => {
      notificationStore.addNotification({
        type: 'info',
        title: `Message from ${data.senderName}`,
        message: data.message,
        source: 'direct-message',
        data
      })
    })
    
    // Group messages
    connection.value.on('GroupMessage', (data) => {
      notificationStore.addNotification({
        type: 'info',
        title: `${data.groupName} Group`,
        message: `${data.senderName}: ${data.message}`,
        source: 'group-message',
        data
      })
    })
    
    // Background job notifications
    connection.value.on('EmailSent', (data) => {
      notificationStore.addNotification({
        type: 'success',
        title: 'Email Sent',
        message: `Email sent to ${data.recipient}`,
        timeout: 3000
      })
    })
    
    connection.value.on('DataCleanupCompleted', (data) => {
      notificationStore.addNotification({
        type: 'info',
        title: 'Data Cleanup Complete',
        message: `Removed ${data.activitiesRemoved} old activities`,
        timeout: 5000
      })
    })
    
    connection.value.on('ReportReady', (data) => {
      notificationStore.handleReportReady(data)
    })
    
    connection.value.on('ReportFailed', (data) => {
      notificationStore.addNotification({
        type: 'error',
        title: 'Report Generation Failed',
        message: data.error,
        timeout: 8000
      })
    })
    
    connection.value.on('ActivityReminder', (data) => {
      notificationStore.handleActivityReminder(data)
    })
    
    connection.value.on('BackupCompleted', (data) => {
      notificationStore.addNotification({
        type: 'success',
        title: 'Backup Complete',
        message: 'Database backup completed successfully',
        timeout: 5000
      })
    })
    
    connection.value.on('BackupFailed', (data) => {
      notificationStore.addNotification({
        type: 'error',
        title: 'Backup Failed',
        message: data.error,
        timeout: 8000
      })
    })
    
    // Business entity change notifications
    connection.value.on('ContactCreated', (data) => {
      notificationStore.handleDataChange({
        entityType: 'Contact',
        action: 'created',
        changedBy: 'User',
        data
      })
    })
    
    connection.value.on('ContactUpdated', (data) => {
      notificationStore.handleDataChange({
        entityType: 'Contact',
        action: 'updated',
        changedBy: 'User',
        data
      })
    })
    
    connection.value.on('CompanyCreated', (data) => {
      notificationStore.handleDataChange({
        entityType: 'Company',
        action: 'created',
        changedBy: 'User',
        data
      })
    })
    
    connection.value.on('OpportunityCreated', (data) => {
      notificationStore.handleDataChange({
        entityType: 'Opportunity',
        action: 'created',
        changedBy: 'User',
        data
      })
    })
    
    // Generic data change handler
    connection.value.on('DataChanged', (data) => {
      notificationStore.handleDataChange(data)
    })
    
    // Online users update
    connection.value.on('OnlineUsersUpdate', (data) => {
      // Handle online users list update
      console.log('Online users update:', data)
    })
  }
  
  async function disconnect() {
    if (connection.value && connection.value.state !== HubConnectionState.Disconnected) {
      try {
        await connection.value.stop()
        console.log('SignalR disconnected')
      } catch (error) {
        console.error('Error disconnecting SignalR:', error)
      }
    }
    connection.value = null
    connectionState.value = 'Disconnected'
    onlineUsers.value = []
  }
  
  async function sendDirectMessage(targetUserId, message) {
    if (!isConnected.value) {
      throw new Error('Not connected to SignalR hub')
    }
    
    try {
      await connection.value.invoke('SendDirectMessage', targetUserId, message)
      return true
    } catch (error) {
      console.error('Error sending direct message:', error)
      return false
    }
  }
  
  async function sendGroupMessage(groupName, message) {
    if (!isConnected.value) {
      throw new Error('Not connected to SignalR hub')
    }
    
    try {
      await connection.value.invoke('SendGroupMessage', groupName, message)
      return true
    } catch (error) {
      console.error('Error sending group message:', error)
      return false
    }
  }
  
  async function updateStatus(status, context = null) {
    if (!isConnected.value) {
      return false
    }
    
    try {
      await connection.value.invoke('UpdateStatus', status, context)
      return true
    } catch (error) {
      console.error('Error updating status:', error)
      return false
    }
  }
  
  async function notifyDataChange(entityType, action, data) {
    if (!isConnected.value) {
      return false
    }
    
    try {
      await connection.value.invoke('NotifyDataChange', entityType, action, data)
      return true
    } catch (error) {
      console.error('Error notifying data change:', error)
      return false
    }
  }
  
  async function joinGroup(groupName) {
    if (!isConnected.value) {
      return false
    }
    
    try {
      await connection.value.invoke('JoinGroup', groupName)
      return true
    } catch (error) {
      console.error('Error joining group:', error)
      return false
    }
  }
  
  async function leaveGroup(groupName) {
    if (!isConnected.value) {
      return false
    }
    
    try {
      await connection.value.invoke('LeaveGroup', groupName)
      return true
    } catch (error) {
      console.error('Error leaving group:', error)
      return false
    }
  }
  
  async function requestOnlineUsers() {
    if (!isConnected.value) {
      return false
    }
    
    try {
      await connection.value.invoke('RequestOnlineUsers')
      return true
    } catch (error) {
      console.error('Error requesting online users:', error)
      return false
    }
  }

  return {
    // State
    connection,
    connectionState,
    onlineUsers,
    connectionAttempts,
    
    // Computed
    isConnected,
    isConnecting,
    isDisconnected,
    
    // Actions
    connect,
    disconnect,
    sendDirectMessage,
    sendGroupMessage,
    updateStatus,
    notifyDataChange,
    joinGroup,
    leaveGroup,
    requestOnlineUsers
  }
})