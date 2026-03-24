<template>
  <div class="opportunities-list">
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Opportunities</h1>
        <p class="text-gray-600 mt-1">Track your sales pipeline and deals</p>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Pipeline View Toggle -->
        <el-button-group>
          <el-button 
            :type="viewMode === 'list' ? 'primary' : 'default'"
            @click="viewMode = 'list'"
          >
            <el-icon><List /></el-icon>
            List
          </el-button>
          <el-button 
            :type="viewMode === 'kanban' ? 'primary' : 'default'"
            @click="viewMode = 'kanban'"
          >
            <el-icon><Grid /></el-icon>
            Kanban
          </el-button>
        </el-button-group>
        
        <!-- Actions -->
        <el-dropdown>
          <el-button>
            <el-icon class="mr-1"><Operation /></el-icon>
            Actions
            <el-icon class="ml-1"><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="exportOpportunities">
                <el-icon><Download /></el-icon>
                Export to CSV
              </el-dropdown-item>
              <el-dropdown-item @click="bulkUpdate" :disabled="selectedOpportunities.length === 0">
                <el-icon><Edit /></el-icon>
                Bulk Update
              </el-dropdown-item>
              <el-dropdown-item divided @click="bulkAction('delete')" :disabled="selectedOpportunities.length === 0">
                <el-icon><Delete /></el-icon>
                Delete Selected
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        
        <!-- Add Opportunity Button -->
        <el-button type="primary" @click="createOpportunity">
          <el-icon class="mr-1"><Plus /></el-icon>
          Add Opportunity
        </el-button>
      </div>
    </div>

    <!-- Filters and Summary -->
    <el-row :gutter="20" class="mb-6">
      <!-- Summary Cards -->
      <el-col :xs="24" :lg="8">
        <el-card shadow="never" class="summary-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600">Total Pipeline</p>
              <p class="text-2xl font-bold text-blue-600">${{ formatCurrency(summary.totalValue) }}</p>
            </div>
            <el-icon class="text-3xl text-blue-600">
              <TrendCharts />
            </el-icon>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :lg="8">
        <el-card shadow="never" class="summary-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600">Weighted Pipeline</p>
              <p class="text-2xl font-bold text-green-600">${{ formatCurrency(summary.weightedValue) }}</p>
            </div>
            <el-icon class="text-3xl text-green-600">
              <Money />
            </el-icon>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :lg="8">
        <el-card shadow="never" class="summary-card">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600">Win Rate</p>
              <p class="text-2xl font-bold text-purple-600">{{ summary.winRate }}%</p>
            </div>
            <el-icon class="text-3xl text-purple-600">
              <Star />
            </el-icon>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Search and Filters -->
    <el-card class="mb-6" shadow="never">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0 lg:space-x-4">
        <!-- Search -->
        <div class="flex-1 max-w-md">
          <el-input
            v-model="searchQuery"
            placeholder="Search opportunities..."
            clearable
            @input="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>

        <!-- Filters -->
        <div class="flex items-center space-x-3">
          <!-- Stage Filter -->
          <el-select
            v-model="filters.stage"
            placeholder="Stage"
            clearable
            class="w-36"
            @change="applyFilters"
          >
            <el-option 
              v-for="stage in stages" 
              :key="stage" 
              :label="stage" 
              :value="stage" 
            />
          </el-select>

          <!-- Assigned To Filter -->
          <el-select
            v-model="filters.assignedToId"
            placeholder="Assigned To"
            filterable
            remote
            clearable
            class="w-40"
            :remote-method="searchUsers"
            :loading="usersLoading"
            @change="applyFilters"
          >
            <el-option
              v-for="user in users"
              :key="user.id"
              :label="user.name"
              :value="user.id"
            />
          </el-select>

          <!-- Expected Close Date Filter -->
          <el-date-picker
            v-model="filters.expectedCloseDateRange"
            type="monthrange"
            range-separator="To"
            start-placeholder="Start month"
            end-placeholder="End month"
            class="w-64"
            @change="applyFilters"
          />

          <!-- Clear Filters -->
          <el-button v-if="hasActiveFilters" text @click="clearFilters">
            <el-icon><Refresh /></el-icon>
            Clear
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- Content Area -->
    <div v-if="viewMode === 'list'">
      <!-- List View -->
      <el-card shadow="never">
        <el-table
          v-loading="loading"
          :data="opportunities"
          style="width: 100%"
          @selection-change="handleSelectionChange"
          @sort-change="handleSort"
          row-key="id"
        >
          <!-- Selection Column -->
          <el-table-column type="selection" width="55" />

          <!-- Opportunity Title -->
          <el-table-column label="Opportunity" min-width="250" sortable="custom" prop="title">
            <template #default="{ row }">
              <div class="flex items-center">
                <div class="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center mr-3">
                  <el-icon class="text-gray-600">
                    <TrendCharts />
                  </el-icon>
                </div>
                <div>
                  <div class="font-medium text-gray-900">{{ row.title }}</div>
                  <div class="text-sm text-gray-500">{{ row.company?.name }}</div>
                </div>
              </div>
            </template>
          </el-table-column>

          <!-- Contact -->
          <el-table-column label="Contact" min-width="180" prop="contact.fullName">
            <template #default="{ row }">
              <div v-if="row.contact">
                <el-link @click="viewContact(row.contact.id)" type="primary">
                  {{ row.contact.firstName }} {{ row.contact.lastName }}
                </el-link>
                <div class="text-sm text-gray-500">{{ row.contact.jobTitle }}</div>
              </div>
              <span v-else class="text-gray-400">-</span>
            </template>
          </el-table-column>

          <!-- Stage -->
          <el-table-column label="Stage" prop="stage" width="140" sortable="custom">
            <template #default="{ row }">
              <el-tag :type="getStageTagType(row.stage)" size="small">
                {{ row.stage }}
              </el-tag>
            </template>
          </el-table-column>

          <!-- Value & Probability -->
          <el-table-column label="Value" width="150" sortable="custom" prop="estimatedValue">
            <template #default="{ row }">
              <div>
                <div class="font-medium text-gray-900">${{ formatCurrency(row.estimatedValue) }}</div>
                <div class="text-sm text-gray-500">{{ row.probabilityPercentage }}% probability</div>
              </div>
            </template>
          </el-table-column>

          <!-- Expected Close Date -->
          <el-table-column label="Expected Close" width="130" sortable="custom" prop="expectedCloseDate">
            <template #default="{ row }">
              <div v-if="row.expectedCloseDate">
                {{ formatDate(row.expectedCloseDate) }}
              </div>
              <span v-else class="text-gray-400">-</span>
            </template>
          </el-table-column>

          <!-- Assigned To -->
          <el-table-column label="Assigned To" width="140">
            <template #default="{ row }">
              <div v-if="row.assignedTo" class="flex items-center">
                <el-avatar :size="24" class="mr-2">
                  {{ row.assignedTo.name.charAt(0) }}
                </el-avatar>
                <span class="text-sm">{{ row.assignedTo.name }}</span>
              </div>
              <span v-else class="text-gray-400">-</span>
            </template>
          </el-table-column>

          <!-- Actions -->
          <el-table-column label="Actions" width="120" fixed="right">
            <template #default="{ row }">
              <el-dropdown trigger="click">
                <el-button text>
                  <el-icon><MoreFilled /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="viewOpportunity(row.id)">
                      <el-icon><View /></el-icon>
                      View
                    </el-dropdown-item>
                    <el-dropdown-item @click="editOpportunity(row.id)">
                      <el-icon><Edit /></el-icon>
                      Edit
                    </el-dropdown-item>
                    <el-dropdown-item @click="changeStage(row)">
                      <el-icon><RefreshRight /></el-icon>
                      Change Stage
                    </el-dropdown-item>
                    <el-dropdown-item @click="duplicateOpportunity(row.id)">
                      <el-icon><CopyDocument /></el-icon>
                      Duplicate
                    </el-dropdown-item>
                    <el-dropdown-item divided @click="deleteOpportunity(row)" class="text-red-600">
                      <el-icon><Delete /></el-icon>
                      Delete
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </template>
          </el-table-column>
        </el-table>

        <!-- Pagination -->
        <div class="mt-6 flex justify-between items-center">
          <div class="text-sm text-gray-500">
            Showing {{ (pagination.page - 1) * pagination.pageSize + 1 }} to 
            {{ Math.min(pagination.page * pagination.pageSize, pagination.total) }} of 
            {{ pagination.total }} opportunities
          </div>
          
          <el-pagination
            v-model:current-page="pagination.page"
            v-model:page-size="pagination.pageSize" 
            :page-sizes="[10, 25, 50, 100]"
            :total="pagination.total"
            layout="sizes, prev, pager, next, jumper"
            @size-change="handlePageSizeChange"
            @current-change="handlePageChange"
          />
        </div>
      </el-card>
    </div>

    <div v-else-if="viewMode === 'kanban'">
      <!-- Kanban View -->
      <div class="kanban-board">
        <el-row :gutter="20">
          <el-col 
            v-for="stage in stages" 
            :key="stage" 
            :xs="24" 
            :md="12" 
            :lg="6"
            class="kanban-column"
          >
            <el-card shadow="never" class="stage-column">
              <template #header>
                <div class="flex items-center justify-between">
                  <div>
                    <h3 class="font-medium">{{ stage }}</h3>
                    <p class="text-sm text-gray-500">{{ getStageCount(stage) }} opportunities</p>
                  </div>
                  <div class="text-sm font-medium text-green-600">
                    ${{ formatCurrency(getStageValue(stage)) }}
                  </div>
                </div>
              </template>
              
              <div class="space-y-3 min-h-96">
                <div 
                  v-for="opportunity in getOpportunitiesByStage(stage)" 
                  :key="opportunity.id"
                  class="opportunity-card p-4 border border-gray-200 rounded-lg cursor-pointer hover:shadow-md transition-shadow"
                  @click="viewOpportunity(opportunity.id)"
                >
                  <div class="mb-3">
                    <h4 class="font-medium text-gray-900 mb-1">{{ opportunity.title }}</h4>
                    <p class="text-sm text-gray-600">{{ opportunity.company?.name }}</p>
                  </div>
                  
                  <div class="mb-3">
                    <div class="text-lg font-bold text-green-600">
                      ${{ formatCurrency(opportunity.estimatedValue) }}
                    </div>
                    <div class="text-sm text-gray-500">
                      {{ opportunity.probabilityPercentage }}% probability
                    </div>
                  </div>
                  
                  <div class="flex items-center justify-between">
                    <div v-if="opportunity.assignedTo" class="flex items-center">
                      <el-avatar :size="20" class="mr-1">
                        {{ opportunity.assignedTo.name.charAt(0) }}
                      </el-avatar>
                      <span class="text-xs text-gray-600">{{ opportunity.assignedTo.name }}</span>
                    </div>
                    <div v-if="opportunity.expectedCloseDate" class="text-xs text-gray-500">
                      {{ formatDate(opportunity.expectedCloseDate) }}
                    </div>
                  </div>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>
    </div>

    <!-- Stage Change Dialog -->
    <el-dialog
      v-model="showStageDialog"
      title="Change Opportunity Stage"
      width="30%"
      :close-on-click-modal="false"
    >
      <el-form v-if="selectedOpportunity">
        <el-form-item label="Current Stage">
          <el-tag :type="getStageTagType(selectedOpportunity.stage)">
            {{ selectedOpportunity.stage }}
          </el-tag>
        </el-form-item>
        <el-form-item label="New Stage">
          <el-select v-model="newStage" placeholder="Select new stage" class="w-full">
            <el-option 
              v-for="stage in stages.filter(s => s !== selectedOpportunity.stage)" 
              :key="stage" 
              :label="stage" 
              :value="stage" 
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Notes" v-if="newStage">
          <el-input 
            v-model="stageChangeNotes" 
            type="textarea" 
            :rows="3"
            placeholder="Add notes about this stage change..."
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showStageDialog = false">Cancel</el-button>
          <el-button 
            type="primary" 
            :loading="changingStage"
            @click="updateOpportunityStage"
            :disabled="!newStage"
          >
            Update Stage
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format } from 'date-fns'
import {
  Search,
  Plus,
  Operation,
  ArrowDown,
  Download,
  Edit,
  Delete,
  Refresh,
  MoreFilled,
  View,
  RefreshRight,
  CopyDocument,
  List,
  Grid,
  TrendCharts,
  Money,
  Star
} from '@element-plus/icons-vue'

