<template>
  <div class="activity-list">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Activities</h1>
        <p class="mt-1 text-gray-600">Manage your calls, emails, meetings, and tasks</p>
      </div>
      
      <!-- Quick Actions -->
      <div class="flex flex-wrap gap-2 mt-4 sm:mt-0">
        <el-button type="primary" @click="createActivity()">
          <el-icon><Plus /></el-icon>
          Add Activity
        </el-button>
        
        <el-dropdown>
          <el-button>
            <el-icon><Clock /></el-icon>
            Today's Schedule
            <el-icon class="ml-2"><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="filterByPeriod('today')">
                <el-icon><Calendar /></el-icon>
                Today
              </el-dropdown-item>
              <el-dropdown-item @click="filterByPeriod('tomorrow')">
                <el-icon><Calendar /></el-icon>
                Tomorrow
              </el-dropdown-item>
              <el-dropdown-item @click="filterByPeriod('week')">
                <el-icon><Calendar /></el-icon>
                This Week
              </el-dropdown-item>
              <el-dropdown-item @click="filterByPeriod('overdue')">
                <el-icon><WarningFilled /></el-icon>
                Overdue
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        
        <el-button @click="refreshActivities">
          <el-icon><Refresh /></el-icon>
        </el-button>
      </div>
    </div>

    <!-- Filters Card -->
    <el-card class="mb-6" shadow="hover">
      <div class="flex flex-wrap gap-4 items-center">
        <!-- Search -->
        <div class="flex-1 min-w-64">
          <el-input
            v-model="searchQuery"
            placeholder="Search activities..."
            clearable
            @input="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>
        
        <!-- Type Filter -->
        <el-select
          v-model="selectedType"
          placeholder="All Types"
          clearable
          class="w-32"
          @change="applyFilters"
        >
          <el-option label="All Types" value="" />
          <el-option label="Call" value="call" />
          <el-option label="Email" value="email" />
          <el-option label="Meeting" value="meeting" />
          <el-option label="Task" value="task" />
        </el-select>
        
        <!-- Status Filter -->
        <el-select
          v-model="selectedStatus"
          placeholder="All Status"
          clearable
          class="w-32"
          @change="applyFilters"
        >
          <el-option label="All Status" value="" />
          <el-option label="Pending" value="pending" />
          <el-option label="Completed" value="completed" />
          <el-option label="Cancelled" value="cancelled" />
        </el-select>
        
        <!-- Date Range -->
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="To"
          start-placeholder="Start date"
          end-placeholder="End date"
          @change="applyFilters"
          class="w-64"
        />
        
        <!-- Assigned To Filter -->
        <el-select
          v-model="selectedAssignee"
          placeholder="All Users"
          clearable
          filterable
          remote
          :remote-method="searchUsers"
          :loading="usersLoading"
          class="w-48"
          @change="applyFilters"
        >
          <el-option
            v-for="user in users"
            :key="user.id"
            :label="user.name"
            :value="user.id"
          />
        </el-select>
        
        <!-- Clear Filters -->
        <el-button 
          @click="clearFilters"
          :disabled="!hasActiveFilters"
        >
          Clear
        </el-button>
      </div>
    </el-card>

    <!-- Stats Cards -->
    <el-row :gutter="20" class="mb-6">
      <el-col :xs="12" :sm="6">
        <el-card shadow="hover" class="stats-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-blue-50">
              <el-icon class="text-2xl text-blue-600"><Clock /></el-icon>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-600">Due Today</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.dueToday }}</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="12" :sm="6">
        <el-card shadow="hover" class="stats-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-red-50">
              <el-icon class="text-2xl text-red-600"><WarningFilled /></el-icon>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-600">Overdue</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.overdue }}</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="12" :sm="6">
        <el-card shadow="hover" class="stats-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-green-50">
              <el-icon class="text-2xl text-green-600"><CircleCheckFilled /></el-icon>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-600">Completed</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.completed }}</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="12" :sm="6">
        <el-card shadow="hover" class="stats-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-purple-50">
              <el-icon class="text-2xl text-purple-600"><DataLine /></el-icon>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-600">This Week</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.thisWeek }}</p>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- View Toggle -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center space-x-4">
        <el-radio-group v-model="viewMode" @change="changeViewMode">
          <el-radio-button value="list">
            <el-icon><List /></el-icon>
            List
          </el-radio-button>
          <el-radio-button value="calendar">
            <el-icon><Calendar /></el-icon>
            Calendar
          </el-radio-button>
          <el-radio-button value="timeline">
            <el-icon><Timer /></el-icon>
            Timeline
          </el-radio-button>
        </el-radio-group>
        
        <!-- Bulk Actions -->
        <div v-if="selectedActivities.length > 0" class="flex items-center space-x-2">
          <span class="text-sm text-gray-600">
            {{ selectedActivities.length }} selected
          </span>
          
          <el-dropdown>
            <el-button size="small">
              Actions
              <el-icon class="ml-1"><ArrowDown /></el-icon>
            </el-button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="bulkComplete">
                  <el-icon><Check /></el-icon>
                  Mark Complete
                </el-dropdown-item>
                <el-dropdown-item @click="bulkReschedule">
                  <el-icon><Calendar /></el-icon>
                  Reschedule
                </el-dropdown-item>
                <el-dropdown-item @click="bulkAssign">
                  <el-icon><User /></el-icon>
                  Reassign
                </el-dropdown-item>
                <el-dropdown-item @click="bulkDelete" class="text-red-600">
                  <el-icon><Delete /></el-icon>
                  Delete
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>
      
      <div class="flex items-center space-x-2">
        <span class="text-sm text-gray-600">
          {{ filteredActivities.length }} activities
        </span>
      </div>
    </div>

    <!-- List View -->
    <el-card v-if="viewMode === 'list'" v-loading="loading" shadow="hover">
      <el-table
        :data="paginatedActivities"
        @selection-change="handleSelectionChange"
        @row-click="viewActivity"
        class="w-full"
        empty-text="No activities found"
        row-class-name="cursor-pointer"
      >
        <el-table-column type="selection" width="55" />
        
        <el-table-column prop="type" label="Type" width="100">
          <template #default="{ row }">
            <el-icon :class="getTypeIconClass(row.type)" class="text-lg">
              <component :is="getTypeIcon(row.type)" />
            </el-icon>
          </template>
        </el-table-column>
        
        <el-table-column prop="title" label="Activity" min-width="200">
          <template #default="{ row }">
            <div>
              <p class="font-medium text-gray-900">{{ row.title }}</p>
              <p class="text-sm text-gray-600 truncate">{{ row.description }}</p>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column prop="relatedTo" label="Related To" width="180">
          <template #default="{ row }">
            <div v-if="row.opportunity || row.contact || row.company">
              <el-link 
                v-if="row.opportunity"
                type="primary" 
                @click.stop="viewOpportunity(row.opportunity.id)"
                class="block"
              >
                {{ row.opportunity.title }}
              </el-link>
              <el-link 
                v-if="row.contact"
                type="primary" 
                @click.stop="viewContact(row.contact.id)"
                class="block text-sm"
              >
                {{ row.contact.firstName }} {{ row.contact.lastName }}
              </el-link>
              <el-link 
                v-if="row.company"
                type="primary" 
                @click.stop="viewCompany(row.company.id)"
                class="block text-sm"
              >
                {{ row.company.name }}
              </el-link>
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>
        
        <el-table-column prop="assignedTo" label="Assigned To" width="140">
          <template #default="{ row }">
            <div v-if="row.assignedTo" class="flex items-center">
              <el-avatar :size="24" class="mr-2">
                {{ row.assignedTo.name.charAt(0) }}
              </el-avatar>
              <span class="text-sm">{{ row.assignedTo.name }}</span>
            </div>
            <span v-else class="text-gray-400">Unassigned</span>
          </template>
        </el-table-column>
        
        <el-table-column prop="dueDate" label="Due Date" width="120" sortable>
          <template #default="{ row }">
            <div v-if="row.dueDate">
              <p :class="getDueDateClass(row.dueDate)">
                {{ formatDate(row.dueDate) }}
              </p>
              <p class="text-xs text-gray-500">{{ formatTime(row.dueDate) }}</p>
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>
        
        <el-table-column prop="status" label="Status" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusTagType(row.status)" size="small">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        
        <el-table-column prop="priority" label="Priority" width="90">
          <template #default="{ row }">
            <el-tag :type="getPriorityTagType(row.priority)" size="small">
              {{ row.priority }}
            </el-tag>
          </template>
        </el-table-column>
        
        <el-table-column label="Actions" width="120" fixed="right">
          <template #default="{ row }">
            <el-button-group>
              <el-button 
                v-if="row.status === 'pending'"
                @click.stop="markComplete(row.id)"
                size="small"
                type="success"
              >
                <el-icon><Check /></el-icon>
              </el-button>
              
              <el-button @click.stop="editActivity(row.id)" size="small">
                <el-icon><Edit /></el-icon>
              </el-button>
              
              <el-dropdown @click.stop>
                <el-button size="small">
                  <el-icon><MoreFilled /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="duplicateActivity(row.id)">
                      <el-icon><CopyDocument /></el-icon>
                      Duplicate
                    </el-dropdown-item>
                    <el-dropdown-item @click="rescheduleActivity(row.id)">
                      <el-icon><Calendar /></el-icon>
                      Reschedule
                    </el-dropdown-item>
                    <el-dropdown-item @click="deleteActivity(row.id)" class="text-red-600">
                      <el-icon><Delete /></el-icon>
                      Delete
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>
      
      <!-- Pagination -->
      <div class="flex justify-between items-center mt-4">
        <div class="text-sm text-gray-700">
          Showing {{ paginationStart }} to {{ paginationEnd }} of {{ filteredActivities.length }} activities
        </div>
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="filteredActivities.length"
          layout="sizes, prev, pager, next"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>

    <!-- Calendar View -->
    <el-card v-else-if="viewMode === 'calendar'" shadow="hover">
      <div class="calendar-view">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center space-x-4">
            <el-button @click="previousMonth">
              <el-icon><ArrowLeft /></el-icon>
            </el-button>
            <h3 class="text-lg font-semibold">{{ currentMonthYear }}</h3>
            <el-button @click="nextMonth">
              <el-icon><ArrowRight /></el-icon>
            </el-button>
          </div>
          <el-button @click="goToToday">Today</el-button>
        </div>
        
        <div class="grid grid-cols-7 gap-1 mb-2">
          <div
            v-for="day in ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']"
            :key="day"
            class="p-2 text-center text-sm font-medium text-gray-500"
          >
            {{ day }}
          </div>
        </div>
        
        <div class="grid grid-cols-7 gap-1">
          <div
            v-for="date in calendarDates"
            :key="`${date.date}-${date.month}`"
            :class="[
              'min-h-24 p-1 border border-gray-200 cursor-pointer transition-colors',
              date.isCurrentMonth ? 'bg-white' : 'bg-gray-50',
              date.isToday ? 'bg-blue-50 border-blue-200' : '',
              'hover:bg-gray-100'
            ]"
            @click="selectDate(date)"
          >
            <div class="text-sm">
              <span :class="[
                'inline-block w-6 h-6 text-center rounded-full',
                date.isToday ? 'bg-blue-600 text-white' : '',
                !date.isCurrentMonth ? 'text-gray-400' : ''
              ]">
                {{ date.date }}
              </span>
            </div>
            
            <div class="mt-1 space-y-1">
              <div
                v-for="activity in getActivitiesForDate(date.fullDate)"
                :key="activity.id"
                :class="[
                  'text-xs p-1 rounded truncate cursor-pointer',
                  getActivityCalendarClass(activity.type)
                ]"
                @click.stop="viewActivity(activity)"
              >
                {{ activity.title }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- Timeline View -->
    <el-card v-else-if="viewMode === 'timeline'" shadow="hover">
      <el-timeline>
        <el-timeline-item
          v-for="activity in timelineActivities"
          :key="activity.id"
          :timestamp="formatDateTime(activity.dueDate || activity.createdAt)"
          :type="getTimelineType(activity.type)"
          placement="top"
        >
          <el-card shadow="never" class="timeline-activity-card">
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <div class="flex items-center mb-2">
                  <el-icon :class="getTypeIconClass(activity.type)" class="mr-2">
                    <component :is="getTypeIcon(activity.type)" />
                  </el-icon>
                  <h4 class="text-sm font-medium text-gray-900">{{ activity.title }}</h4>
                  <el-tag :type="getStatusTagType(activity.status)" size="small" class="ml-2">
                    {{ activity.status }}
                  </el-tag>
                </div>
                <p class="text-sm text-gray-600 mb-2">{{ activity.description }}</p>
                
                <div class="flex items-center space-x-4 text-xs text-gray-500">
                  <div v-if="activity.assignedTo" class="flex items-center">
                    <el-icon class="mr-1"><User /></el-icon>
                    {{ activity.assignedTo.name }}
                  </div>
                  <div v-if="activity.opportunity" class="flex items-center">
                    <el-icon class="mr-1"><TrendCharts /></el-icon>
                    {{ activity.opportunity.title }}
                  </div>
                </div>
              </div>
              
              <el-dropdown>
                <el-button text size="small">
                  <el-icon><MoreFilled /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="viewActivity(activity)">
                      <el-icon><View /></el-icon>
                      View
                    </el-dropdown-item>
                    <el-dropdown-item @click="editActivity(activity.id)">
                      <el-icon><Edit /></el-icon>
                      Edit
                    </el-dropdown-item>
                    <el-dropdown-item v-if="activity.status === 'pending'" @click="markComplete(activity.id)">
                      <el-icon><Check /></el-icon>
                      Complete
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </el-card>
        </el-timeline-item>
      </el-timeline>
      
      <div v-if="timelineActivities.length === 0" class="text-center py-8 text-gray-500">
        <el-icon class="text-4xl mb-2"><Clock /></el-icon>
        <p>No activities to display</p>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format, startOfMonth, endOfMonth, eachDayOfInterval, isSameMonth, isToday, addMonths, subMonths, isSameDay, isPast } from 'date-fns'
