<template>
  <div class="companies-list">
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Companies</h1>
        <p class="text-gray-600 mt-1">Manage your business relationships</p>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Import/Export -->
        <el-dropdown>
          <el-button>
            <el-icon class="mr-1"><Operation /></el-icon>
            Actions
            <el-icon class="ml-1"><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="importCompanies">
                <el-icon><Upload /></el-icon>
                Import Companies
              </el-dropdown-item>
              <el-dropdown-item @click="exportCompanies">
                <el-icon><Download /></el-icon>
                Export Companies
              </el-dropdown-item>
              <el-dropdown-item divided @click="bulkAction('delete')" :disabled="selectedCompanies.length === 0">
                <el-icon><Delete /></el-icon>
                Delete Selected
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        
        <!-- Add Company Button -->
        <el-button type="primary" @click="createCompany">
          <el-icon class="mr-1"><Plus /></el-icon>
          Add Company
        </el-button>
      </div>
    </div>

    <!-- Search and Filters -->
    <el-card class="mb-6" shadow="never">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0 lg:space-x-4">
        <!-- Search -->
        <div class="flex-1 max-w-md">
          <el-input
            v-model="searchQuery"
            placeholder="Search companies..."
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
          <!-- Industry Filter -->
          <el-select
            v-model="filters.industry"
            placeholder="Industry"
            clearable
            class="w-40"
            @change="applyFilters"
          >
            <el-option label="Technology" value="Technology" />
            <el-option label="Healthcare" value="Healthcare" />
            <el-option label="Finance" value="Finance" />
            <el-option label="Manufacturing" value="Manufacturing" />
            <el-option label="Retail" value="Retail" />
            <el-option label="Education" value="Education" />
          </el-select>

          <!-- Company Size Filter -->
          <el-select
            v-model="filters.companySize"
            placeholder="Size"
            clearable
            class="w-32"
            @change="applyFilters"
          >
            <el-option label="1-10" value="1-10" />
            <el-option label="11-50" value="11-50" />
            <el-option label="51-200" value="51-200" />
            <el-option label="201-500" value="201-500" />
            <el-option label="500+" value="500+" />
          </el-select>

          <!-- Revenue Filter -->
          <el-select
            v-model="filters.revenueRange"
            placeholder="Revenue"
            clearable
            class="w-36"
            @change="applyFilters"
          >
            <el-option label="< $1M" value="0-1000000" />
            <el-option label="$1M - $10M" value="1000000-10000000" />
            <el-option label="$10M - $100M" value="10000000-100000000" />
            <el-option label="$100M+" value="100000000+" />
          </el-select>

          <!-- Clear Filters -->
          <el-button v-if="hasActiveFilters" text @click="clearFilters">
            <el-icon><Refresh /></el-icon>
            Clear
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- Companies Table -->
    <el-card shadow="never">
      <el-table
        v-loading="loading"
        :data="companies"
        style="width: 100%"
        @selection-change="handleSelectionChange"
        @sort-change="handleSort"
        row-key="id"
      >
        <!-- Selection Column -->
        <el-table-column type="selection" width="55" />

        <!-- Logo and Name -->
        <el-table-column label="Company" min-width="250" sortable="custom" prop="name">
          <template #default="{ row }">
            <div class="flex items-center">
              <div class="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center mr-3">
                <img 
                  v-if="row.logo" 
                  :src="row.logo" 
                  :alt="row.name"
                  class="w-8 h-8 object-contain rounded"
                />
                <el-icon v-else class="text-gray-400">
                  <OfficeBuilding />
                </el-icon>
              </div>
              <div>
                <div class="font-medium text-gray-900">{{ row.name }}</div>
                <div class="text-sm text-gray-500">{{ row.industry }}</div>
              </div>
            </div>
          </template>
        </el-table-column>

        <!-- Website -->
        <el-table-column label="Website" prop="website" min-width="200">
          <template #default="{ row }">
            <div v-if="row.website">
              <el-link :href="row.website" target="_blank" type="primary">
                {{ formatWebsite(row.website) }}
              </el-link>
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Contact Info -->
        <el-table-column label="Contact" min-width="200">
          <template #default="{ row }">
            <div class="space-y-1">
              <div v-if="row.email" class="text-sm">
                <el-link :href="`mailto:${row.email}`" target="_blank" type="primary">
                  {{ row.email }}
                </el-link>
              </div>
              <div v-if="row.phoneNumber" class="text-sm">
                <el-link :href="`tel:${row.phoneNumber}`" target="_blank" type="primary">
                  {{ row.phoneNumber }}
                </el-link>
              </div>
              <div v-if="!row.email && !row.phoneNumber" class="text-gray-400">-</div>
            </div>
          </template>
        </el-table-column>

        <!-- Location -->
        <el-table-column label="Location" min-width="180">
          <template #default="{ row }">
            <div v-if="row.city || row.country">
              {{ [row.city, row.country].filter(Boolean).join(', ') }}
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Company Size -->
        <el-table-column label="Size" prop="companySize" width="100" sortable="custom">
          <template #default="{ row }">
            <el-tag v-if="row.companySize" type="info" size="small">
              {{ row.companySize }}
            </el-tag>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Annual Revenue -->
        <el-table-column label="Revenue" prop="annualRevenue" width="120" sortable="custom">
          <template #default="{ row }">
            <div v-if="row.annualRevenue" class="font-medium text-green-600">
              ${{ formatCurrency(row.annualRevenue) }}
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Contacts Count -->
        <el-table-column label="Contacts" width="100" sortable="custom" prop="contactsCount">
          <template #default="{ row }">
            <el-badge :value="row.contactsCount" type="primary">
              <el-button text size="small" @click="viewContacts(row.id)">
                View
              </el-button>
            </el-badge>
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
                  <el-dropdown-item @click="viewCompany(row.id)">
                    <el-icon><View /></el-icon>
                    View
                  </el-dropdown-item>
                  <el-dropdown-item @click="editCompany(row.id)">
                    <el-icon><Edit /></el-icon>
                    Edit
                  </el-dropdown-item>
                  <el-dropdown-item @click="viewContacts(row.id)">
                    <el-icon><User /></el-icon>
                    View Contacts
                  </el-dropdown-item>
                  <el-dropdown-item @click="visitWebsite(row.website)" :disabled="!row.website">
                    <el-icon><Link /></el-icon>
                    Visit Website
                  </el-dropdown-item>
                  <el-dropdown-item divided @click="deleteCompany(row)" class="text-red-600">
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
          {{ pagination.total }} companies
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

    <!-- Import Dialog -->
    <el-dialog
      v-model="showImportDialog"
      title="Import Companies"
      width="40%"
      :close-on-click-modal="false"
    >
      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            CSV File
          </label>
          <el-upload
            ref="uploadRef"
            :auto-upload="false"
            :show-file-list="true"
            :limit="1"
            accept=".csv"
            @change="handleFileChange"
          >
            <el-button>
              <el-icon><Upload /></el-icon>
              Choose CSV File
            </el-button>
            <template #tip>
              <div class="el-upload__tip">
                CSV files up to 10MB. Download template 
                <el-link type="primary" @click="downloadTemplate">here</el-link>
              </div>
            </template>
          </el-upload>
        </div>
      </div>
      
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showImportDialog = false">Cancel</el-button>
          <el-button type="primary" :loading="importing" @click="processImport">
            Import
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
import {
  Search,
  Plus,
  Operation,
  ArrowDown,
  Upload,
  Download,
  Delete,
  Refresh,
  MoreFilled,
  View,
  Edit,
  User,
  Link,
  OfficeBuilding
} from '@element-plus/icons-vue'