// Router
const router = useRouter()

// Reactive state
const loading = ref(true)
const changingStage = ref(false)
const usersLoading = ref(false)
const searchQuery = ref('')
const viewMode = ref('list')
const showStageDialog = ref(false)
const selectedOpportunity = ref(null)
const newStage = ref('')
const stageChangeNotes = ref('')

const opportunities = ref([])
const users = ref([])
const selectedOpportunities = ref([])

// Constants
const stages = ['Prospecting', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won', 'Closed Lost']

// Filters
const filters = reactive({
  stage: '',
  assignedToId: null,
  expectedCloseDateRange: null
})

// Pagination
const pagination = reactive({
  page: 1,
  pageSize: 25,
  total: 0
})

// Sorting
const sort = reactive({
  field: 'createdAt',
  order: 'desc'
})

// Summary stats
const summary = reactive({
  totalValue: 0,
  weightedValue: 0,
  winRate: 0
})

// Computed
const hasActiveFilters = computed(() => {
  return filters.stage || filters.assignedToId || filters.expectedCloseDateRange
})

// Methods
const loadOpportunities = async () => {
  try {
    loading.value = true
    
    // Build query parameters
    const params = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      search: searchQuery.value,
      sortField: sort.field,
      sortOrder: sort.order,
      ...filters
    }
    
    // API call would go here
    // const response = await opportunitiesApi.getOpportunities(params)
    
    // Mock data for demonstration
    await new Promise(resolve => setTimeout(resolve, 500))
    
    const mockOpportunities = [
      {
        id: 1,
        title: 'Enterprise Platform Upgrade',
        estimatedValue: 150000,
        probabilityPercentage: 75,
        stage: 'Proposal',
        status: 'Open',
        expectedCloseDate: '2024-04-30T00:00:00Z',
        company: { id: 1, name: 'Acme Corp' },
        contact: { id: 1, firstName: 'John', lastName: 'Smith', jobTitle: 'CTO' },
        assignedTo: { id: 1, name: 'Sarah Johnson' },
        createdAt: '2024-01-15T10:00:00Z'
      },
      {
        id: 2,
        title: 'Digital Transformation Project',
        estimatedValue: 250000,
        probabilityPercentage: 60,
        stage: 'Negotiation',
        status: 'Open',
        expectedCloseDate: '2024-05-15T00:00:00Z',
        company: { id: 2, name: 'TechStart Inc' },
        contact: { id: 2, firstName: 'Jane', lastName: 'Doe', jobTitle: 'VP Engineering' },
        assignedTo: { id: 2, name: 'Mike Wilson' },
        createdAt: '2024-02-01T09:00:00Z'
      }
    ]
    
    opportunities.value = mockOpportunities
    pagination.total = mockOpportunities.length
    
    // Calculate summary
    summary.totalValue = mockOpportunities.reduce((sum, opp) => sum + opp.estimatedValue, 0)
    summary.weightedValue = mockOpportunities.reduce((sum, opp) => 
      sum + (opp.estimatedValue * opp.probabilityPercentage / 100), 0)
    summary.winRate = 68 // This would come from API
    
  } catch (error) {
    console.error('Error loading opportunities:', error)
    ElMessage.error('Failed to load opportunities')
  } finally {
    loading.value = false
  }
}

