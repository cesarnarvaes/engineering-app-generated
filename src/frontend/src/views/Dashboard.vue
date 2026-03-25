<template>
  <div class="dashboard">
    <!-- Header with quick stats -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold text-gray-900 mb-2">Dashboard</h1>
      <p class="text-gray-600">Welcome back, {{ user.name }}! Here's your CRM overview.</p>
    </div>

    <!-- Quick Stats Cards -->
    <el-row :gutter="20" class="mb-6">
      <el-col :xs="24" :sm="12" :md="6" v-for="stat in quickStats" :key="stat.title">
        <el-card class="stat-card" shadow="hover">
          <div class="flex items-center">
            <div :class="['stat-icon', `bg-${stat.color}-100`]">
              <component :is="stat.icon" :class="`text-${stat.color}-600`" class="h-6 w-6" />
            </div>
            <div class="ml-4 flex-1">
              <p class="text-sm font-medium text-gray-600">{{ stat.title }}</p>
              <p class="text-2xl font-bold text-gray-900">{{ stat.value }}</p>
              <div class="flex items-center mt-1">
                <el-icon 
                  :class="stat.trend === 'up' ? 'text-green-500' : 'text-red-500'" 
                  class="h-4 w-4 mr-1"
                >
                  <component :is="stat.trend === 'up' ? 'ArrowUp' : 'ArrowDown'" />
                </el-icon>
                <span 
                  :class="stat.trend === 'up' ? 'text-green-600' : 'text-red-600'"
                  class="text-sm font-medium"
                >
                  {{ stat.change }}%
                </span>
                <span class="text-gray-500 text-sm ml-1">vs last month</span>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Main Dashboard Content -->
    <el-row :gutter="20">
      <!-- Left Column -->
      <el-col :xs="24" :lg="16">
        <!-- Sales Pipeline Chart -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-blue-600">
                  <TrendCharts />
                </el-icon>
                <span class="text-lg font-semibold">Sales Pipeline</span>
              </div>
              <el-select v-model="pipelineTimeRange" class="w-32" size="small">
                <el-option label="Last 30 days" value="30d" />
                <el-option label="Last 90 days" value="90d" />
                <el-option label="This year" value="1y" />
              </el-select>
            </div>
          </template>
          
          <div class="pipeline-chart h-64">
            <Bar
              :data="pipelineChartData"
              :options="pipelineChartOptions"
              v-if="pipelineChartData.labels.length > 0"
            />
            <div v-else class="flex items-center justify-center h-full text-gray-500">
              <el-icon class="mr-2"><Loading /></el-icon>
              Loading chart data...
            </div>
          </div>
        </el-card>

        <!-- Recent Activities -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-green-600">
                  <Clock />
                </el-icon>
                <span class="text-lg font-semibold">Recent Activities</span>
              </div>
              <el-link type="primary" :underline="false" @click="viewAllActivities">
                View All
              </el-link>
            </div>
          </template>
          
          <div class="activity-list">
            <div 
              v-for="activity in recentActivities" 
              :key="activity.id"
              class="activity-item flex items-start py-3 border-b border-gray-100 last:border-b-0"
            >
              <el-avatar :size="36" class="mr-3">
                <component :is="getActivityIcon(activity.type)" />
              </el-avatar>
              <div class="flex-1">
                <p class="text-sm font-medium text-gray-900">{{ activity.title }}</p>
                <p class="text-sm text-gray-600">{{ activity.description }}</p>
                <div class="flex items-center mt-1">
                  <el-tag :type="getActivityTagType(activity.type)" size="small" class="mr-2">
                    {{ activity.type }}
                  </el-tag>
                  <span class="text-xs text-gray-500">{{ formatDate(activity.createdAt) }}</span>
                </div>
              </div>
              <el-button text type="primary" size="small" @click="viewActivity(activity.id)">
                View
              </el-button>
            </div>
            
            <div v-if="recentActivities.length === 0" class="text-center py-8 text-gray-500">
              <el-icon class="text-4xl mb-2"><Calendar /></el-icon>
              <p>No recent activities</p>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Right Column -->
      <el-col :xs="24" :lg="8">
        <!-- Quick Actions -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center">
              <el-icon class="mr-2 text-purple-600">
                <Lightning />
              </el-icon>
              <span class="text-lg font-semibold">Quick Actions</span>
            </div>
          </template>
          
          <div class="grid grid-cols-2 gap-3">
            <el-button 
              v-for="action in quickActions" 
              :key="action.key"
              :type="action.type"
              class="h-16 flex flex-col items-center justify-center"
              @click="handleQuickAction(action.key)"
            >
              <el-icon class="mb-1">
                <component :is="action.icon" />
              </el-icon>
              <span class="text-xs">{{ action.text }}</span>
            </el-button>
          </div>
        </el-card>

        <!-- Top Opportunities -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-yellow-600">
                  <Star />
                </el-icon>
                <span class="text-lg font-semibold">Top Opportunities</span>
              </div>
              <el-link type="primary" :underline="false" @click="viewAllOpportunities">
                View All
              </el-link>
            </div>
          </template>
          
          <div class="space-y-3">
            <div 
              v-for="opportunity in topOpportunities" 
              :key="opportunity.id"
              class="opportunity-item p-3 border border-gray-100 rounded-lg hover:bg-gray-50 cursor-pointer"
              @click="viewOpportunity(opportunity.id)"
            >
              <div class="flex items-center justify-between mb-2">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ opportunity.title }}</h4>
                <el-tag :type="getStageTagType(opportunity.stage)" size="small">
                  {{ opportunity.stage }}
                </el-tag>
              </div>
              <p class="text-sm text-gray-600 mb-2">{{ opportunity.company?.name }}</p>
              <div class="flex items-center justify-between">
                <span class="text-lg font-bold text-green-600">
                  ${{ formatCurrency(opportunity.estimatedValue) }}
                </span>
                <span class="text-sm text-gray-500">
                  {{ opportunity.probabilityPercentage }}% probability
                </span>
              </div>
            </div>
            
            <div v-if="topOpportunities.length === 0" class="text-center py-8 text-gray-500">
              <el-icon class="text-4xl mb-2"><TrendCharts /></el-icon>
              <p>No opportunities</p>
            </div>
          </div>
        </el-card>

        <!-- Upcoming Tasks -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-red-600">
                  <AlarmClock />
                </el-icon>
                <span class="text-lg font-semibold">Upcoming Tasks</span>
              </div>
              <el-link type="primary" :underline="false" @click="viewAllTasks">
                View All
              </el-link>
            </div>
          </template>
          
          <div class="space-y-3">
            <div 
              v-for="task in upcomingTasks" 
              :key="task.id"
              class="task-item flex items-center p-3 border border-gray-100 rounded-lg hover:bg-gray-50"
            >
              <el-checkbox 
                v-model="task.completed" 
                @change="toggleTask(task.id, task.completed)"
                class="mr-3"
              />
              <div class="flex-1">
                <p :class="['text-sm font-medium', task.completed ? 'line-through text-gray-500' : 'text-gray-900']">
                  {{ task.title }}
                </p>
                <p class="text-xs text-gray-500">{{ formatDate(task.dueDate) }}</p>
              </div>
              <el-tag 
                :type="getTaskPriorityType(task.priority)" 
                size="small"
              >
                {{ task.priority }}
              </el-tag>
            </div>
            
            <div v-if="upcomingTasks.length === 0" class="text-center py-8 text-gray-500">
              <el-icon class="text-4xl mb-2"><Calendar /></el-icon>
              <p>No upcoming tasks</p>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import { format } from 'date-fns'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'