// Router
const router = useRouter()

// Reactive state
const loading = ref(true)
const importing = ref(false)
const searchQuery = ref('')
const showImportDialog = ref(false)
const uploadRef = ref()

const companies = ref([])
const selectedCompanies = ref([])

// Filters
const filters = reactive({
  industry: '',
  companySize: '',
  revenueRange: ''
})

// Pagination
const pagination = reactive({
  page: 1,
  pageSize: 25,
  total: 0
})

// Sorting
const sort = reactive({
  field: 'name',
  order: 'asc'
})

// Computed
const hasActiveFilters = computed(() => {
  return filters.industry || filters.companySize || filters.revenueRange
})

// Methods
const loadCompanies = async () => {
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
    // const response = await companiesApi.getCompanies(params)
    
    // Mock data for demonstration
    await new Promise(resolve => setTimeout(resolve, 500))
    
    const mockCompanies = [
      {
        id: 1,
        name: 'Acme Corporation',
        industry: 'Technology',
        companySize: '51-200',
        website: 'https://acme.com',
        email: 'contact@acme.com',
        phoneNumber: '+1 (555) 123-4567',
        city: 'San Francisco',
        country: 'United States',
        annualRevenue: 5000000,
        contactsCount: 12,
        createdAt: '2024-01-15T10:00:00Z'
      },
      {
        id: 2,
        name: 'TechStart Inc',
        industry: 'Technology',
        companySize: '11-50',
        website: 'https://techstart.io',
        email: 'hello@techstart.io',
        phoneNumber: '+1 (555) 987-6543',
        city: 'Austin',
        country: 'United States',
        annualRevenue: 2500000,
        contactsCount: 8,
        createdAt: '2024-01-14T09:30:00Z'
      }
    ]
    
    companies.value = mockCompanies
    pagination.total = mockCompanies.length
    
  } catch (error) {
    console.error('Error loading companies:', error)
    ElMessage.error('Failed to load companies')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  pagination.page = 1
  loadCompanies()
}

