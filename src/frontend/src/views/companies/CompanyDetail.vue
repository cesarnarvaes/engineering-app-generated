<template>
  <div class="company-detail" v-loading="loading">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <el-button @click="goBack" text class="mr-4">
          <el-icon><ArrowLeft /></el-icon>
          Back
        </el-button>
        <div class="flex items-center">
          <div class="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center mr-4">
            <img 
              v-if="companyData?.logo" 
              :src="companyData.logo" 
              :alt="companyData.name"
              class="w-10 h-10 object-contain rounded"
            />
            <el-icon v-else class="text-gray-400 text-2xl">
              <Office />
            </el-icon>
          </div>
          <div>
            <h1 class="text-2xl font-semibold text-gray-900">
              {{ companyData?.name }}
            </h1>
            <p class="text-gray-600 mt-1">{{ companyData?.industry }}</p>
          </div>
        </div>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Action Buttons -->
        <el-button @click="visitWebsite" :disabled="!companyData?.website">
          <el-icon><Link /></el-icon>
          Website
        </el-button>
        <el-button @click="sendEmail" :disabled="!companyData?.email">
          <el-icon><Message /></el-icon>
          Email
        </el-button>
        
        <!-- Edit/Save Toggle -->
        <el-button 
          v-if="!editMode" 
          type="primary" 
          @click="toggleEditMode"
        >
          <el-icon><Edit /></el-icon>
          Edit
        </el-button>
        
        <div v-else class="flex space-x-2">
          <el-button @click="cancelEdit">
            Cancel
          </el-button>
          <el-button 
            type="primary" 
            :loading="saving"
            @click="saveCompany"
          >
            <el-icon><Check /></el-icon>
            Save
          </el-button>
        </div>
        
        <!-- More Actions -->
        <el-dropdown>
          <el-button>
            <el-icon><MoreFilled /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="duplicateCompany">
                <el-icon><CopyDocument /></el-icon>
                Duplicate
              </el-dropdown-item>
              <el-dropdown-item @click="exportCompany">
                <el-icon><Download /></el-icon>
                Export
              </el-dropdown-item>
              <el-dropdown-item divided @click="deleteCompany" class="text-red-600">
                <el-icon><Delete /></el-icon>
                Delete
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- Main Content -->
    <el-row :gutter="20">
      <!-- Left Column - Company Information -->
      <el-col :xs="24" :lg="16">
        <!-- Company Details Card -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center">
              <el-icon class="mr-2 text-blue-600">
                <OfficeBuilding />
              </el-icon>
              <span class="text-lg font-semibold">Company Information</span>
            </div>
          </template>
          
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <!-- Basic Information -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Company Name</label>
              <el-input 
                v-if="editMode"
                v-model="companyForm.name" 
                placeholder="Enter company name"
              />
              <p v-else class="text-gray-900">{{ companyData?.name || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Industry</label>
              <el-select 
                v-if="editMode"
                v-model="companyForm.industry" 
                placeholder="Select industry"
                class="w-full"
                filterable
                allow-create
              >
                <el-option label="Technology" value="Technology" />
                <el-option label="Healthcare" value="Healthcare" />
                <el-option label="Finance" value="Finance" />
                <el-option label="Manufacturing" value="Manufacturing" />
                <el-option label="Retail" value="Retail" />
                <el-option label="Education" value="Education" />
                <el-option label="Real Estate" value="Real Estate" />
                <el-option label="Consulting" value="Consulting" />
                <el-option label="Other" value="Other" />
              </el-select>
              <p v-else class="text-gray-900">{{ companyData?.industry || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Company Size</label>
              <el-select 
                v-if="editMode"
                v-model="companyForm.companySize" 
                placeholder="Select size"
                class="w-full"
              >
                <el-option label="1-10 employees" value="1-10" />
                <el-option label="11-50 employees" value="11-50" />
                <el-option label="51-200 employees" value="51-200" />
                <el-option label="201-500 employees" value="201-500" />
                <el-option label="501-1000 employees" value="501-1000" />
                <el-option label="1000+ employees" value="1000+" />
              </el-select>
              <el-tag v-else-if="companyData?.companySize" type="info" size="small">
                {{ companyData.companySize }} employees
              </el-tag>
              <span v-else class="text-gray-400">-</span>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Annual Revenue</label>
              <el-input 
                v-if="editMode"
                v-model="companyForm.annualRevenue" 
                placeholder="Enter annual revenue"
                type="number"
              >
                <template #prepend>$</template>
              </el-input>
              <div v-else-if="companyData?.annualRevenue" class="font-medium text-green-600">
                ${{ formatCurrency(companyData.annualRevenue) }}
              </div>
              <span v-else class="text-gray-400">-</span>
            </div>

            <!-- Contact Information -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Website</label>
              <el-input 
                v-if="editMode"
                v-model="companyForm.website" 
                placeholder="https://example.com"
              />
              <div v-else>
                <el-link v-if="companyData?.website" :href="companyData.website" target="_blank" type="primary">
                  {{ formatWebsite(companyData.website) }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <el-input 
                v-if="editMode"
                v-model="companyForm.email" 
                placeholder="contact@company.com"
                type="email"
              />
              <div v-else>
                <el-link v-if="companyData?.email" :href="`mailto:${companyData.email}`" target="_blank" type="primary">
                  {{ companyData.email }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Phone</label>
              <el-input 
                v-if="editMode"
                v-model="companyForm.phoneNumber" 
                placeholder="+1 (555) 123-4567"
              />
              <div v-else>
                <el-link v-if="companyData?.phoneNumber" :href="`tel:${companyData.phoneNumber}`" target="_blank" type="primary">
                  {{ companyData.phoneNumber }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <!-- Address Information -->
            <div class="lg:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Address</label>
              <div v-if="editMode" class="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <el-input v-model="companyForm.addressLine1" placeholder="Address Line 1" />
                <el-input v-model="companyForm.addressLine2" placeholder="Address Line 2" />
                <el-input v-model="companyForm.city" placeholder="City" />
                <el-input v-model="companyForm.stateProvince" placeholder="State/Province" />
                <el-input v-model="companyForm.postalCode" placeholder="Postal Code" />
                <el-input v-model="companyForm.country" placeholder="Country" />
              </div>
              <div v-else class="text-gray-900">
                <div v-if="hasAddress">
                  <div>{{ companyData?.addressLine1 }}</div>
                  <div v-if="companyData?.addressLine2">{{ companyData.addressLine2 }}</div>
                  <div>
                    {{ [companyData?.city, companyData?.stateProvince, companyData?.postalCode].filter(Boolean).join(', ') }}
                  </div>
                  <div v-if="companyData?.country">{{ companyData.country }}</div>
                </div>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>
          </div>
        </el-card>

        <!-- Contacts -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-green-600">
                  <User />
                </el-icon>
                <span class="text-lg font-semibold">Contacts ({{ contacts.length }})</span>
              </div>
              <div class="flex space-x-2">
                <el-button @click="viewAllContacts" size="small">
                  View All
                </el-button>
                <el-button @click="addContact" size="small" type="primary">
                  <el-icon><Plus /></el-icon>
                  Add Contact
                </el-button>
              </div>
            </div>
          </template>
          
          <div class="space-y-4">
            <div 
              v-for="contact in contacts.slice(0, 5)" 
              :key="contact.id"
              class="flex items-center justify-between p-4 border border-gray-100 rounded-lg hover:bg-gray-50 cursor-pointer"
              @click="viewContact(contact.id)"
            >
              <div class="flex items-center">
                <el-avatar :size="40" class="mr-3">
                  {{ contact.firstName.charAt(0) + contact.lastName.charAt(0) }}
                </el-avatar>
                <div>
                  <h4 class="text-sm font-medium text-gray-900">
                    {{ contact.firstName }} {{ contact.lastName }}
                  </h4>
                  <p class="text-sm text-gray-600">{{ contact.jobTitle }}</p>
                </div>
              </div>
              <div class="flex items-center space-x-2">
                <el-button v-if="contact.email" @click.stop="sendContactEmail(contact.email)" text size="small">
                  <el-icon><Message /></el-icon>
                </el-button>
                <el-button v-if="contact.phoneNumber" @click.stop="callContact(contact.phoneNumber)" text size="small">
                  <el-icon><Phone /></el-icon>
                </el-button>
              </div>
            </div>
            
            <div v-if="contacts.length === 0" class="text-center py-8 text-gray-500">
              <el-icon class="text-4xl mb-2"><User /></el-icon>
              <p>No contacts found</p>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Right Column - Sidebar -->
      <el-col :xs="24" :lg="8">
        <!-- Quick Stats -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Company Stats</span>
          </template>
          
          <div class="space-y-4">
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Total Contacts</span>
              <span class="font-semibold">{{ stats.contacts }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Active Opportunities</span>
              <span class="font-semibold">{{ stats.activeOpportunities }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Total Value</span>
              <span class="font-semibold text-green-600">${{ formatCurrency(stats.totalValue) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Won Opportunities</span>
              <span class="font-semibold">{{ stats.wonOpportunities }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Last Activity</span>
              <span class="font-semibold">{{ stats.lastActivity || 'Never' }}</span>
            </div>
          </div>
        </el-card>

        <!-- Opportunities -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="text-lg font-semibold">Opportunities</span>
              <el-link @click="addOpportunity" type="primary" :underline="false">
                <el-icon><Plus /></el-icon>
                Add
              </el-link>
            </div>
          </template>
          
          <div class="space-y-3">
            <div 
              v-for="opportunity in opportunities.slice(0, 5)" 
              :key="opportunity.id"
              class="p-3 border border-gray-100 rounded-lg hover:bg-gray-50 cursor-pointer"
              @click="viewOpportunity(opportunity.id)"
            >
              <div class="flex items-center justify-between mb-2">
                <h4 class="text-sm font-medium text-gray-900 truncate">{{ opportunity.title }}</h4>
                <el-tag :type="getStageTagType(opportunity.stage)" size="small">
                  {{ opportunity.stage }}
                </el-tag>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-lg font-bold text-green-600">
                  ${{ formatCurrency(opportunity.estimatedValue) }}
                </span>
                <span class="text-sm text-gray-500">
                  {{ opportunity.probabilityPercentage }}%
                </span>
              </div>
            </div>
            
            <div v-if="opportunities.length === 0" class="text-center py-4 text-gray-500">
              <el-icon class="text-4xl mb-2"><TrendCharts /></el-icon>
              <p class="text-sm">No opportunities</p>
            </div>
          </div>
        </el-card>

        <!-- Company Timeline -->
        <el-card shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Timeline</span>
          </template>
          
          <el-timeline>
            <el-timeline-item
              v-for="event in timeline"
              :key="event.id"
              :timestamp="formatDate(event.createdAt)"
              :type="getTimelineType(event.type)"
            >
              <h4 class="text-sm font-medium">{{ event.title }}</h4>
              <p class="text-sm text-gray-600">{{ event.description }}</p>
            </el-timeline-item>
          </el-timeline>
          
          <div v-if="timeline.length === 0" class="text-center py-4 text-gray-500">
            <p class="text-sm">No timeline events</p>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format } from 'date-fns'
import {
  ArrowLeft,
  OfficeBuilding,
  Link,
  Message,
  Edit,
  Check,
  MoreFilled,
  CopyDocument,
  Download,
  Delete,
  User,
  Plus,
  Phone,
  TrendCharts
} from '@element-plus/icons-vue'

// Router
const route = useRoute()
const router = useRouter()

// Reactive state
const loading = ref(true)
const saving = ref(false)
const editMode = ref(false)

const companyData = ref(null)
const originalData = ref(null)
const contacts = ref([])
const opportunities = ref([])
const timeline = ref([])

// Form data
const companyForm = reactive({
  name: '',
  industry: '',
  companySize: '',
  website: '',
  email: '',
  phoneNumber: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  stateProvince: '',
  postalCode: '',
  country: '',
  annualRevenue: null
})

// Stats
const stats = reactive({
  contacts: 0,
  activeOpportunities: 0,
  totalValue: 0,
  wonOpportunities: 0,
  lastActivity: ''
})

// Computed
const companyId = computed(() => route.params.id)

const hasAddress = computed(() => {
  return companyData.value?.addressLine1 || 
         companyData.value?.city || 
         companyData.value?.country
})

// Methods
const loadCompany = async () => {
  try {
    loading.value = true
    
    // API call would go here
    // const response = await companiesApi.getCompany(companyId.value)
    
    // Mock data
    const mockCompany = {
      id: companyId.value,
      name: 'Acme Corporation',
      industry: 'Technology',
      companySize: '51-200',
      website: 'https://acme.com',
      email: 'contact@acme.com',
      phoneNumber: '+1 (555) 123-4567',
      addressLine1: '123 Business St',
      addressLine2: 'Suite 100',
      city: 'San Francisco',
      stateProvince: 'CA',
      postalCode: '94105',
      country: 'United States',
      annualRevenue: 5000000,
      createdAt: '2024-01-15T10:00:00Z',
      updatedAt: '2024-03-20T14:30:00Z'
    }
    
    companyData.value = mockCompany
    originalData.value = { ...mockCompany }
    
    // Populate form
    Object.assign(companyForm, mockCompany)
    
    // Load related data
    await Promise.all([
      loadContacts(),
      loadOpportunities(),
      loadTimeline(),
      loadStats()
    ])
    
  } catch (error) {
    console.error('Error loading company:', error)
    ElMessage.error('Failed to load company details')
  } finally {
    loading.value = false
  }
}

const loadContacts = async () => {
  // Mock contacts
  contacts.value = [
    {
      id: 1,
      firstName: 'John',
      lastName: 'Smith',
      email: 'john.smith@acme.com',
      phoneNumber: '+1 (555) 123-4567',
      jobTitle: 'CTO'
    },
    {
      id: 2,
      firstName: 'Jane',
      lastName: 'Doe',
      email: 'jane.doe@acme.com',
      phoneNumber: '+1 (555) 987-6543',
      jobTitle: 'VP Engineering'
    }
  ]
}

const loadOpportunities = async () => {
  // Mock opportunities
  opportunities.value = [
    {
      id: 1,
      title: 'Enterprise Platform Upgrade',
      estimatedValue: 150000,
      probabilityPercentage: 75,
      stage: 'Proposal'
    }
  ]
}

const loadTimeline = async () => {
  // Mock timeline
  timeline.value = [
    {
      id: 1,
      title: 'Company Created',
      description: 'Company was added to the system',
      type: 'created',
      createdAt: '2024-01-15T10:00:00Z'
    }
  ]
}

const loadStats = async () => {
  stats.contacts = 12
  stats.activeOpportunities = 3
  stats.totalValue = 450000
  stats.wonOpportunities = 8
  stats.lastActivity = 'March 20, 2024'
}

const toggleEditMode = () => {
  editMode.value = !editMode.value
}

const cancelEdit = () => {
  editMode.value = false
  // Reset form to original data
  Object.assign(companyForm, originalData.value)
}

const saveCompany = async () => {
  try {
    saving.value = true
    
    // Validate required fields
    if (!companyForm.name) {
      ElMessage.error('Company name is required')
      return
    }
    
    // API call would go here
    // const response = await companiesApi.updateCompany(companyId.value, companyForm)
    
    // Update local data
    Object.assign(companyData.value, companyForm)
    originalData.value = { ...companyData.value }
    
    ElMessage.success('Company updated successfully')
    editMode.value = false
    
  } catch (error) {
    console.error('Error saving company:', error)
    ElMessage.error('Failed to save company')
  } finally {
    saving.value = false
  }
}

const deleteCompany = async () => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this company? This action cannot be undone and will affect all related contacts and opportunities.',
      'Delete Company',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    // API call would go here
    // await companiesApi.deleteCompany(companyId.value)
    
    ElMessage.success('Company deleted successfully')
    router.push('/companies')
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting company:', error)
      ElMessage.error('Failed to delete company')
    }
  }
}

const duplicateCompany = () => {
  router.push(`/companies/new?duplicate=${companyId.value}`)
}

const exportCompany = () => {
  ElMessage.info('Export functionality will be implemented')
}

const goBack = () => {
  router.back()
}

const visitWebsite = () => {
  if (companyData.value?.website) {
    window.open(companyData.value.website, '_blank')
  }
}

const sendEmail = () => {
  if (companyData.value?.email) {
    window.open(`mailto:${companyData.value.email}`)
  }
}

const sendContactEmail = (email) => {
  window.open(`mailto:${email}`)
}

const callContact = (phoneNumber) => {
  window.open(`tel:${phoneNumber}`)
}

const addContact = () => {
  router.push(`/contacts/new?companyId=${companyId.value}`)
}

const addOpportunity = () => {
  router.push(`/opportunities/new?companyId=${companyId.value}`)
}

const viewAllContacts = () => {
  router.push(`/contacts?companyId=${companyId.value}`)
}

const viewContact = (contactId) => {
  router.push(`/contacts/${contactId}`)
}

const viewOpportunity = (opportunityId) => {
  router.push(`/opportunities/${opportunityId}`)
}

// Helper functions
const formatWebsite = (website) => {
  return website?.replace(/^https?:\/\//, '')
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-US', {
    notation: 'compact',
    compactDisplay: 'short'
  }).format(amount)
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

const getTimelineType = (type) => {
  const types = {
    created: 'primary',
    updated: 'success',
    deleted: 'danger'
  }
  return types[type] || 'primary'
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

// Lifecycle
onMounted(() => {
  loadCompany()
})

// Watch for route changes
watch(() => route.params.id, (newId) => {
  if (newId) {
    loadCompany()
  }
}, { immediate: true })
</script>

<style scoped>
.company-detail {
  min-height: calc(100vh - 200px);
}

.grid {
  display: grid;
  gap: 1.5rem;
}

.grid-cols-1 {
  grid-template-columns: repeat(1, minmax(0, 1fr));
}

@media (min-width: 1024px) {
  .lg\:grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
  
  .lg\:col-span-2 {
    grid-column: span 2;
  }
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .company-detail {
    padding: 0 1rem;
  }
  
  .company-detail :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>