const searchUsers = async (query) => {
  if (!query) return
  
  try {
    usersLoading.value = true
    
    // Mock users
    users.value = [
      { id: 1, name: 'Sarah Johnson' },
      { id: 2, name: 'Mike Wilson' },
      { id: 3, name: 'Emily Brown' }
    ]
    
  } catch (error) {
    console.error('Error searching users:', error)
  } finally {
    usersLoading.value = false
  }
}

const handleSearch = () => {
  pagination.page = 1
  loadOpportunities()
}

const applyFilters = () => {
  pagination.page = 1
  loadOpportunities()
}

const clearFilters = () => {
  Object.assign(filters, {
    stage: '',
    assignedToId: null,
    expectedCloseDateRange: null
  })
  searchQuery.value = ''
  pagination.page = 1
  loadOpportunities()
}

const handleSort = ({ prop, order }) => {
  sort.field = prop
  sort.order = order === 'ascending' ? 'asc' : 'desc'
  loadOpportunities()
}

const handlePageChange = () => {
  loadOpportunities()
}

const handlePageSizeChange = () => {
  pagination.page = 1
  loadOpportunities()
}

const handleSelectionChange = (selection) => {
  selectedOpportunities.value = selection
}

const createOpportunity = () => {
  router.push('/opportunities/new')
}