// Icons
import {
  User,
  OfficeBuilding,
  TrendCharts,
  Calendar,
  Clock,
  Lightning,
  Star,
  AlarmClock,
  ArrowUp,
  ArrowDown,
  Loading,
  Plus,
  Phone,
  Message
} from '@element-plus/icons-vue'

// Register Chart.js components
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

// Router and stores
const router = useRouter()
const authStore = useAuthStore()

// Reactive state
const loading = ref(true)
const pipelineTimeRange = ref('30d')

// Sample data (replace with API calls)
const quickStats = ref([
  {
    title: 'Total Contacts',
    value: '1,234',
    change: 12,
    trend: 'up',
    color: 'blue',
    icon: 'User'
  },
  {
    title: 'Active Opportunities',
    value: '56',
    change: 8,
    trend: 'up',
    color: 'green',
    icon: 'TrendCharts'
  },
  {
    title: 'Revenue This Month',
    value: '$84.2K',
    change: -3,
    trend: 'down',
    color: 'yellow',
    icon: 'OfficeBuilding'
  },
  {
    title: 'Tasks Due Today',
    value: '12',
    change: 4,
    trend: 'up',
    color: 'red',
    icon: 'Calendar'
  }
])

const recentActivities = ref([])
const topOpportunities = ref([])
const upcomingTasks = ref([])