import {
  Plus,
  Clock,
  ArrowDown,
  Calendar,
  WarningFilled,
  Refresh,
  Search,
  List,
  Timer,
  Check,
  User,
  Delete,
  CircleCheckFilled,
  DataLine,
  Edit,
  MoreFilled,
  CopyDocument,
  ArrowLeft,
  ArrowRight,
  Phone,
  Message,
  VideoCamera,
  Setting,
  TrendCharts,
  View
} from '@element-plus/icons-vue'

// Router
const router = useRouter()

// Reactive state
const loading = ref(true)
const usersLoading = ref(false)
const searchQuery = ref('')
const selectedType = ref('')
const selectedStatus = ref('')
const selectedAssignee = ref(null)
const dateRange = ref([])
const currentPage = ref(1)
const pageSize = ref(20)
const viewMode = ref('list')
const selectedActivities = ref([])
const currentDate = ref(new Date())

const users = ref([])
const activities = ref([])

const stats = reactive({
  dueToday: 0,
  overdue: 0,
  completed: 0,
  thisWeek: 0
})

// Mock data
const mockActivities = [
  {
    id: 1,
    title: 'Follow up call with John Smith',
    description: 'Discuss project timeline and next steps',
    type: 'call',
    status: 'pending',
    priority: 'high',
    dueDate: '2024-03-25T10:00:00Z',
    assignedTo: { id: 1, name: 'Sarah Johnson' },
    opportunity: { id: 1, title: 'Enterprise Platform Upgrade' },
    contact: { id: 1, firstName: 'John', lastName: 'Smith' },
    company: { id: 1, name: 'Acme Corp' },
    createdAt: '2024-03-20T09:00:00Z'
  },
  {
    id: 2,
    title: 'Send proposal document',
    description: 'Email the detailed proposal with pricing',
    type: 'email',
    status: 'completed',
    priority: 'medium',
    dueDate: '2024-03-24T14:00:00Z',
    assignedTo: { id: 1, name: 'Sarah Johnson' },
    opportunity: { id: 1, title: 'Enterprise Platform Upgrade' },
    contact: { id: 1, firstName: 'John', lastName: 'Smith' },
    company: { id: 1, name: 'Acme Corp' },
    createdAt: '2024-03-20T09:00:00Z'
  },
  {
    id: 3,
    title: 'Product demo meeting',
    description: 'Demonstrate new features to the client team',
    type: 'meeting',
    status: 'pending',
    priority: 'high',
    dueDate: '2024-03-26T15:00:00Z',
    assignedTo: { id: 2, name: 'Mike Chen' },
    opportunity: { id: 2, title: 'Software Integration' },
    contact: { id: 2, firstName: 'Jane', lastName: 'Doe' },
    company: { id: 2, name: 'Tech Solutions' },
    createdAt: '2024-03-20T09:00:00Z'
  }
]

