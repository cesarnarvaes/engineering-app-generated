<template>
  <div class="contact-detail" v-loading="loading">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <el-button @click="goBack" text class="mr-4">
          <el-icon><ArrowLeft /></el-icon>
          Back
        </el-button>
        <div>
          <h1 class="text-2xl font-semibold text-gray-900">
            {{ contactData?.firstName }} {{ contactData?.lastName }}
          </h1>
          <p class="text-gray-600 mt-1">{{ contactData?.jobTitle }} at {{ contactData?.company?.name }}</p>
        </div>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Action Buttons -->
        <el-button @click="sendEmail" :disabled="!contactData?.email">
          <el-icon><Message /></el-icon>
          Email
        </el-button>
        <el-button @click="makeCall" :disabled="!contactData?.phoneNumber">
          <el-icon><Phone /></el-icon>
          Call
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
            @click="saveContact"
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
              <el-dropdown-item @click="duplicateContact">
                <el-icon><CopyDocument /></el-icon>
                Duplicate
              </el-dropdown-item>
              <el-dropdown-item @click="exportContact">
                <el-icon><Download /></el-icon>
                Export
              </el-dropdown-item>
              <el-dropdown-item divided @click="deleteContact" class="text-red-600">
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
      <!-- Left Column - Contact Information -->
      <el-col :xs="24" :lg="16">
        <!-- Contact Details Card -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center">
              <el-icon class="mr-2 text-blue-600">
                <User />
              </el-icon>
              <span class="text-lg font-semibold">Contact Information</span>
            </div>
          </template>
          
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <!-- Avatar -->
            <div class="lg:col-span-2 flex items-center">
              <el-avatar :size="80" :src="contactForm.avatar" class="mr-4">
                {{ getInitials(contactForm.firstName, contactForm.lastName) }}
              </el-avatar>
              <div v-if="editMode">
                <el-upload
                  :show-file-list="false"
                  :before-upload="beforeAvatarUpload"
                  action=""
                >
                  <el-button size="small">
                    <el-icon><Upload /></el-icon>
                    Change Photo
                  </el-button>
                </el-upload>
              </div>
            </div>

            <!-- Personal Information -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">First Name</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.firstName" 
                placeholder="Enter first name"
              />
              <p v-else class="text-gray-900">{{ contactData?.firstName || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.lastName" 
                placeholder="Enter last name"
              />
              <p v-else class="text-gray-900">{{ contactData?.lastName || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.email" 
                placeholder="Enter email"
                type="email"
              />
              <div v-else>
                <el-link v-if="contactData?.email" :href="`mailto:${contactData.email}`" type="primary">
                  {{ contactData.email }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Phone</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.phoneNumber" 
                placeholder="Enter phone number"
              />
              <div v-else>
                <el-link v-if="contactData?.phoneNumber" :href="`tel:${contactData.phoneNumber}`" type="primary">
                  {{ contactData.phoneNumber }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Job Title</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.jobTitle" 
                placeholder="Enter job title"
              />
              <p v-else class="text-gray-900">{{ contactData?.jobTitle || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Department</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.department" 
                placeholder="Enter department"
              />
              <p v-else class="text-gray-900">{{ contactData?.department || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Company</label>
              <el-select
                v-if="editMode"
                v-model="contactForm.companyId"
                placeholder="Select company"
                filterable
                remote
                clearable
                class="w-full"
                :remote-method="searchCompanies"
                :loading="companiesLoading"
              >
                <el-option
                  v-for="company in companies"
                  :key="company.id"
                  :label="company.name"
                  :value="company.id"
                />
              </el-select>
              <div v-else>
                <el-link v-if="contactData?.company" @click="viewCompany(contactData.company.id)" type="primary">
                  {{ contactData.company.name }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <el-select 
                v-if="editMode"
                v-model="contactForm.status" 
                placeholder="Select status"
                class="w-full"
              >
                <el-option label="Active" value="Active" />
                <el-option label="Inactive" value="Inactive" />
                <el-option label="Lead" value="Lead" />
              </el-select>
              <el-tag v-else :type="getStatusType(contactData?.status)" size="small">
                {{ contactData?.status }}
              </el-tag>
            </div>

            <!-- Notes -->
            <div class="lg:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Notes</label>
              <el-input 
                v-if="editMode"
                v-model="contactForm.notes" 
                type="textarea"
                :rows="3"
                placeholder="Add notes about this contact"
              />
              <p v-else class="text-gray-900">{{ contactData?.notes || '-' }}</p>
            </div>
          </div>
        </el-card>

        <!-- Activities -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-green-600">
                  <Clock />
                </el-icon>
                <span class="text-lg font-semibold">Recent Activities</span>
              </div>
              <el-button @click="createActivity" size="small" type="primary">
                <el-icon><Plus /></el-icon>
                Add Activity
              </el-button>
            </div>
          </template>
          
          <div class="space-y-4">
            <div 
              v-for="activity in activities" 
              :key="activity.id"
              class="flex items-start p-4 border border-gray-100 rounded-lg hover:bg-gray-50"
            >
              <el-avatar :size="32" class="mr-3">
                <component :is="getActivityIcon(activity.type)" />
              </el-avatar>
              <div class="flex-1">
                <h4 class="text-sm font-medium text-gray-900">{{ activity.title }}</h4>
                <p class="text-sm text-gray-600 mt-1">{{ activity.description }}</p>
                <div class="flex items-center mt-2">
                  <el-tag :type="getActivityTagType(activity.type)" size="small" class="mr-2">
                    {{ activity.type }}
                  </el-tag>
                  <span class="text-xs text-gray-500">{{ formatDate(activity.createdAt) }}</span>
                </div>
              </div>
              <el-button text size="small" @click="viewActivity(activity.id)">
                View
              </el-button>
            </div>
            
            <div v-if="activities.length === 0" class="text-center py-8 text-gray-500">
              <el-icon class="text-4xl mb-2"><Calendar /></el-icon>
              <p>No activities recorded</p>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Right Column - Sidebar -->
      <el-col :xs="24" :lg="8">
        <!-- Quick Stats -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Stats</span>
          </template>
          
          <div class="space-y-4">
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Total Opportunities</span>
              <span class="font-semibold">{{ stats.opportunities }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Total Value</span>
              <span class="font-semibold text-green-600">${{ formatCurrency(stats.totalValue) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Last Contact</span>
              <span class="font-semibold">{{ stats.lastContact || 'Never' }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Activities</span>
              <span class="font-semibold">{{ stats.activities }}</span>
            </div>
          </div>
        </el-card>

        <!-- Related Opportunities -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="text-lg font-semibold">Opportunities</span>
              <el-link @click="createOpportunity" type="primary" :underline="false">
                <el-icon><Plus /></el-icon>
                Add
              </el-link>
            </div>
          </template>
          
          <div class="space-y-3">
            <div 
              v-for="opportunity in opportunities" 
              :key="opportunity.id"
              class="p-3 border border-gray-100 rounded-lg hover:bg-gray-50 cursor-pointer"
              @click="viewOpportunity(opportunity.id)"
            >
              <h4 class="text-sm font-medium text-gray-900 mb-1">{{ opportunity.title }}</h4>
              <div class="flex items-center justify-between">
                <span class="text-lg font-bold text-green-600">
                  ${{ formatCurrency(opportunity.estimatedValue) }}
                </span>
                <el-tag :type="getStageTagType(opportunity.stage)" size="small">
                  {{ opportunity.stage }}
                </el-tag>
              </div>
            </div>
            
            <div v-if="opportunities.length === 0" class="text-center py-4 text-gray-500">
              <p class="text-sm">No opportunities</p>
            </div>
          </div>
        </el-card>

        <!-- Contact Timeline -->
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
  User,
  Message,
  Phone,
  Edit,
  Check,
  MoreFilled,
  CopyDocument,
  Download,
  Delete,
  Upload,
  Clock,
  Plus,
  Calendar
} from '@element-plus/icons-vue'

// Router
const route = useRoute()
const router = useRouter()

// Reactive state
const loading = ref(true)
const saving = ref(false)
const editMode = ref(false)
const companiesLoading = ref(false)

const contactData = ref(null)
const originalData = ref(null)
const companies = ref([])
const activities = ref([])
const opportunities = ref([])
const timeline = ref([])

// Form data
const contactForm = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  jobTitle: '',
  department: '',
  companyId: null,
  status: 'Active',
  notes: '',
  avatar: ''
})

// Stats
const stats = reactive({
  opportunities: 0,
  totalValue: 0,
  lastContact: '',
  activities: 0
})

// Computed
const contactId = computed(() => route.params.id)

// Methods
const loadContact = async () => {
  try {
    loading.value = true
    
    // API call would go here
    // const response = await contactsApi.getContact(contactId.value)
    
    // Mock data
    const mockContact = {
      id: contactId.value,
      firstName: 'John',
      lastName: 'Smith',
      email: 'john.smith@example.com',
      phoneNumber: '+1 (555) 123-4567',
      jobTitle: 'Software Engineer',
      department: 'Engineering',
      status: 'Active',
      notes: 'Key decision maker for technical implementations.',
      company: { id: 1, name: 'Acme Corp' },
      createdAt: '2024-01-15T10:00:00Z',
      updatedAt: '2024-03-20T14:30:00Z'
    }
    
    contactData.value = mockContact
    originalData.value = { ...mockContact }
    
    // Populate form
    Object.assign(contactForm, {
      firstName: mockContact.firstName,
      lastName: mockContact.lastName,
      email: mockContact.email,
      phoneNumber: mockContact.phoneNumber,
      jobTitle: mockContact.jobTitle,
      department: mockContact.department,
      companyId: mockContact.company?.id,
      status: mockContact.status,
      notes: mockContact.notes,
      avatar: mockContact.avatar
    })
    
    // Load related data
    await Promise.all([
      loadActivities(),
      loadOpportunities(),
      loadTimeline(),
      loadStats()
    ])
    
  } catch (error) {
    console.error('Error loading contact:', error)
    ElMessage.error('Failed to load contact details')
  } finally {
    loading.value = false
  }
}

const loadActivities = async () => {
  // Mock activities
  activities.value = [
    {
      id: 1,
      title: 'Follow-up call scheduled',
      description: 'Discussed project requirements and next steps',
      type: 'call',
      createdAt: '2024-03-20T10:00:00Z'
    }
  ]
}

const loadOpportunities = async () => {
  // Mock opportunities
  opportunities.value = [
    {
      id: 1,
      title: 'Website Redesign',
      estimatedValue: 50000,
      stage: 'Proposal',
      createdAt: '2024-03-15T09:00:00Z'
    }
  ]
}

const loadTimeline = async () => {
  // Mock timeline
  timeline.value = [
    {
      id: 1,
      title: 'Contact Created',
      description: 'Contact was added to the system',
      type: 'created',
      createdAt: '2024-01-15T10:00:00Z'
    }
  ]
}

const loadStats = async () => {
  stats.opportunities = 3
  stats.totalValue = 125000
  stats.lastContact = 'March 20, 2024'
  stats.activities = 12
}

const searchCompanies = async (query) => {
  if (!query) return
  
  companiesLoading.value = true
  
  try {
    // Mock companies
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

const toggleEditMode = () => {
  editMode.value = !editMode.value
}

const cancelEdit = () => {
  editMode.value = false
  // Reset form to original data
  Object.assign(contactForm, originalData.value)
}

const saveContact = async () => {
  try {
    saving.value = true
    
    // Validate required fields
    if (!contactForm.firstName || !contactForm.lastName) {
      ElMessage.error('First name and last name are required')
      return
    }
    
    // API call would go here
    // const response = await contactsApi.updateContact(contactId.value, contactForm)
    
    // Update local data
    Object.assign(contactData.value, contactForm)
    originalData.value = { ...contactData.value }
    
    ElMessage.success('Contact updated successfully')
    editMode.value = false
    
  } catch (error) {
    console.error('Error saving contact:', error)
    ElMessage.error('Failed to save contact')
  } finally {
    saving.value = false
  }
}

const deleteContact = async () => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this contact? This action cannot be undone.',
      'Delete Contact',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    // API call would go here
    // await contactsApi.deleteContact(contactId.value)
    
    ElMessage.success('Contact deleted successfully')
    router.push('/contacts')
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting contact:', error)
      ElMessage.error('Failed to delete contact')
    }
  }
}

const duplicateContact = () => {
  router.push(`/contacts/new?duplicate=${contactId.value}`)
}

const exportContact = () => {
  ElMessage.info('Export functionality will be implemented')
}

const goBack = () => {
  router.back()
}

const sendEmail = () => {
  if (contactData.value?.email) {
    window.open(`mailto:${contactData.value.email}`)
  }
}

const makeCall = () => {
  if (contactData.value?.phoneNumber) {
    window.open(`tel:${contactData.value.phoneNumber}`)
  }
}

const beforeAvatarUpload = (file) => {
  const isJPG = file.type === 'image/jpeg' || file.type === 'image/png'
  const isLt2M = file.size / 1024 / 1024 < 2
  
  if (!isJPG) {
    ElMessage.error('Avatar picture must be JPG or PNG format!')
  }
  if (!isLt2M) {
    ElMessage.error('Avatar picture size cannot exceed 2MB!')
  }
  
  return isJPG && isLt2M
}

const createActivity = () => {
  router.push(`/activities/new?contactId=${contactId.value}`)
}

const createOpportunity = () => {
  router.push(`/opportunities/new?contactId=${contactId.value}`)
}

const viewActivity = (activityId) => {
  router.push(`/activities/${activityId}`)
}

const viewOpportunity = (opportunityId) => {
  router.push(`/opportunities/${opportunityId}`)
}

const viewCompany = (companyId) => {
  router.push(`/companies/${companyId}`)
}

// Helper functions
const getInitials = (firstName, lastName) => {
  return `${firstName?.charAt(0) || ''}${lastName?.charAt(0) || ''}`
}

const getStatusType = (status) => {
  const types = {
    'Active': 'success',
    'Inactive': 'info',
    'Lead': 'warning'
  }
  return types[status] || 'info'
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

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-US').format(amount)
}

// Lifecycle
onMounted(() => {
  loadContact()
})

// Watch for route changes
watch(() => route.params.id, (newId) => {
  if (newId) {
    loadContact()
  }
}, { immediate: true })
</script>

<style scoped>
.contact-detail {
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
  .contact-detail {
    padding: 0 1rem;
  }
  
  .contact-detail :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>