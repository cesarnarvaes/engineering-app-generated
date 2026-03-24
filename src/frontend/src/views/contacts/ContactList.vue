<template>
  <div class="contacts-list">
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Contacts</h1>
        <p class="text-gray-600 mt-1">Manage your contact relationships</p>
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
              <el-dropdown-item @click="importContacts">
                <el-icon><Upload /></el-icon>
                Import Contacts
              </el-dropdown-item>
              <el-dropdown-item @click="exportContacts">
                <el-icon><Download /></el-icon>
                Export Contacts
              </el-dropdown-item>
              <el-dropdown-item divided @click="bulkAction('delete')" :disabled="selectedContacts.length === 0">
                <el-icon><Delete /></el-icon>
                Delete Selected
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        
        <!-- Add Contact Button -->
        <el-button type="primary" @click="createContact">
          <el-icon class="mr-1"><Plus /></el-icon>
          Add Contact
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
            placeholder="Search contacts..."
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
          <!-- Status Filter -->
          <el-select
            v-model="filters.status"
            placeholder="Status"
            clearable
            class="w-32"
            @change="applyFilters"
          >
            <el-option label="Active" value="Active" />
            <el-option label="Inactive" value="Inactive" />
            <el-option label="Lead" value="Lead" />
          </el-select>

          <!-- Company Filter -->
          <el-select
            v-model="filters.companyId"
            placeholder="Company"
            filterable
            remote
            clearable
            class="w-48"
            :remote-method="searchCompanies"
            :loading="companiesLoading"
            @change="applyFilters"
          >
            <el-option
              v-for="company in companies"
              :key="company.id"
              :label="company.name"
              :value="company.id"
            />
          </el-select>

          <!-- Created Date Filter -->
          <el-date-picker
            v-model="filters.dateRange"
            type="datetimerange"
            range-separator="To"
            start-placeholder="Start date"
            end-placeholder="End date"
            class="w-80"
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

    <!-- Contacts Table -->
    <el-card shadow="never">
      <el-table
        v-loading="loading"
        :data="contacts"
        style="width: 100%"
        @selection-change="handleSelectionChange"
        @sort-change="handleSort"
        row-key="id"
      >
        <!-- Selection Column -->
        <el-table-column type="selection" width="55" />

        <!-- Avatar and Name -->
        <el-table-column label="Contact" min-width="200" sortable="custom" prop="fullName">
          <template #default="{ row }">
            <div class="flex items-center">
              <el-avatar
                :size="40"
                :src="row.avatar"
                class="mr-3"
              >
                {{ row.firstName.charAt(0) + row.lastName.charAt(0) }}
              </el-avatar>
              <div>
                <div class="font-medium text-gray-900">
                  {{ row.firstName }} {{ row.lastName }}
                </div>
                <div class="text-sm text-gray-500">{{ row.jobTitle }}</div>
              </div>
            </div>
          </template>
        </el-table-column>

        <!-- Email -->
        <el-table-column label="Email" prop="email" min-width="200" sortable="custom">
          <template #default="{ row }">
            <div v-if="row.email">
              <el-link :href="`mailto:${row.email}`" target="_blank" type="primary">
                {{ row.email }}
              </el-link>
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Phone -->
        <el-table-column label="Phone" prop="phoneNumber" width="150">
          <template #default="{ row }">
            <div v-if="row.phoneNumber">
              <el-link :href="`tel:${row.phoneNumber}`" target="_blank" type="primary">
                {{ row.phoneNumber }}
              </el-link>
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Company -->
        <el-table-column label="Company" prop="company.name" min-width="180" sortable="custom">
          <template #default="{ row }">
            <div v-if="row.company">
              <el-link @click="viewCompany(row.company.id)" type="primary">
                {{ row.company.name }}
              </el-link>
            </div>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>

        <!-- Status -->
        <el-table-column label="Status" prop="status" width="120" sortable="custom">
          <template #default="{ row }">
            <el-tag 
              :type="getStatusType(row.status)"
              size="small"
            >
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>

        <!-- Created Date -->
        <el-table-column label="Created" prop="createdAt" width="120" sortable="custom">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
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
                  <el-dropdown-item @click="viewContact(row.id)">
                    <el-icon><View /></el-icon>
                    View
                  </el-dropdown-item>
                  <el-dropdown-item @click="editContact(row.id)">
                    <el-icon><Edit /></el-icon>
                    Edit
                  </el-dropdown-item>
                  <el-dropdown-item @click="sendEmail(row.email)" :disabled="!row.email">
                    <el-icon><Message /></el-icon>
                    Send Email
                  </el-dropdown-item>
                  <el-dropdown-item @click="makeCall(row.phoneNumber)" :disabled="!row.phoneNumber">
                    <el-icon><Phone /></el-icon>
                    Call
                  </el-dropdown-item>
                  <el-dropdown-item divided @click="deleteContact(row)" class="text-red-600">
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
          {{ pagination.total }} contacts
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
      title="Import Contacts"
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
import { format } from 'date-fns'
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
  Message,
  Phone
} from '@element-plus/icons-vue'

// Router
const router = useRouter()

// Reactive state
const loading = ref(true)
const importing = ref(false)
const companiesLoading = ref(false)
const searchQuery = ref('')
const showImportDialog = ref(false)
const uploadRef = ref()

const contacts = ref([])
const companies = ref([])
const selectedContacts = ref([])