// Computed properties
const filteredActivities = computed(() => {
  let filtered = activities.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(activity =>
      activity.title.toLowerCase().includes(query) ||
      activity.description.toLowerCase().includes(query)
    )
  }

  if (selectedType.value) {
    filtered = filtered.filter(activity => activity.type === selectedType.value)
  }

  if (selectedStatus.value) {
    filtered = filtered.filter(activity => activity.status === selectedStatus.value)
  }

  if (selectedAssignee.value) {
    filtered = filtered.filter(activity => activity.assignedTo?.id === selectedAssignee.value)
  }

  if (dateRange.value && dateRange.value.length === 2) {
    const [start, end] = dateRange.value
    filtered = filtered.filter(activity => {
      if (!activity.dueDate) return false
      const activityDate = new Date(activity.dueDate)
      return activityDate >= start && activityDate <= end
    })
  }

  return filtered
})

const paginatedActivities = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredActivities.value.slice(start, end)
})

const paginationStart = computed(() => {
  return Math.min((currentPage.value - 1) * pageSize.value + 1, filteredActivities.value.length)
})

const paginationEnd = computed(() => {
  return Math.min(currentPage.value * pageSize.value, filteredActivities.value.length)
})

const hasActiveFilters = computed(() => {
  return !!(searchQuery.value || selectedType.value || selectedStatus.value || 
            selectedAssignee.value || (dateRange.value && dateRange.value.length === 2))
})