const viewOpportunity = (opportunityId) => {
  router.push(`/opportunities/${opportunityId}`)
}

const editOpportunity = (opportunityId) => {
  router.push(`/opportunities/${opportunityId}/edit`)
}

const viewContact = (contactId) => {
  router.push(`/contacts/${contactId}`)
}

const changeStage = (opportunity) => {
  selectedOpportunity.value = opportunity
  newStage.value = ''
  stageChangeNotes.value = ''
  showStageDialog.value = true
}

const updateOpportunityStage = async () => {
  try {
    changingStage.value = true
    
    // API call would go here
    // await opportunitiesApi.updateStage(selectedOpportunity.value.id, {
    //   stage: newStage.value,
    //   notes: stageChangeNotes.value
    // })
    
    // Update local data
    selectedOpportunity.value.stage = newStage.value
    
    ElMessage.success('Opportunity stage updated successfully')
    showStageDialog.value = false
    loadOpportunities()
    
  } catch (error) {
    console.error('Error updating stage:', error)
    ElMessage.error('Failed to update stage')
  } finally {
    changingStage.value = false
  }
}

const duplicateOpportunity = (opportunityId) => {
  router.push(`/opportunities/new?duplicate=${opportunityId}`)
}

const deleteOpportunity = async (opportunity) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete "${opportunity.title}"?`,
      'Delete Opportunity',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    // API call would go here
    // await opportunitiesApi.deleteOpportunity(opportunity.id)
    
    ElMessage.success('Opportunity deleted successfully')
    loadOpportunities()
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting opportunity:', error)
      ElMessage.error('Failed to delete opportunity')
    }
  }
}

const bulkAction = async (action) => {
  if (selectedOpportunities.value.length === 0) return
  
  try {
    if (action === 'delete') {
      await ElMessageBox.confirm(
        `Are you sure you want to delete ${selectedOpportunities.value.length} opportunities?`,
        'Delete Opportunities',
        {
          confirmButtonText: 'Delete',
          cancelButtonText: 'Cancel', 
          type: 'warning',
          confirmButtonClass: 'el-button--danger'
        }
      )
      
      ElMessage.success(`${selectedOpportunities.value.length} opportunities deleted`)
      selectedOpportunities.value = []
      loadOpportunities()
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error performing bulk action:', error)
      ElMessage.error('Failed to perform action')
    }
  }
}

const bulkUpdate = () => {
  ElMessage.info('Bulk update functionality will be implemented')
}

const exportOpportunities = () => {
  ElMessage.info('Export functionality will be implemented')
}

// Kanban methods
const getOpportunitiesByStage = (stage) => {
  return opportunities.value.filter(opp => opp.stage === stage)
}

const getStageCount = (stage) => {
  return getOpportunitiesByStage(stage).length
}

const getStageValue = (stage) => {
  return getOpportunitiesByStage(stage).reduce((sum, opp) => sum + opp.estimatedValue, 0)
}

// Helper functions
const getStageTagType = (stage) => {
  const types = {
    'Prospecting': 'info',
    'Qualification': 'primary',
    'Proposal': 'warning',
    'Negotiation': 'danger',
    'Closed Won': 'success',
    'Closed Lost': 'info'
  }
  return types[stage] || 'info'
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-US', {
    notation: 'compact',
    compactDisplay: 'short'
  }).format(amount)
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

// Lifecycle 
onMounted(() => {
  loadOpportunities()
})

// Watch for route changes 
watch(() => router.currentRoute.value.query, () => {
  // Handle query parameter changes if needed
}, { immediate: true })
</script>

<style scoped>
.opportunities-list {
  min-height: calc(100vh - 200px);
}

.summary-card {
  transition: transform 0.2s ease;
}

.summary-card:hover {
  transform: translateY(-2px);
}

.kanban-board {
  min-height: 600px;
}

.kanban-column {
  margin-bottom: 20px;
}

.stage-column {
  height: 100%;
}

.stage-column :deep(.el-card__body) {
  height: calc(100% - 60px);
  overflow-y: auto;
}

.opportunity-card {
  transition: all 0.2s ease;
}

.opportunity-card:hover {
  transform: translateY(-1px);
}

:deep(.el-table) {
  border-radius: 8px;
  overflow: hidden;
}

:deep(.el-table__header-wrapper) {
  background-color: #f8fafc;
}

:deep(.el-table__row:hover) {
  background-color: #f9fafb;
}

:deep(.el-pagination) {
  justify-content: center;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .kanban-column {
    margin-bottom: 16px;
  }
  
  .opportunities-list :deep(.el-card__body) {
    padding: 16px;
  }
  
  :deep(.el-table__body-wrapper) {
    overflow-x: auto;
  }
}
</style>