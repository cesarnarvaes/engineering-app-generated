<template>
  <div class="reports">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Reports</h1>
        <p class="mt-1 text-gray-600">Generate and analyze business insights</p>
      </div>
      
      <div class="flex flex-wrap gap-2 mt-4 sm:mt-0">
        <el-button type="primary" @click="createCustomReport">
          <el-icon><DocumentAdd /></el-icon>
          Create Report
        </el-button>
        
        <el-dropdown>
          <el-button>
            <el-icon><Download /></el-icon>
            Export
            <el-icon class="ml-2"><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="exportReport('excel')">
                <el-icon><Document /></el-icon>
                Excel
              </el-dropdown-item>
              <el-dropdown-item @click="exportReport('pdf')">
                <el-icon><Document /></el-icon>
                PDF
              </el-dropdown-item>
              <el-dropdown-item @click="exportReport('csv')">
                <el-icon><Document /></el-icon>
                CSV
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        
        <el-button @click="refreshReports">
          <el-icon><Refresh /></el-icon>
        </el-button>
      </div>
    </div>

    <!-- Report Categories -->
    <el-row :gutter="20" class="mb-6">
      <el-col :xs="24" :sm="12" :lg="6">
        <el-card shadow="hover" class="report-category-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-blue-50">
              <el-icon class="text-2xl text-blue-600"><TrendCharts /></el-icon>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-semibold text-gray-900">Sales</h3>
              <p class="text-sm text-gray-600">{{ salesReports.length }} reports</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="12" :lg="6">
        <el-card shadow="hover" class="report-category-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-green-50">
              <el-icon class="text-2xl text-green-600"><User /></el-icon>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-semibold text-gray-900">Activities</h3>
              <p class="text-sm text-gray-600">{{ activityReports.length }} reports</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="12" :lg="6">
        <el-card shadow="hover" class="report-category-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-purple-50">
              <el-icon class="text-2xl text-purple-600"><OfficeBuilding /></el-icon>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-semibold text-gray-900">Companies</h3>
              <p class="text-sm text-gray-600">{{ companyReports.length }} reports</p>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="12" :lg="6">
        <el-card shadow="hover" class="report-category-card">
          <div class="flex items-center">
            <div class="p-3 rounded-lg bg-orange-50">
              <el-icon class="text-2xl text-orange-600"><DataAnalysis /></el-icon>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-semibold text-gray-900">Custom</h3>
              <p class="text-sm text-gray-600">{{ customReports.length }} reports</p>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Quick Reports -->
    <el-card class="mb-6" shadow="hover">
      <template #header>
        <div class="flex items-center justify-between">
          <span class="text-lg font-semibold">Quick Reports</span>
          <el-link type="primary" @click="viewAllReports">View All</el-link>
        </div>
      </template>
      
      <el-row :gutter="20">
        <el-col :xs="24" :sm="12" :lg="8" v-for="report in quickReports" :key="report.id">
          <div 
            class="p-4 border border-gray-200 rounded-lg hover:border-blue-300 cursor-pointer transition-colors"
            @click="generateReport(report)"
          >
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <h4 class="text-md font-medium text-gray-900 mb-2">{{ report.title }}</h4>
                <p class="text-sm text-gray-600 mb-3">{{ report.description }}</p>
                
                <div class="flex items-center justify-between">
                  <el-tag :type="getCategoryTagType(report.category)" size="small">
                    {{ report.category }}
                  </el-tag>
                  <span class="text-xs text-gray-500">{{ report.estimatedTime }}</span>
                </div>
              </div>
              
              <el-icon class="text-gray-400 ml-2">
                <component :is="getCategoryIcon(report.category)" />
              </el-icon>
            </div>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- Recent Reports -->
    <el-card shadow="hover" v-loading="loading">
      <template #header>
        <div class="flex items-center justify-between">
          <span class="text-lg font-semibold">Recent Reports</span>
          <el-button @click="loadReports" size="small">
            <el-icon><Refresh /></el-icon>
            Refresh
          </el-button>
        </div>
      </template>
      
      <el-table
        :data="recentReports"
        @row-click="viewReport"
        empty-text="No reports generated yet"
        row-class-name="cursor-pointer"
      >
        <el-table-column prop="name" label="Report Name" min-width="200">
          <template #default="{ row }">
            <div class="flex items-center">
              <el-icon class="mr-2" :class="getCategoryIconClass(row.category)">
                <component :is="getCategoryIcon(row.category)" />
              </el-icon>
              <div>
                <p class="font-medium text-gray-900">{{ row.name }}</p>
                <p class="text-sm text-gray-600">{{ row.description }}</p>
              </div>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column prop="category" label="Category" width="120">
          <template #default="{ row }">
            <el-tag :type="getCategoryTagType(row.category)" size="small">
              {{ row.category }}
            </el-tag>
          </template>
        </el-table-column>
        
        <el-table-column prop="createdBy" label="Created By" width="140">
          <template #default="{ row }">
            <div class="flex items-center">
              <el-avatar :size="24" class="mr-2">
                {{ row.createdBy.name.charAt(0) }}
              </el-avatar>
              <span class="text-sm">{{ row.createdBy.name }}</span>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column prop="createdAt" label="Created At" width="140" sortable>
          <template #default="{ row }">
            <div>
              <p class="text-sm text-gray-900">{{ formatDate(row.createdAt) }}</p>
              <p class="text-xs text-gray-500">{{ formatTime(row.createdAt) }}</p>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column prop="status" label="Status" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusTagType(row.status)" size="small">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        
        <el-table-column label="Actions" width="120" fixed="right">
          <template #default="{ row }">
            <el-button-group>
              <el-button @click.stop="viewReport(row)" size="small">
                <el-icon><View /></el-icon>
              </el-button>
              
              <el-button @click.stop="downloadReport(row.id)" size="small">
                <el-icon><Download /></el-icon>
              </el-button>
              
              <el-dropdown @click.stop>
                <el-button size="small">
                  <el-icon><MoreFilled /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="duplicateReport(row.id)">
                      <el-icon><CopyDocument /></el-icon>
                      Duplicate
                    </el-dropdown-item>
                    <el-dropdown-item @click="scheduleReport(row.id)">
                      <el-icon><Timer /></el-icon>
                      Schedule
                    </el-dropdown-item>
                    <el-dropdown-item @click="shareReport(row.id)">
                      <el-icon><Share /></el-icon>
                      Share
                    </el-dropdown-item>
                    <el-dropdown-item @click="deleteReport(row.id)" class="text-red-600">
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
          Showing {{ paginationStart }} to {{ paginationEnd }} of {{ totalReports }} reports
        </div>
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalReports"
          layout="sizes, prev, pager, next"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>

    <!-- Report Generation Dialog -->
    <el-dialog
      v-model="showReportDialog"
      title="Generate Report"
      width="50%"
      :close-on-click-modal="false"
    >
      <el-form :model="reportForm" label-width="120px">
        <el-form-item label="Report Type">
          <el-select v-model="reportForm.type" placeholder="Select report type" class="w-full">
            <el-option
              v-for="report in quickReports"
              :key="report.id"
              :label="report.title"
              :value="report.id"
            />
          </el-select>
        </el-form-item>
        
        <el-form-item label="Date Range">
          <el-date-picker
            v-model="reportForm.dateRange"
            type="daterange"
            range-separator="To"
            start-placeholder="Start date"
            end-placeholder="End date"
            class="w-full"
          />
        </el-form-item>
        
        <el-form-item label="Format">
          <el-radio-group v-model="reportForm.format">
            <el-radio value="pdf">PDF</el-radio>
            <el-radio value="excel">Excel</el-radio>
            <el-radio value="csv">CSV</el-radio>
          </el-radio-group>
        </el-form-item>
        
        <el-form-item label="Email Results">
          <el-switch v-model="reportForm.emailResults" />
        </el-form-item>
        
        <el-form-item v-if="reportForm.emailResults" label="Email Address">
          <el-input v-model="reportForm.email" placeholder="Enter email address" />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showReportDialog = false">Cancel</el-button>
          <el-button 
            type="primary" 
            :loading="generating"
            @click="submitReportGeneration"
          >
            Generate Report
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format } from 'date-fns'
import {
  DocumentAdd,
  Download,
  ArrowDown,
  Refresh,
  TrendCharts,
  User,
  OfficeBuilding,
  DataAnalysis,
  Document,
  View,
  MoreFilled,
  CopyDocument,
  Timer,
  Share,
  Delete
} from '@element-plus/icons-vue'

