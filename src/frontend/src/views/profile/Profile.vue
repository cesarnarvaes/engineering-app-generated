<template>
  <div class="profile">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Profile</h1>
        <p class="mt-1 text-gray-600">Manage your account settings and preferences</p>
      </div>
    </div>

    <el-row :gutter="20">
      <!-- Left Column - Profile Information -->
      <el-col :xs="24" :lg="16">
        <!-- Basic Information -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="text-lg font-semibold">Basic Information</span>
              <el-button 
                v-if="!editingBasic" 
                @click="editingBasic = true"
                size="small"
              >
                <el-icon><Edit /></el-icon>
                Edit
              </el-button>
              <div v-else class="space-x-2">
                <el-button @click="cancelBasicEdit" size="small">Cancel</el-button>
                <el-button 
                  type="primary" 
                  :loading="savingBasic"
                  @click="saveBasicInfo"
                  size="small"
                >
                  <el-icon><Check /></el-icon>
                  Save
                </el-button>
              </div>
            </div>
          </template>
          
          <el-form :model="basicForm" label-width="120px">
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="First Name">
                  <el-input 
                    v-if="editingBasic"
                    v-model="basicForm.firstName" 
                    placeholder="Enter first name"
                  />
                  <p v-else class="text-gray-900">{{ userData?.firstName || '-' }}</p>
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="Last Name">
                  <el-input 
                    v-if="editingBasic"
                    v-model="basicForm.lastName" 
                    placeholder="Enter last name"
                  />
                  <p v-else class="text-gray-900">{{ userData?.lastName || '-' }}</p>
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="Email">
                  <el-input 
                    v-if="editingBasic"
                    v-model="basicForm.email" 
                    type="email"
                    placeholder="Enter email address"
                  />
                  <p v-else class="text-gray-900">{{ userData?.email || '-' }}</p>
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="Phone">
                  <el-input 
                    v-if="editingBasic"
                    v-model="basicForm.phone" 
                    placeholder="Enter phone number"
                  />
                  <p v-else class="text-gray-900">{{ userData?.phone || '-' }}</p>
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="Job Title">
                  <el-input 
                    v-if="editingBasic"
                    v-model="basicForm.jobTitle" 
                    placeholder="Enter job title"
                  />
                  <p v-else class="text-gray-900">{{ userData?.jobTitle || '-' }}</p>
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="Department">
                  <el-select
                    v-if="editingBasic"
                    v-model="basicForm.department" 
                    placeholder="Select department"
                    class="w-full"
                  >
                    <el-option label="Sales" value="sales" />
                    <el-option label="Marketing" value="marketing" />
                    <el-option label="Support" value="support" />
                    <el-option label="Management" value="management" />
                  </el-select>
                  <p v-else class="text-gray-900 capitalize">{{ userData?.department || '-' }}</p>
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </el-card>

        <!-- Change Password -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Change Password</span>
          </template>
          
          <el-form :model="passwordForm" label-width="150px" @submit.prevent="changePassword">
            <el-form-item label="Current Password">
              <el-input 
                v-model="passwordForm.currentPassword" 
                type="password"
                placeholder="Enter current password"
                show-password
              />
            </el-form-item>
            
            <el-form-item label="New Password">
              <el-input 
                v-model="passwordForm.newPassword" 
                type="password"
                placeholder="Enter new password"
                show-password
              />
            </el-form-item>
            
            <el-form-item label="Confirm Password">
              <el-input 
                v-model="passwordForm.confirmPassword" 
                type="password"
                placeholder="Confirm new password"
                show-password
              />
            </el-form-item>
            
            <el-form-item>
              <el-button 
                type="primary" 
                :loading="changingPassword"
                @click="changePassword"
              >
                Change Password
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>

        <!-- Preferences -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="text-lg font-semibold">Preferences</span>
              <el-button 
                type="primary" 
                :loading="savingPreferences"
                @click="savePreferences"
                size="small"
              >
                Save Preferences
              </el-button>
            </div>
          </template>
          
          <el-form :model="preferencesForm" label-width="200px">
            <el-form-item label="Language">
              <el-select v-model="preferencesForm.language" placeholder="Select language" class="w-60">
                <el-option label="English" value="en" />
                <el-option label="Spanish" value="es" />
                <el-option label="French" value="fr" />
                <el-option label="German" value="de" />
              </el-select>
            </el-form-item>
            
            <el-form-item label="Timezone">
              <el-select v-model="preferencesForm.timezone" placeholder="Select timezone" class="w-60">
                <el-option label="UTC" value="UTC" />
                <el-option label="Eastern Time" value="America/New_York" />
                <el-option label="Central Time" value="America/Chicago" />
                <el-option label="Mountain Time" value="America/Denver" />
                <el-option label="Pacific Time" value="America/Los_Angeles" />
              </el-select>
            </el-form-item>
            
            <el-form-item label="Date Format">
              <el-select v-model="preferencesForm.dateFormat" placeholder="Select date format" class="w-60">
                <el-option label="MM/DD/YYYY" value="MM/DD/YYYY" />
                <el-option label="DD/MM/YYYY" value="DD/MM/YYYY" />
                <el-option label="YYYY-MM-DD" value="YYYY-MM-DD" />
              </el-select>
            </el-form-item>
            
            <el-form-item label="Theme">
              <el-radio-group v-model="preferencesForm.theme">
                <el-radio value="light">Light</el-radio>
                <el-radio value="dark">Dark</el-radio>
                <el-radio value="auto">Auto</el-radio>
              </el-radio-group>
            </el-form-item>
            
            <el-form-item label="Email Notifications">
              <el-switch v-model="preferencesForm.emailNotifications" />
              <p class="text-sm text-gray-600 mt-1">Receive email notifications for important updates</p>
            </el-form-item>
            
            <el-form-item label="Desktop Notifications">
              <el-switch v-model="preferencesForm.desktopNotifications" />
              <p class="text-sm text-gray-600 mt-1">Show desktop notifications for real-time updates</p>
            </el-form-item>
            
            <el-form-item label="Activity Reminders">
              <el-switch v-model="preferencesForm.activityReminders" />
              <p class="text-sm text-gray-600 mt-1">Get reminders for upcoming activities</p>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <!-- Right Column - Profile Summary & Actions -->
      <el-col :xs="24" :lg="8">
        <!-- Profile Summary -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Profile Summary</span>
          </template>
          
          <div class="text-center">
            <el-avatar :size="80" class="mb-4">
              {{ getInitials() }}
            </el-avatar>
            
            <h3 class="text-xl font-semibold text-gray-900 mb-1">
              {{ userData?.firstName }} {{ userData?.lastName }}
            </h3>
            <p class="text-gray-600 mb-2">{{ userData?.jobTitle }}</p>
            <p class="text-sm text-gray-500">{{ userData?.department }}</p>
            
            <el-divider />
            
            <div class="space-y-2 text-left">
              <div class="flex justify-between">
                <span class="text-gray-600">Member Since</span>
                <span class="font-medium">{{ formatDate(userData?.createdAt) }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">Last Login</span>
                <span class="font-medium">{{ formatDate(userData?.lastLoginAt) }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">Role</span>
                <el-tag size="small" type="primary">{{ userData?.role }}</el-tag>
              </div>
            </div>
          </div>
        </el-card>

        <!-- Quick Actions -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Quick Actions</span>
          </template>
          
          <div class="space-y-2">
            <el-button class="w-full justify-start" @click="uploadAvatar">
              <el-icon class="mr-2"><Upload /></el-icon>
              Upload Photo
            </el-button>
            
            <el-button class="w-full justify-start" @click="downloadData">
              <el-icon class="mr-2"><Download /></el-icon>
              Download My Data
            </el-button>
            
            <el-button class="w-full justify-start" @click="viewActivityLog">
              <el-icon class="mr-2"><Document /></el-icon>
              View Activity Log
            </el-button>
            
            <el-button class="w-full justify-start" @click="exportContacts">
              <el-icon class="mr-2"><Share /></el-icon>
              Export Contacts
            </el-button>
          </div>
        </el-card>

        <!-- Account Security -->
        <el-card shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Security</span>
          </template>
          
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="font-medium text-gray-900">Two-Factor Authentication</p>
                <p class="text-sm text-gray-600">Add extra security to your account</p>
              </div>
              <el-switch v-model="securitySettings.twoFactorEnabled" @change="toggleTwoFactor" />
            </div>
            
            <el-divider />
            
            <div>
              <p class="font-medium text-gray-900 mb-2">Active Sessions</p>
              <div class="space-y-2">
                <div 
                  v-for="session in activeSessions" 
                  :key="session.id"
                  class="flex items-center justify-between p-2 rounded border"
                >
                  <div class="flex items-center">
                    <el-icon class="text-green-600 mr-2">
                      <SuccessFilled />
                    </el-icon>
                    <div>
                      <p class="text-sm font-medium">{{ session.device }}</p>
                      <p class="text-xs text-gray-500">{{ session.location }}</p>
                    </div>
                  </div>
                  <el-button 
                    v-if="!session.current"
                    @click="terminateSession(session.id)" 
                    size="small"
                    type="danger"
                    text
                  >
                    End
                  </el-button>
                  <el-tag v-else size="small" type="success">Current</el-tag>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format } from 'date-fns'
import {
  Edit,
  Check,
  Upload,
  Download,
  Document,
  Share,
  SuccessFilled
} from '@element-plus/icons-vue'

// Reactive state
const editingBasic = ref(false)
const savingBasic = ref(false)
const changingPassword = ref(false)
const savingPreferences = ref(false)

const userData = ref(null)

const basicForm = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  jobTitle: '',
  department: ''
})

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const preferencesForm = reactive({
  language: 'en',
  timezone: 'UTC',
  dateFormat: 'MM/DD/YYYY',
  theme: 'light',
  emailNotifications: true,
  desktopNotifications: false,
  activityReminders: true
})

