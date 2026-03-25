import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'
import router from '@/router'

// Create axios instance
const api = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // Add auth token if available
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    
    // Add request timestamp for debugging
    config.metadata = { startTime: new Date() }
    
    return config
  },
  (error) => {
    console.error('Request error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor
api.interceptors.response.use(
  (response) => {
    // Calculate request duration
    const endTime = new Date()
    const duration = endTime - response.config.metadata.startTime
    
    // Log slow requests in development
    if (import.meta.env.DEV && duration > 2000) {
      console.warn(`Slow API request: ${response.config.url} took ${duration}ms`)
    }
    
    return response
  },
  async (error) => {
    const authStore = useAuthStore()
    const notificationStore = useNotificationsStore()
    const originalRequest = error.config
    
    // Handle 401 Unauthorized
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      
      // Try to refresh token
      const refreshSuccessful = await authStore.refreshAccessToken()
      
      if (refreshSuccessful) {
        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${authStore.token}`
        return api(originalRequest)
      } else {
        // Refresh failed, redirect to login
        await authStore.logout()
        router.push({
          name: 'Login',
          query: { redirect: router.currentRoute.value.fullPath }
        })
        return Promise.reject(error)
      }
    }
    
    // Handle 403 Forbidden
    if (error.response?.status === 403) {
      notificationStore.addNotification({
        type: 'error',
        title: 'Access Denied',
        message: 'You do not have permission to perform this action',
        timeout: 6000
      })
    }
    
    // Handle 404 Not Found
    if (error.response?.status === 404) {
      notificationStore.addNotification({
        type: 'error',
        title: 'Not Found',
        message: 'The requested resource was not found',
        timeout: 5000
      })
    }
    
    // Handle 500 Internal Server Error
    if (error.response?.status >= 500) {
      notificationStore.addNotification({
        type: 'error',
        title: 'Server Error',
        message: 'An internal server error occurred. Please try again later.',
        timeout: 8000
      })
    }
    
    // Handle network errors
    if (error.code === 'NETWORK_ERROR' || error.message === 'Network Error') {
      notificationStore.addNotification({
        type: 'error',
        title: 'Network Error',
        message: 'Unable to connect to the server. Please check your internet connection.',
        timeout: 8000
      })
    }
    
    // Handle timeout errors
    if (error.code === 'ECONNABORTED') {
      notificationStore.addNotification({
        type: 'error',
        title: 'Request Timeout',
        message: 'The request took too long to complete. Please try again.',
        timeout: 6000
      })
    }
    
    return Promise.reject(error)
  }
)

// Helper function to handle paginated requests
export async function fetchPaginated(url, params = {}) {
  const response = await api.get(url, { params })
  return response.data
}

// Helper function to upload files
export async function uploadFile(url, formData, onProgress) {
  const config = {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  }
  
  if (onProgress) {
    config.onUploadProgress = (progressEvent) => {
      const percentCompleted = Math.round(
        (progressEvent.loaded * 100) / progressEvent.total
      )
      onProgress(percentCompleted)
    }
  }
  
  const response = await api.post(url, formData, config)
  return response.data
}

// Helper function to download files
export async function downloadFile(url, filename) {
  const response = await api.get(url, {
    responseType: 'blob'
  })
  
  // Create blob link to download
  const href = URL.createObjectURL(response.data)
  const link = document.createElement('a')
  link.href = href
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(href)
}

// API endpoints organized by feature
export const authAPI = {
  login: (credentials) => api.post('/auth/login', credentials),
  logout: () => api.post('/auth/logout'),
  getCurrentUser: () => api.get('/auth/me'),
  refreshToken: (refreshToken) => api.post('/auth/refresh', { refreshToken }),
  changePassword: (data) => api.post('/auth/change-password', data),
  resetPassword: (email) => api.post('/auth/reset-password', { email }),
  checkEmail: (email) => api.get(`/auth/check-email/${email}`)
}

export const usersAPI = {
  getUsers: (params) => fetchPaginated('/users', params),
  getUserById: (id) => api.get(`/users/${id}`),
  updateUser: (id, data) => api.put(`/users/${id}`, data),
  deactivateUser: (id) => api.post(`/users/${id}/deactivate`),
  activateUser: (id) => api.post(`/users/${id}/activate`),
  getUserRoles: (id) => api.get(`/users/${id}/roles`),
  assignRole: (id, role) => api.post(`/users/${id}/roles/${role}`),
  removeRole: (id, role) => api.delete(`/users/${id}/roles/${role}`),
  getAvailableRoles: () => api.get('/users/roles/available')
}

export const contactsAPI = {
  getContacts: (params) => fetchPaginated('/contacts', params),
  getContactById: (id) => api.get(`/contacts/${id}`),
  createContact: (data) => api.post('/contacts', data),
  updateContact: (id, data) => api.put(`/contacts/${id}`, data),
  deleteContact: (id) => api.delete(`/contacts/${id}`)
}

export const companiesAPI = {
  getCompanies: (params) => fetchPaginated('/companies', params),
  getCompanyById: (id) => api.get(`/companies/${id}`),
  createCompany: (data) => api.post('/companies', data),
  updateCompany: (id, data) => api.put(`/companies/${id}`, data),
  deleteCompany: (id) => api.delete(`/companies/${id}`)
}

export const opportunitiesAPI = {
  getOpportunities: (params) => fetchPaginated('/opportunities', params),
  getOpportunityById: (id) => api.get(`/opportunities/${id}`),
  createOpportunity: (data) => api.post('/opportunities', data),
  updateOpportunity: (id, data) => api.put(`/opportunities/${id}`, data),
  deleteOpportunity: (id) => api.delete(`/opportunities/${id}`)
}

export const activitiesAPI = {
  getActivities: (params) => fetchPaginated('/activities', params),
  getActivityById: (id) => api.get(`/activities/${id}`),
  createActivity: (data) => api.post('/activities', data),
  updateActivity: (id, data) => api.put(`/activities/${id}`, data),
  deleteActivity: (id) => api.delete(`/activities/${id}`)
}

export const reportsAPI = {
  generateReport: (data) => api.post('/reports/generate', data),
  getReport: (id) => api.get(`/reports/${id}`),
  downloadReport: (id, filename) => downloadFile(`/reports/${id}/download`, filename)
}

// Default export
export default api