const currentMonthYear = computed(() => {
  return format(currentDate.value, 'MMMM yyyy')
})

const calendarDates = computed(() => {
  const start = startOfMonth(currentDate.value)
  const end = endOfMonth(currentDate.value)
  
  // Get first day of the month's week
  const startDate = new Date(start)
  startDate.setDate(startDate.getDate() - startDate.getDay())
  
  // Get last day of the month's week
  const endDate = new Date(end)
  endDate.setDate(endDate.getDate() + (6 - endDate.getDay()))
  
  const dates = eachDayOfInterval({ start: startDate, end: endDate })
  
  return dates.map(date => ({
    date: date.getDate(),
    month: date.getMonth(),
    fullDate: date,
    isCurrentMonth: isSameMonth(date, currentDate.value),
    isToday: isToday(date)
  }))
})

const timelineActivities = computed(() => {
  return [...filteredActivities.value].sort((a, b) => {
    const dateA = new Date(a.dueDate || a.createdAt)
    const dateB = new Date(b.dueDate || b.createdAt)
    return dateB - dateA
  })
})

// Methods
const loadActivities = async () => {
  try {
    loading.value = true
    
    // API call would go here
    // const response = await activitiesApi.getActivities()
    
    activities.value = mockActivities
    calculateStats()
    
  } catch (error) {
    console.error('Error loading activities:', error)
    ElMessage.error('Failed to load activities')
  } finally {
    loading.value = false
  }
}