// Chart data
const pipelineChartData = ref({
  labels: ['Prospecting', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won'],
  datasets: [
    {
      label: 'Revenue ($)',
      data: [45000, 78000, 120000, 95000, 150000],
      backgroundColor: [
        'rgba(59, 130, 246, 0.8)',
        'rgba(16, 185, 129, 0.8)', 
        'rgba(245, 158, 11, 0.8)',
        'rgba(239, 68, 68, 0.8)',
        'rgba(139, 69, 219, 0.8)'
      ],
      borderColor: [
        'rgba(59, 130, 246, 1)',
        'rgba(16, 185, 129, 1)',
        'rgba(245, 158, 11, 1)',
        'rgba(239, 68, 68, 1)',
        'rgba(139, 69, 219, 1)'
      ],
      borderWidth: 1
    }
  ]
})

const pipelineChartOptions = ref({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    },
    title: {
      display: false
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        callback: function(value) {
          return '$' + (value / 1000) + 'K'
        }
      }
    }
  }
})

// Quick actions
const quickActions = ref([
  { key: 'add-contact', text: 'Add Contact', icon: 'User', type: 'primary' },
  { key: 'add-company', text: 'Add Company', icon: 'OfficeBuilding', type: 'success' },
  { key: 'add-opportunity', text: 'Add Opportunity', icon: 'TrendCharts', type: 'warning' },
  { key: 'schedule-call', text: 'Schedule Call', icon: 'Phone', type: 'info' }
])

// Computed
const user = computed(() => authStore.user)

// Methods
const handleQuickAction = (actionKey) => {
  const routes = {
    'add-contact': '/contacts/new',
    'add-company': '/companies/new', 
    'add-opportunity': '/opportunities/new',
    'schedule-call': '/activities/new'
  }
  
  router.push(routes[actionKey])
}

const viewAllActivities = () => router.push('/activities')
const viewAllOpportunities = () => router.push('/opportunities')
const viewAllTasks = () => router.push('/tasks')

const viewActivity = (activityId) => router.push(`/activities/${activityId}`)
const viewOpportunity = (opportunityId) => router.push(`/opportunities/${opportunityId}`)

const toggleTask = async (taskId, completed) => {
  try {
    // API call to update task status
    ElMessage.success(completed ? 'Task marked as completed' : 'Task marked as incomplete')
  } catch (error) {
    ElMessage.error('Failed to update task status')
  }
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-US').format(amount)
}

const getActivityIcon = (type) => {
  const icons = {
    call: 'Phone',
    email: 'Message', 
    meeting: 'Calendar',
    task: 'Clock'
  }
  return icons[type] || 'Clock'
}

const getActivityTagType = (type) => {
  const types = {
    call: 'primary',
    email: 'success',
    meeting: 'warning',
    task: 'info'
  }
  return types[type] || 'info'
}

const getStageTagType = (stage) => {
  const types = {
    'Prospecting': 'info',
    'Qualification': 'primary', 
    'Proposal': 'warning',
    'Negotiation': 'danger',
    'Closed Won': 'success'
  }
  return types[stage] || 'info'
}

const getTaskPriorityType = (priority) => {
  const types = {
    high: 'danger',
    medium: 'warning',
    low: 'info'
  }
  return types[priority] || 'info'
}

const loadDashboardData = async () => {
  try {
    // Load dashboard data from APIs
    loading.value = true
    
    // Simulate API calls
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Load sample data
    recentActivities.value = [
      {
        id: 1,
        title: 'Follow-up call with John Smith',
        description: 'Discussed project requirements and next steps',
        type: 'call',
        createdAt: new Date().toISOString()
      }
    ]
    
    topOpportunities.value = [
      {
        id: 1,
        title: 'Website Redesign Project',
        company: { name: 'Acme Corp' },
        estimatedValue: 50000,
        probabilityPercentage: 75,
        stage: 'Proposal'
      }
    ]
    
    upcomingTasks.value = [
      {
        id: 1,
        title: 'Prepare proposal for Acme Corp',
        dueDate: new Date().toISOString(),
        priority: 'high',
        completed: false
      }
    ]
    
  } catch (error) {
    console.error('Error loading dashboard data:', error)
    ElMessage.error('Failed to load dashboard data')
  } finally {
    loading.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadDashboardData()
})
</script>

<style scoped>
.dashboard {
  max-width: 100%;
}

.stat-card {
  transition: transform 0.2s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pipeline-chart {
  padding: 1rem 0;
}

.activity-item:hover {
  background-color: #f9fafb;
  border-radius: 8px;
}

.opportunity-item {
  transition: all 0.2s ease;
}

.task-item {
  transition: all 0.2s ease;
}

/* Chart.js responsiveness */
:deep(.chart-container) {
  position: relative;
  height: 100%;
  width: 100%;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .dashboard {
    padding: 0 1rem;
  }
  
  .stat-card {
    margin-bottom: 1rem;
  }
}
</style>