const applyFilters = () => {
  pagination.page = 1
  loadCompanies()
}

const clearFilters = () => {
  Object.assign(filters, {
    industry: '',
    companySize: '',
    revenueRange: ''
  })
  searchQuery.value = ''
  pagination.page = 1
  loadCompanies()
}

const handleSort = ({ prop, order }) => {
  sort.field = prop
  sort.order = order === 'ascending' ? 'asc' : 'desc'
  loadCompanies()
}

const handlePageChange = () => {
  loadCompanies()
}

const handlePageSizeChange = () => {
  pagination.page = 1
  loadCompanies()
}

const handleSelectionChange = (selection) => {
  selectedCompanies.value = selection
}

const createCompany = () => {
  router.push('/companies/new')
}

const viewCompany = (companyId) => {
  router.push(`/companies/${companyId}`)
}

const editCompany = (companyId) => {
  router.push(`/companies/${companyId}/edit`)
}

const viewContacts = (companyId) => {
  router.push(`/contacts?companyId=${companyId}`)
}

const visitWebsite = (website) => {
  if (website) {
    window.open(website, '_blank')
  }
}

const deleteCompany = async (company) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete ${company.name}? This will also affect related contacts and opportunities.`,
      'Delete Company',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    // API call would go here
    // await companiesApi.deleteCompany(company.id)
    
    ElMessage.success('Company deleted successfully')
    loadCompanies()
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting company:', error)
      ElMessage.error('Failed to delete company')
    }
  }
}

const bulkAction = async (action) => {
  if (selectedCompanies.value.length === 0) return
  
  try {
    if (action === 'delete') {
      await ElMessageBox.confirm(
        `Are you sure you want to delete ${selectedCompanies.value.length} companies?`,
        'Delete Companies',
        {
          confirmButtonText: 'Delete',
          cancelButtonText: 'Cancel', 
          type: 'warning',
          confirmButtonClass: 'el-button--danger'
        }
      )
      
      // API call would go here
      // await companiesApi.deleteCompanies(selectedCompanies.value.map(c => c.id))
      
      ElMessage.success(`${selectedCompanies.value.length} companies deleted`)
      selectedCompanies.value = []
      loadCompanies()
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error performing bulk action:', error)
      ElMessage.error('Failed to perform action')
    }
  }
}

const importCompanies = () => {
  showImportDialog.value = true
}

const exportCompanies = () => {
  // Implementation would depend on backend API
  ElMessage.info('Export functionality will be implemented')
}

const handleFileChange = (file) => {
  console.log('File selected:', file)
}

const processImport = async () => {
  try {
    importing.value = true
    
    // Process import logic would go here
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    ElMessage.success('Companies imported successfully')
    showImportDialog.value = false
    loadCompanies()
    
  } catch (error) {
    console.error('Import error:', error)
    ElMessage.error('Failed to import companies')
  } finally {
    importing.value = false
  }
}

const downloadTemplate = () => {
  // Download CSV template
  ElMessage.info('Template download will be implemented')
}

const formatWebsite = (website) => {
  return website.replace(/^https?:\/\//, '')
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-US', {
    notation: 'compact',
    compactDisplay: 'short'
  }).format(amount)
}

// Lifecycle 
onMounted(() => {
  loadCompanies()
})

// Watch for route changes 
watch(() => router.currentRoute.value.query, () => {
  // Handle query parameter changes if needed
}, { immediate: true })
</script>

<style scoped>
.companies-list {
  min-height: calc(100vh - 200px);
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

:deep(.el-upload__tip) {
  color: #6b7280;
  font-size: 12px;
  line-height: 1.4;
  margin-top: 8px;
}

/* Badge positioning */
:deep(.el-badge) {
  display: inline-block;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  :deep(.el-table__body-wrapper) {
    overflow-x: auto;
  }
  
  .companies-list :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>