const calculateStats = () => {
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  
  stats.dueToday = activities.value.filter(a => {
    if (!a.dueDate || a.status === 'completed') return false
    const dueDate = new Date(a.dueDate)
    return isSameDay(dueDate, today)
  }).length
  
  stats.overdue = activities.value.filter(a => {
    if (!a.dueDate || a.status === 'completed') return false
    const dueDate = new Date(a.dueDate)
    return isPast(dueDate) && !isSameDay(dueDate, today)
  }).length
  
  stats.completed = activities.value.filter(a => a.status === 'completed').length
  
  // This week calculation would be more precise with proper date logic
  stats.thisWeek = activities.value.filter(a => {
    if (!a.dueDate) return false
    const dueDate = new Date(a.dueDate)
    const weekStart = new Date(now)
    weekStart.setDate(now.getDate() - now.getDay())
    const weekEnd = new Date(weekStart)
    weekEnd.setDate(weekStart.getDate() + 6)
    return dueDate >= weekStart && dueDate <= weekEnd
  }).length
}

const searchUsers = async (query) => {
  if (!query) return
  usersLoading.value = true
  try {
    users.value = [
      { id: 1, name: 'Sarah Johnson' },
      { id: 2, name: 'Mike Chen' }
    ]
  } finally {
    usersLoading.value = false
  }
}

