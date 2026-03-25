<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Logo and title -->
      <div class="text-center">
        <div class="flex justify-center items-center mb-6">
          <el-icon class="text-4xl text-blue-600 mr-3">
            <OfficeBuilding />
          </el-icon>
          <h1 class="text-3xl font-bold text-gray-900">BizCRM</h1>
        </div>
        <h2 class="text-xl text-gray-700 font-medium">
          Sign in to your account
        </h2>
        <p class="mt-2 text-sm text-gray-600">
          Welcome back! Please enter your details.
        </p>
      </div>

      <!-- Login form -->
      <el-card class="login-card" shadow="hover">
        <el-form
          ref="loginFormRef"
          :model="loginForm"
          :rules="formRules"
          class="login-form"
          size="large"
          @submit.prevent="handleLogin"
        >
          <!-- Email field -->
          <el-form-item prop="email">
            <el-input
              v-model="loginForm.email"
              type="email"
              placeholder="Email address"
              prefix-icon="Message"
              :disabled="loading"
              clearable
              @keyup.enter="handleLogin"
            />
          </el-form-item>

          <!-- Password field -->
          <el-form-item prop="password">
            <el-input
              v-model="loginForm.password"
              type="password"
              placeholder="Password"
              prefix-icon="Lock"
              :disabled="loading"
              show-password
              @keyup.enter="handleLogin"
            />
          </el-form-item>

          <!-- Remember me and forgot password -->
          <div class="flex items-center justify-between mb-6">
            <el-checkbox v-model="loginForm.rememberMe" :disabled="loading">
              Remember me
            </el-checkbox>
            <el-link type="primary" :underline="false" @click="showForgotPassword">
              Forgot password?
            </el-link>
          </div>

          <!-- Login button -->
          <el-form-item>
            <el-button
              type="primary"
              class="w-full"
              size="large"
              :loading="loading"
              @click="handleLogin"
            >
              <el-icon v-if="!loading" class="mr-2">
                <User />
              </el-icon>
              {{ loading ? 'Signing in...' : 'Sign in' }}
            </el-button>
          </el-form-item>

          <!-- Demo users (for development) -->
          <div v-if="isDevelopment" class="mt-6 pt-6 border-t border-gray-200">
            <p class="text-sm text-gray-500 mb-3 text-center">Demo Users:</p>
            <div class="space-y-2">
              <el-button 
                v-for="demoUser in demoUsers" 
                :key="demoUser.email"
                text 
                size="small"
                class="w-full"
                @click="fillDemoUser(demoUser)"
              >
                {{ demoUser.role }} - {{ demoUser.email }}
              </el-button>
            </div>
          </div>
        </el-form>
      </el-card>

      <!-- Error handling -->
      <el-alert
        v-if="errorMessage"
        :title="errorMessage"
        type="error"
        :closable="true"
        show-icon
        @close="errorMessage = ''"
      />

      <!-- Additional links -->
      <div class="text-center text-sm text-gray-600">
        <p>
          Need an account? 
          <el-link type="primary" :underline="false" @click="showRegistration">
            Contact your administrator
          </el-link>
        </p>
      </div>
    </div>

    <!-- Forgot Password Dialog -->
    <el-dialog
      v-model="showForgotPasswordDialog"
      title="Reset Password"
      width="30%"
      :close-on-click-modal="false"
    >
      <el-form
        ref="forgotPasswordFormRef"
        :model="forgotPasswordForm"
        :rules="forgotPasswordRules"
      >
        <el-form-item label="Email" prop="email">
          <el-input
            v-model="forgotPasswordForm.email"
            type="email"
            placeholder="Enter your email address"
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showForgotPasswordDialog = false">Cancel</el-button>
          <el-button 
            type="primary" 
            :loading="forgotPasswordLoading"
            @click="handleForgotPassword"
          >
            Send Reset Link
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive} from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import { OfficeBuilding, User, Message, Lock } from '@element-plus/icons-vue'