// Filters
const filters = reactive({
  status: '',
  companyId: null,
  dateRange: null
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

// Computed
const hasActiveFilters = computed(() => {
  return filters.status || filters.companyId || filters.dateRange
})

// Methods
const loadContacts = async () => {
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
    // const response = await contactsApi.getContacts(params)
    
    // Mock data for demonstration
    await new Promise(resolve => setTimeout(resolve, 500))
    
    const mockContacts = [
      {
        id: 1,
        firstName: 'John',
        lastName: 'Smith', 
        email: 'john.smith@example.com',
        phoneNumber: '+1 (555) 123-4567',
        jobTitle: 'Software Engineer',
        status: 'Active',
        company: { id: 1, name: 'Acme Corp' },
        createdAt: '2024-01-15T10:00:00Z'
      },
      {
        id: 2,
        firstName: 'Jane',
        lastName: 'Doe',
        email: 'jane.doe@example.com',
        phoneNumber: '+1 (555) 987-6543',
        jobTitle: 'Product Manager',
        status: 'Lead',
        company: { id: 2, name: 'TechStart Inc' },
        createdAt: '2024-01-14T09:30:00Z'
      }
    ]
    
    contacts.value = mockContacts
    pagination.total = mockContacts.length
    
  } catch (error) {
    console.error('Error loading contacts:', error)
    ElMessage.error('Failed to load contacts')
  } finally {
    loading.value = false
  }
}

const searchCompanies = async (query) => {
  if (!query) return
  
  try {
    companiesLoading.value = true
    
    // API call would go here
    // const response = await companiesApi.searchCompanies(query)
    
    // Mock data
    companies.value = [
      { id: 1, name: 'Acme Corp' },
      { id: 2, name: 'TechStart Inc' }
    ]
    
  } catch (error) {
    console.error('Error searching companies:', error)
  } finally {
    companiesLoading.value = false
  }
}

const handleSearch = () => {
  pagination.page = 1
  loadContacts()
}

const applyFilters = () => {
  pagination.page = 1
  loadContacts()
}

const clearFilters = () => {
  Object.assign(filters, {
    status: '',
    companyId: null,
    dateRange: null
  })
  searchQuery.value = ''
  pagination.page = 1
  loadContacts()
}

const handleSort = ({ prop, order }) => {
  sort.field = prop
  sort.order = order === 'ascending' ? 'asc' : 'desc'
  loadContacts()
}

const handlePageChange = () => {
  loadContacts()
}

const handlePageSizeChange = () => {
  pagination.page = 1
  loadContacts()
}

const handleSelectionChange = (selection) => {
  selectedContacts.value = selection
}

const createContact = () => {
  router.push('/contacts/new')
}

const viewContact = (contactId) => {
  router.push(`/contacts/${contactId}`)
}

const editContact = (contactId) => {
  router.push(`/contacts/${contactId}/edit`)
}

const viewCompany = (companyId) => {
  router.push(`/companies/${companyId}`)
}

const deleteContact = async (contact) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete ${contact.firstName} ${contact.lastName}?`,
      'Delete Contact',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    // API call would go here
    // await contactsApi.deleteContact(contact.id)
    
    ElMessage.success('Contact deleted successfully')
    loadContacts()
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting contact:', error)
      ElMessage.error('Failed to delete contact')
    }
  }
}

const bulkAction = async (action) => {
  if (selectedContacts.value.length === 0) return
  
  try {
    if (action === 'delete') {
      await ElMessageBox.confirm(
        `Are you sure you want to delete ${selectedContacts.value.length} contacts?`,
        'Delete Contacts',
        {
          confirmButtonText: 'Delete',
          cancelButtonText: 'Cancel', 
          type: 'warning',
          confirmButtonClass: 'el-button--danger'
        }
      )
      
      // API call would go here
      // await contactsApi.deleteContacts(selectedContacts.value.map(c => c.id))
      
      ElMessage.success(`${selectedContacts.value.length} contacts deleted`)
      selectedContacts.value = []
      loadContacts()
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error performing bulk action:', error)
      ElMessage.error('Failed to perform action')
    }
  }
}

const sendEmail = (email) => {
  window.open(`mailto:${email}`)
}

const makeCall = (phoneNumber) => {
  window.open(`tel:${phoneNumber}`)
}

const importContacts = () => {
  showImportDialog.value = true
}

const exportContacts = () => {
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
    
    ElMessage.success('Contacts imported successfully')
    showImportDialog.value = false
    loadContacts()
    
  } catch (error) {
    console.error('Import error:', error)
    ElMessage.error('Failed to import contacts')
  } finally {
    importing.value = false
  }
}

const downloadTemplate = () => {
  // Download CSV template
  ElMessage.info('Template download will be implemented')
}

const getStatusType = (status) => {
  const types = {
    'Active': 'success',
    'Inactive': 'info',
    'Lead': 'warning'
  }
  return types[status] || 'info'
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

// Lifecycle 
onMounted(() => {
  loadContacts()
})

// Watch for route changes 
watch(() => router.currentRoute.value.query, () => {
  // Handle query parameter changes if needed
}, { immediate: true })
</script>

<style scoped>
.contacts-list {
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

/* Mobile responsiveness */
@media (max-width: 768px) {
  :deep(.el-table__body-wrapper) {
    overflow-x: auto;
  }
  
  .contacts-list :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>