const handleSearch = () => {
  currentPage.value = 1
}

const applyFilters = () => {
  currentPage.value = 1
}

const clearFilters = () => {
  searchQuery.value = ''
  selectedType.value = ''
  selectedStatus.value = ''
  selectedAssignee.value = null
  dateRange.value = []
  currentPage.value = 1
}

const filterByPeriod = (period) => {
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  
  switch (period) {
    case 'today':
      dateRange.value = [today, today]
      break
    case 'tomorrow':
      const tomorrow = new Date(today)
      tomorrow.setDate(today.getDate() + 1)
      dateRange.value = [tomorrow, tomorrow]
      break
    case 'week':
      const weekStart = new Date(today)
      weekStart.setDate(today.getDate() - today.getDay())
      const weekEnd = new Date(weekStart)
      weekEnd.setDate(weekStart.getDate() + 6)
      dateRange.value = [weekStart, weekEnd]
      break
    case 'overdue':
      selectedStatus.value = 'pending'
      // Filter for overdue items would need additional logic
      break
  }
  applyFilters()
}

const refreshActivities = () => {
  loadActivities()
}

const changeViewMode = () => {
  // View mode changed
}

const handleSelectionChange = (selected) => {
  selectedActivities.value = selected
}

const handleSizeChange = (newSize) => {
  pageSize.value = newSize
  currentPage.value = 1
}

const handlePageChange = (newPage) => {
  currentPage.value = newPage
}

const createActivity = (params = {}) => {
  const queryParams = new URLSearchParams(params).toString()
  router.push(`/activities/new${queryParams ? '?' + queryParams : ''}`)
}

const viewActivity = (activity) => {
  router.push(`/activities/${activity.id}`)
}

const editActivity = (activityId) => {
  router.push(`/activities/${activityId}/edit`)
}

const duplicateActivity = (activityId) => {
  router.push(`/activities/new?duplicate=${activityId}`)
}

const markComplete = async (activityId) => {
  try {
    // API call would go here
    const activity = activities.value.find(a => a.id === activityId)
    if (activity) {
      activity.status = 'completed'
      calculateStats()
      ElMessage.success('Activity marked as completed')
    }
  } catch (error) {
    console.error('Error marking complete:', error)
    ElMessage.error('Failed to complete activity')
  }
}

const rescheduleActivity = (activityId) => {
  ElMessage.info('Reschedule functionality will be implemented')
}

const deleteActivity = async (activityId) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this activity?',
      'Delete Activity',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    const index = activities.value.findIndex(a => a.id === activityId)
    if (index !== -1) {
      activities.value.splice(index, 1)
      calculateStats()
      ElMessage.success('Activity deleted successfully')
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting activity:', error)
      ElMessage.error('Failed to delete activity')
    }
  }
}

const bulkComplete = () => {
  selectedActivities.value.forEach(activity => {
    activity.status = 'completed'
  })
  calculateStats()
  selectedActivities.value = []
  ElMessage.success('Activities marked as completed')
}

const bulkReschedule = () => {
  ElMessage.info('Bulk reschedule functionality will be implemented')
}

const bulkAssign = () => {
  ElMessage.info('Bulk assign functionality will be implemented')
}