const securitySettings = reactive({
  twoFactorEnabled: false
})

const activeSessions = ref([
  {
    id: 1,
    device: 'Windows PC - Chrome',
    location: 'New York, US',
    current: true
  },
  {
    id: 2,
    device: 'iPhone - Safari',
    location: 'New York, US',
    current: false
  }
])

// Methods
const loadUserData = async () => {
  try {
    // API call would go here
    // const response = await userApi.getProfile()
    
    // Mock data
    const mockUser = {
      id: 1,
      firstName: 'Sarah',
      lastName: 'Johnson',
      email: 'sarah.johnson@company.com',
      phone: '+1 (555) 123-4567',
      jobTitle: 'Sales Manager',
      department: 'sales',
      role: 'manager',
      createdAt: '2024-01-15T10:00:00Z',
      lastLoginAt: '2024-03-25T09:30:00Z'
    }
    
    userData.value = mockUser
    
    // Populate forms
    Object.assign(basicForm, {
      firstName: mockUser.firstName,
      lastName: mockUser.lastName,
      email: mockUser.email,
      phone: mockUser.phone,
      jobTitle: mockUser.jobTitle,
      department: mockUser.department
    })
    
  } catch (error) {
    console.error('Error loading user data:', error)
    ElMessage.error('Failed to load profile data')
  }
}

