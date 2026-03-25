import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'
import { useNotificationsStore } from './notifications'
import { useSignalRStore } from './signalr'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref(localStorage.getItem('auth-token'))
  const refreshToken = ref(localStorage.getItem('refresh-token'))
  const user = ref(null)
  const loading = ref(false)

  // Computed
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  
  const userRoles = computed(() => user.value?.roles || [])
  
  const isSystemAdmin = computed(() => userRoles.value.includes('systemadmin'))
  
  const isManager = computed(() => userRoles.value.includes('manager') || isSystemAdmin.value)
  
  const isStaff = computed(() => 
    userRoles.value.includes('userstaff') || isManager.value
  )
  
  const isBusinessUser = computed(() => 
    userRoles.value.includes('businessuser') || isStaff.value
  )

  // Actions
  async function login(credentials) {
    const notificationStore = useNotificationsStore()
    const signalRStore = useSignalRStore()
    
    loading.value = true
    
    try {
      const response = await api.post('/auth/login', credentials)
      
      if (response.data.success) {
        const { token: accessToken, refreshToken: newRefreshToken, user: userData } = response.data.data
        
        // Store tokens
        token.value = accessToken
        refreshToken.value = newRefreshToken
        user.value = userData
        
        // Persist to localStorage
        localStorage.setItem('auth-token', accessToken)
        localStorage.setItem('refresh-token', newRefreshToken)
        
        // Set default auth header
        api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
        
        // Initialize SignalR connection
        await signalRStore.connect()
        
        notificationStore.addNotification({
          type: 'success',
          title: 'Welcome back!',
          message: `Hello ${userData.fullName}`
        })
        
        return true
      } else {
        throw new Error(response.data.message || 'Login failed')
      }
    } catch (error) {
      console.error('Login error:', error)
      
      const message = error.response?.data?.message || error.message || 'Login failed'
      
      notificationStore.addNotification({
        type: 'error',
        title: 'Login Failed',
        message
      })
      
      return false
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    const notificationStore = useNotificationsStore()
    const signalRStore = useSignalRStore()
    
    try {
      // Call logout API
      await api.post('/auth/logout')
    } catch (error) {
      console.error('Logout API error:', error)
    } finally {
      // Clear local state regardless of API success
      token.value = null
      refreshToken.value = null
      user.value = null
      
      // Clear localStorage
      localStorage.removeItem('auth-token')
      localStorage.removeItem('refresh-token')
      
      // Clear auth header
      delete api.defaults.headers.common['Authorization']
      
      // Disconnect SignalR
      await signalRStore.disconnect()
      
      notificationStore.addNotification({
        type: 'info',
        title: 'Logged Out',
        message: 'You have been logged out successfully'
      })
    }
  }

  async function getCurrentUser() {
    if (!token.value) {
      return false
    }
    
    try {
      // Set auth header
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      
      const response = await api.get('/auth/me')
      
      if (response.data.success) {
        user.value = response.data.data
        return true
      } else {
        throw new Error('Failed to get user info')
      }
    } catch (error) {
      console.error('Get current user error:', error)
      
      // If unauthorized, clear auth data
      if (error.response?.status === 401) {
        await logout()
      }
      
      return false
    }
  }

  async function refreshAccessToken() {
    if (!refreshToken.value) {
      await logout()
      return false
    }
    
    try {
      const response = await api.post('/auth/refresh', {
        refreshToken: refreshToken.value
      })
      
      if (response.data.success) {
        const { token: newAccessToken, refreshToken: newRefreshToken } = response.data.data
        
        token.value = newAccessToken
        refreshToken.value = newRefreshToken
        
        localStorage.setItem('auth-token', newAccessToken)
        localStorage.setItem('refresh-token', newRefreshToken)
        
        api.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`
        
        return true
      } else {
        throw new Error('Token refresh failed')
      }
    } catch (error) {
      console.error('Token refresh error:', error)
      await logout()
      return false
    }
  }

  async function changePassword(currentPassword, newPassword) {
    const notificationStore = useNotificationsStore()
    
    try {
      const response = await api.post('/auth/change-password', {
        currentPassword,
        newPassword
      })
      
      if (response.data.success) {
        notificationStore.addNotification({
          type: 'success',
          title: 'Password Changed',
          message: 'Your password has been updated successfully'
        })
        return true
      } else {
        throw new Error(response.data.message || 'Failed to change password')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Failed to change password'
      
      notificationStore.addNotification({
        type: 'error',
        title: 'Password Change Failed',
        message
      })
      
      return false
    }
  }

  function hasRole(role) {
    return userRoles.value.includes(role)
  }
  
  function hasAnyRole(roles) {
    return roles.some(role => userRoles.value.includes(role))
  }
  
  function hasAllRoles(roles) {
    return roles.every(role => userRoles.value.includes(role))
  }

  return {
    // State
    token,
    refreshToken,
    user,
    loading,
    
    // Computed
    isAuthenticated,
    userRoles,
    isSystemAdmin,
    isManager,
    isStaff,
    isBusinessUser,
    
    // Actions
    login,
    logout,
    getCurrentUser,
    refreshAccessToken,
    changePassword,
    hasRole,
    hasAnyRole,
    hasAllRoles
  }
})