const bulkDelete = async () => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete ${selectedActivities.value.length} activities?`,
      'Delete Activities',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    const idsToDelete = selectedActivities.value.map(a => a.id)
    activities.value = activities.value.filter(a => !idsToDelete.includes(a.id))
    calculateStats()
    selectedActivities.value = []
    ElMessage.success('Activities deleted successfully')
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting activities:', error)
      ElMessage.error('Failed to delete activities')
    }
  }
}

const viewOpportunity = (opportunityId) => {
  router.push(`/opportunities/${opportunityId}`)
}

const viewContact = (contactId) => {
  router.push(`/contacts/${contactId}`)
}

const viewCompany = (companyId) => {
  router.push(`/companies/${companyId}`)
}

// Calendar methods
const previousMonth = () => {
  currentDate.value = subMonths(currentDate.value, 1)
}

const nextMonth = () => {
  currentDate.value = addMonths(currentDate.value, 1)
}

const goToToday = () => {
  currentDate.value = new Date()
}

const selectDate = (date) => {
  // Handle date selection for creating activities
  createActivity({ date: format(date.fullDate, 'yyyy-MM-dd') })
}

const getActivitiesForDate = (date) => {
  return activities.value.filter(activity => {
    if (!activity.dueDate) return false
    return isSameDay(new Date(activity.dueDate), date)
  })
}

// Helper functions
const getTypeIcon = (type) => {
  const icons = {
    call: Phone,
    email: Message,
    meeting: VideoCamera,
    task: Setting
  }
  return icons[type] || Clock
}

const getTypeIconClass = (type) => {
  const classes = {
    call: 'text-blue-600',
    email: 'text-green-600',
    meeting: 'text-purple-600',
    task: 'text-orange-600'
  }
  return classes[type] || 'text-gray-600'
}

const getStatusTagType = (status) => {
  const types = {
    pending: 'warning',
    completed: 'success',
    cancelled: 'info'
  }
  return types[status] || 'info'
}

const getPriorityTagType = (priority) => {
  const types = {
    high: 'danger',
    medium: 'warning',
    low: 'success'
  }
  return types[priority] || 'info'
}

const getDueDateClass = (dueDate) => {
  const date = new Date(dueDate)
  const now = new Date()
  
  if (isPast(date) && !isSameDay(date, now)) {
    return 'text-red-600 font-medium'
  } else if (isSameDay(date, now)) {
    return 'text-orange-600 font-medium'
  } else {
    return 'text-gray-900'
  }
}

const getActivityCalendarClass = (type) => {
  const classes = {
    call: 'bg-blue-100 text-blue-800',
    email: 'bg-green-100 text-green-800',
    meeting: 'bg-purple-100 text-purple-800',
    task: 'bg-orange-100 text-orange-800'
  }
  return classes[type] || 'bg-gray-100 text-gray-800'
}

const getTimelineType = (type) => {
  const types = {
    call: 'primary',
    email: 'success',
    meeting: 'warning',
    task: 'info'
  }
  return types[type] || 'info'
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd')
}

const formatTime = (date) => {
  return format(new Date(date), 'HH:mm')
}

const formatDateTime = (date) => {
  return format(new Date(date), 'MMM dd, yyyy HH:mm')
}

// Lifecycle
onMounted(() => {
  loadActivities()
})
</script>

<style scoped>
.activity-list {
  min-height: calc(100vh - 200px);
}

.stats-card {
  transition: transform 0.2s ease-in-out;
}

.stats-card:hover {
  transform: translateY(-2px);
}

.timeline-activity-card {
  border: 1px solid #e5e7eb;
  transition: all 0.2s ease-in-out;
}

.timeline-activity-card:hover {
  border-color: #d1d5db;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

/* Grid utilities for calendar */
.grid {
  display: grid;
}

.grid-cols-7 {
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.gap-1 {
  gap: 0.25rem;
}

.gap-4 {
  gap: 1rem;
}

.min-h-24 {
  min-height: 6rem;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .activity-list :deep(.el-table) {
    font-size: 0.875rem;
  }
  
  .activity-list :deep(.el-table-column) {
    padding: 8px 0;
  }
}
</style>