const cancelBasicEdit = () => {
  editingBasic.value = false
  // Reset form to original values
  Object.assign(basicForm, {
    firstName: userData.value.firstName,
    lastName: userData.value.lastName,
    email: userData.value.email,
    phone: userData.value.phone,
    jobTitle: userData.value.jobTitle,
    department: userData.value.department
  })
}

const saveBasicInfo = async () => {
  try {
    savingBasic.value = true
    
    if (!basicForm.firstName || !basicForm.lastName || !basicForm.email) {
      ElMessage.error('First name, last name, and email are required')
      return
    }
    
    // API call would go here
    // await userApi.updateProfile(basicForm)
    
    Object.assign(userData.value, basicForm)
    editingBasic.value = false
    
    ElMessage.success('Profile updated successfully')
    
  } catch (error) {
    console.error('Error saving profile:', error)
    ElMessage.error('Failed to save profile')
  } finally {
    savingBasic.value = false
  }
}

const changePassword = async () => {
  try {
    changingPassword.value = true
    
    if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
      ElMessage.error('All password fields are required')
      return
    }
    
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      ElMessage.error('New passwords do not match')
      return
    }
    
    if (passwordForm.newPassword.length < 8) {
      ElMessage.error('New password must be at least 8 characters long')
      return
    }
    
    // API call would go here
    // await userApi.changePassword(passwordForm)
    
    ElMessage.success('Password changed successfully')
    
    // Reset form
    Object.assign(passwordForm, {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    })
    
  } catch (error) {
    console.error('Error changing password:', error)
    ElMessage.error('Failed to change password')
  } finally {
    changingPassword.value = false
  }
}