// Router
const router = useRouter()

// Reactive state
const loading = ref(false)
const generating = ref(false)
const showReportDialog = ref(false)
const currentPage = ref(1)
const pageSize = ref(20)
const totalReports = ref(0)

const recentReports = ref([])

const reportForm = reactive({
  type: '',
  dateRange: [],
  format: 'pdf',
  emailResults: false,
  email: ''
})

// Mock data
const quickReports = ref([
  {
    id: 'sales-pipeline',
    title: 'Sales Pipeline Report',
    description: 'Current opportunities by stage',
    category: 'Sales',
    estimatedTime: '2 mins'
  },
  {
    id: 'activity-summary',
    title: 'Activity Summary',
    description: 'Tasks and meetings overview',
    category: 'Activities',
    estimatedTime: '1 min'
  },
  {
    id: 'contact-analysis',
    title: 'Contact Analysis',
    description: 'Contact engagement metrics',
    category: 'Companies',
    estimatedTime: '3 mins'
  },
  {
    id: 'revenue-forecast',
    title: 'Revenue Forecast',
    description: 'Projected revenue analysis',
    category: 'Sales',
    estimatedTime: '5 mins'
  },
  {
    id: 'team-performance',
    title: 'Team Performance',
    description: 'User activity and performance',
    category: 'Activities',
    estimatedTime: '4 mins'
  },
  {
    id: 'lead-conversion',
    title: 'Lead Conversion',
    description: 'Lead to opportunity conversion rates',
    category: 'Sales',
    estimatedTime: '3 mins'
  }
])