// Router and stores
const router = useRouter()
const authStore = useAuthStore()

// Form refs
const loginFormRef = ref()
const forgotPasswordFormRef = ref()

// Reactive state
const loading = ref(false)
const errorMessage = ref('')
const showForgotPasswordDialog = ref(false)
const forgotPasswordLoading = ref(false)
const isDevelopment = ref(import.meta.env.DEV)

// Form data
const loginForm = reactive({
  email: '',
  password: '',
  rememberMe: false
})

const forgotPasswordForm = reactive({
  email: ''
})

// Form validation rules
const formRules = {
  email: [
    { required: true, message: 'Please input your email', trigger: 'blur' },
    { type: 'email', message: 'Please input a valid email', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Please input your password', trigger: 'blur' },
    { min: 6, message: 'Password must be at least 6 characters', trigger: 'blur' }
  ]
}

const forgotPasswordRules = {
  email: [
    { required: true, message: 'Please input your email', trigger: 'blur' },
    { type: 'email', message: 'Please input a valid email', trigger: 'blur' }
  ]
}

// Demo users for development
const demoUsers = [
  { email: 'admin@example.com', password: 'admin123', role: 'System Admin' },
  { email: 'manager@example.com', password: 'manager123', role: 'Manager' },
  { email: 'user@example.com', password: 'user123', role: 'Business User' }
]

// Methods
const handleLogin = async () => {
  try {
    const valid = await loginFormRef.value?.validate()
    if (!valid) return

    loading.value = true
    errorMessage.value = ''

    await authStore.login({
      email: loginForm.email,
      password: loginForm.password,
      rememberMe: loginForm.rememberMe
    })

    ElMessage.success('Login successful')
    
    // Redirect to dashboard or intended route
    const redirectPath = router.currentRoute.value.query.redirect || '/dashboard'
    await router.push(redirectPath)

  } catch (error) {
    console.error('Login error:', error)
    errorMessage.value = error.message || 'Login failed. Please try again.'
  } finally {
    loading.value = false
  }
}

const handleForgotPassword = async () => {
  try {
    const valid = await forgotPasswordFormRef.value?.validate()
    if (!valid) return

    forgotPasswordLoading.value = true

    await authStore.forgotPassword(forgotPasswordForm.email)

    ElMessage.success('Password reset link sent to your email')
    showForgotPasswordDialog.value = false
    forgotPasswordForm.email = ''

  } catch (error) {
    console.error('Forgot password error:', error)
    ElMessage.error(error.message || 'Failed to send reset link')
  } finally {
    forgotPasswordLoading.value = false
  }
}

const showForgotPassword = () => {
  forgotPasswordForm.email = loginForm.email
  showForgotPasswordDialog.value = true
}

const showRegistration = () => {
  ElMessage.info('Please contact your system administrator to create an account')
}

const fillDemoUser = (demoUser) => {
  loginForm.email = demoUser.email
  loginForm.password = demoUser.password
}
</script>

<style scoped>
.login-card {
  border: none;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

.login-card :deep(.el-card__body) {
  padding: 2rem;
}

.login-form {
  margin-top: 0;
}

.login-form :deep(.el-form-item) {
  margin-bottom: 1.5rem;
}

.login-form :deep(.el-input__inner) {
  height: 48px;
  border-radius: 8px;
  border: 1px solid #e5e7eb;
  transition: all 0.3s ease;
}

.login-form :deep(.el-input__inner:focus) {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.login-form :deep(.el-button--primary) {
  height: 48px;
  border-radius: 8px;
  font-weight: 500;
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  border: none;
  transition: all 0.3s ease;
}

.login-form :deep(.el-button--primary:hover) {
  background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

/* Animation for form appearance */
.login-card {
  animation: slideUp 0.6s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Mobile responsiveness */
@media (max-width: 640px) {
  .login-card :deep(.el-card__body) {
    padding: 1.5rem;
  }
}
</style>