const savePreferences = async () => {
  try {
    savingPreferences.value = true
    
    // API call would go here
    // await userApi.updatePreferences(preferencesForm)
    
    ElMessage.success('Preferences saved successfully')
    
  } catch (error) {
    console.error('Error saving preferences:', error)
    ElMessage.error('Failed to save preferences')
  } finally {
    savingPreferences.value = false
  }
}

const toggleTwoFactor = async (enabled) => {
  try {
    if (enabled) {
      // Show setup dialog or redirect to 2FA setup
      ElMessage.info('Two-factor authentication setup will be implemented')
    } else {
      await ElMessageBox.confirm(
        'Are you sure you want to disable two-factor authentication? This will make your account less secure.',
        'Disable Two-Factor Authentication',
        {
          confirmButtonText: 'Disable',
          cancelButtonText: 'Cancel',
          type: 'warning'
        }
      )
      
      // API call would go here
      // await userApi.disableTwoFactor()
      
      ElMessage.success('Two-factor authentication disabled')
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error toggling 2FA:', error)
      ElMessage.error('Failed to update two-factor authentication')
      // Revert the switch
      securitySettings.twoFactorEnabled = !enabled
    } else {
      // User cancelled, revert the switch
      securitySettings.twoFactorEnabled = !enabled
    }
  }
}

const terminateSession = async (sessionId) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to end this session?',
      'Terminate Session',
      {
        confirmButtonText: 'End Session',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    // API call would go here
    // await userApi.terminateSession(sessionId)
    
    const index = activeSessions.value.findIndex(s => s.id === sessionId)
    if (index !== -1) {
      activeSessions.value.splice(index, 1)
    }
    
    ElMessage.success('Session terminated successfully')
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error terminating session:', error)
      ElMessage.error('Failed to terminate session')
    }
  }
}

const uploadAvatar = () => {
  ElMessage.info('Photo upload functionality will be implemented')
}

const downloadData = () => {
  ElMessage.info('Data download functionality will be implemented')
}

const viewActivityLog = () => {
  ElMessage.info('Activity log functionality will be implemented')
}

const exportContacts = () => {
  ElMessage.info('Contact export functionality will be implemented')
}

// Helper functions
const getInitials = () => {
  if (!userData.value) return 'U'
  const first = userData.value.firstName?.charAt(0) || ''
  const last = userData.value.lastName?.charAt(0) || ''
  return (first + last).toUpperCase()
}

const formatDate = (date) => {
  if (!date) return 'Never'
  return format(new Date(date), 'MMM dd, yyyy')
}

// Lifecycle
onMounted(() => {
  loadUserData()
})
</script>

<style scoped>
.profile {
  min-height: calc(100vh - 200px);
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .profile :deep(.el-card__body) {
    padding: 16px;
  }
  
  .profile :deep(.el-form-item__label) {
    text-align: left;
  }
}
</style>