const salesReports = computed(() => quickReports.value.filter(r => r.category === 'Sales'))
const activityReports = computed(() => quickReports.value.filter(r => r.category === 'Activities'))
const companyReports = computed(() => quickReports.value.filter(r => r.category === 'Companies'))
const customReports = computed(() => quickReports.value.filter(r => r.category === 'Custom'))

const paginationStart = computed(() => {
  return Math.min((currentPage.value - 1) * pageSize.value + 1, totalReports.value)
})

const paginationEnd = computed(() => {
  return Math.min(currentPage.value * pageSize.value, totalReports.value)
})

// Methods
const loadReports = async () => {
  try {
    loading.value = true
    
    // API call would go here
    // const response = await reportsApi.getReports()
    
    recentReports.value = [
      {
        id: 1,
        name: 'Monthly Sales Report',
        description: 'Sales performance for March 2024',
        category: 'Sales',
        status: 'completed',
        createdBy: { name: 'Sarah Johnson' },
        createdAt: '2024-03-20T10:00:00Z'
      },
      {
        id: 2,
        name: 'Activity Summary',
        description: 'Weekly activity overview',
        category: 'Activities',
        status: 'completed',
        createdBy: { name: 'Mike Chen' },
        createdAt: '2024-03-19T15:30:00Z'
      },
      {
        id: 3,
        name: 'Pipeline Forecast',
        description: 'Q2 pipeline analysis',
        category: 'Sales',
        status: 'processing',
        createdBy: { name: 'Sarah Johnson' },
        createdAt: '2024-03-18T09:15:00Z'
      }
    ]
    
    totalReports.value = recentReports.value.length
    
  } catch (error) {
    console.error('Error loading reports:', error)
    ElMessage.error('Failed to load reports')
  } finally {
    loading.value = false
  }
}

const createCustomReport = () => {
  showReportDialog.value = true
}

const generateReport = (report) => {
  reportForm.type = report.id
  showReportDialog.value = true
}

const submitReportGeneration = async () => {
  try {
    generating.value = true
    
    if (!reportForm.type) {
      ElMessage.error('Please select a report type')
      return
    }
    
    if (!reportForm.dateRange || reportForm.dateRange.length !== 2) {
      ElMessage.error('Please select a date range')
      return
    }
    
    // API call would go here
    // await reportsApi.generateReport(reportForm)
    
    ElMessage.success('Report generation started. You will be notified when it\'s ready.')
    showReportDialog.value = false
    
    // Reset form
    Object.assign(reportForm, {
      type: '',
      dateRange: [],
      format: 'pdf',
      emailResults: false,
      email: ''
    })
    
    // Reload reports
    setTimeout(() => {
      loadReports()
    }, 1000)
    
  } catch (error) {
    console.error('Error generating report:', error)
    ElMessage.error('Failed to generate report')
  } finally {
    generating.value = false
  }
}

const viewReport = (report) => {
  // Navigate to report detail view (would be created as needed)
  ElMessage.info('Report view functionality will be implemented')
}

const downloadReport = (reportId) => {
  // Download report file
  ElMessage.success('Report download started')
}

const duplicateReport = (reportId) => {
  ElMessage.info('Report duplication functionality will be implemented')
}

const scheduleReport = (reportId) => {
  ElMessage.info('Report scheduling functionality will be implemented')
}

const shareReport = (reportId) => {
  ElMessage.info('Report sharing functionality will be implemented')
}

const deleteReport = async (reportId) => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this report?',
      'Delete Report',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning'
      }
    )
    
    const index = recentReports.value.findIndex(r => r.id === reportId)
    if (index !== -1) {
      recentReports.value.splice(index, 1)
      totalReports.value--
      ElMessage.success('Report deleted successfully')
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting report:', error)
      ElMessage.error('Failed to delete report')
    }
  }
}

const exportReport = (format) => {
  ElMessage.success(`Exporting report as ${format.toUpperCase()}`)
}

const refreshReports = () => {
  loadReports()
}

const viewAllReports = () => {
  // Filter or navigate to show all reports
  ElMessage.info('View all reports functionality will be implemented')
}

const handleSizeChange = (newSize) => {
  pageSize.value = newSize
  currentPage.value = 1
  loadReports()
}

const handlePageChange = (newPage) => {
  currentPage.value = newPage
  loadReports()
}

// Helper functions
const getCategoryIcon = (category) => {
  const icons = {
    Sales: TrendCharts,
    Activities: User,
    Companies: OfficeBuilding,
    Custom: DataAnalysis
  }
  return icons[category] || Document
}

const getCategoryIconClass = (category) => {
  const classes = {
    Sales: 'text-blue-600',
    Activities: 'text-green-600',
    Companies: 'text-purple-600',
    Custom: 'text-orange-600'
  }
  return classes[category] || 'text-gray-600'
}

const getCategoryTagType = (category) => {
  const types = {
    Sales: 'primary',
    Activities: 'success',
    Companies: 'warning',
    Custom: 'danger'
  }
  return types[category] || 'info'
}

const getStatusTagType = (status) => {
  const types = {
    completed: 'success',
    processing: 'warning',
    failed: 'danger'
  }
  return types[status] || 'info'
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

const formatTime = (date) => {
  return format(new Date(date), 'HH:mm')
}

// Lifecycle
onMounted(() => {
  loadReports()
})
</script>

<style scoped>
.reports {
  min-height: calc(100vh - 200px);
}

.report-category-card {
  transition: transform 0.2s ease-in-out;
  cursor: pointer;
}

.report-category-card:hover {
  transform: translateY(-2px);
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .reports :deep(.el-table) {
    font-size: 0.875rem;
  }
  
  .reports :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>